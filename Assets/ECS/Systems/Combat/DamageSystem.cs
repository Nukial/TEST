using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using RPG.ECS.Stats;

namespace RPG.ECS.Combat
{
    /// <summary>
    /// ECS equivalent of the damage flow from DevionGames.StatSystem.StatsHandler
    /// (ApplyDamage, SendDamage) and DamageData configuration.
    ///
    /// Processes DamageRequest components by:
    /// 1. Looking up the sender's damage stat value
    /// 2. Checking for critical strikes
    /// 3. Subtracting from the receiver's health attribute
    /// 4. Producing a DamageResult component on the receiver
    /// 5. Marking the request as processed for cleanup
    /// </summary>
    [BurstCompile]
    [UpdateAfter(typeof(StatCalculationSystem))]
    public partial struct DamageSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DamageRequest>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (request, entity) in
                SystemAPI.Query<RefRO<DamageRequest>>().WithEntityAccess())
            {
                var req = request.ValueRO;
                float damageValue = 0f;
                bool isCritical = false;

                // Get damage value from sender's stats
                if (SystemAPI.HasBuffer<StatElement>(req.Sender))
                {
                    var senderStats = SystemAPI.GetBuffer<StatElement>(req.Sender);
                    for (int i = 0; i < senderStats.Length; i++)
                    {
                        if (senderStats[i].NameHash == req.SendingStatHash)
                        {
                            damageValue = senderStats[i].Value;
                            break;
                        }
                    }

                    // Check for critical strike
                    if (req.CriticalStrikeStatHash != 0)
                    {
                        for (int i = 0; i < senderStats.Length; i++)
                        {
                            if (senderStats[i].NameHash == req.CriticalStrikeStatHash)
                            {
                                float critChance = senderStats[i].Value;
                                // Deterministic crit check using entity index as seed
                                var rng = Unity.Mathematics.Random.CreateFromIndex(
                                    (uint)(entity.Index + SystemAPI.Time.ElapsedTime * 1000));
                                if (rng.NextFloat(0f, 100f) < critChance)
                                {
                                    damageValue *= 2f;
                                    isCritical = true;
                                }
                                break;
                            }
                        }
                    }
                }

                // Apply damage to receiver's attribute
                if (SystemAPI.HasBuffer<AttributeElement>(req.Receiver))
                {
                    var receiverAttrs = SystemAPI.GetBuffer<AttributeElement>(req.Receiver);
                    for (int a = 0; a < receiverAttrs.Length; a++)
                    {
                        if (receiverAttrs[a].StatNameHash == req.ReceivingStatHash)
                        {
                            var attr = receiverAttrs[a];
                            attr.CurrentValue -= damageValue;
                            if (attr.CurrentValue < 0f)
                                attr.CurrentValue = 0f;
                            receiverAttrs[a] = attr;
                            break;
                        }
                    }
                }

                // Calculate knockback direction
                float3 knockbackDir = float3.zero;
                float knockbackForce = 0f;
                if (req.EnableKnockback)
                {
                    if (SystemAPI.HasComponent<Unity.Transforms.LocalTransform>(req.Sender) &&
                        SystemAPI.HasComponent<Unity.Transforms.LocalTransform>(req.Receiver))
                    {
                        var senderPos = SystemAPI.GetComponent<Unity.Transforms.LocalTransform>(req.Sender).Position;
                        var receiverPos = SystemAPI.GetComponent<Unity.Transforms.LocalTransform>(req.Receiver).Position;
                        knockbackDir = math.normalizesafe(receiverPos - senderPos);
                        knockbackForce = req.KnockbackStrength;
                    }
                }

                // Create DamageResult on receiver
                ecb.AddComponent(req.Receiver, new DamageResult
                {
                    Sender = req.Sender,
                    DamageAmount = damageValue,
                    IsCritical = isCritical,
                    KnockbackDirection = knockbackDir,
                    KnockbackForce = knockbackForce
                });

                // Mark request for cleanup
                ecb.AddComponent<DamageProcessedTag>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Cleanup system that removes processed damage requests and
    /// one-frame DamageResult components.
    /// </summary>
    [UpdateAfter(typeof(DamageSystem))]
    public partial struct DamageCleanupSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            // Remove processed damage request entities
            foreach (var (_, entity) in
                SystemAPI.Query<RefRO<DamageProcessedTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }

            // Remove one-frame DamageResult components
            foreach (var (_, entity) in
                SystemAPI.Query<RefRO<DamageResult>>().WithEntityAccess())
            {
                ecb.RemoveComponent<DamageResult>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
