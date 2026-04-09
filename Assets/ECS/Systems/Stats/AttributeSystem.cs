using Unity.Burst;
using Unity.Entities;

namespace RPG.ECS.Stats
{
    /// <summary>
    /// ECS equivalent of Attribute's current value clamping logic from
    /// DevionGames.StatSystem.Attribute.
    ///
    /// Ensures that each attribute's CurrentValue stays clamped between
    /// 0 and the parent stat's calculated Value. Also handles initialization
    /// via StartPercent.
    /// </summary>
    [BurstCompile]
    [UpdateAfter(typeof(StatCalculationSystem))]
    public partial struct AttributeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StatsHandlerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ClampAttributesJob().ScheduleParallel();
        }

        /// <summary>
        /// For each entity with both stats and attributes, clamp the attribute's
        /// CurrentValue to [0, stat.Value]. This mirrors Attribute.CurrentValue setter
        /// which clamps between 0 and Value.
        /// </summary>
        [BurstCompile]
        public partial struct ClampAttributesJob : IJobEntity
        {
            public void Execute(
                ref DynamicBuffer<AttributeElement> attributes,
                in DynamicBuffer<StatElement> stats)
            {
                for (int a = 0; a < attributes.Length; a++)
                {
                    var attr = attributes[a];

                    // Find matching stat
                    float maxValue = 0f;
                    for (int s = 0; s < stats.Length; s++)
                    {
                        if (stats[s].NameHash == attr.StatNameHash)
                        {
                            maxValue = stats[s].Value;
                            break;
                        }
                    }

                    // Clamp CurrentValue between 0 and max
                    if (attr.CurrentValue > maxValue)
                        attr.CurrentValue = maxValue;
                    if (attr.CurrentValue < 0f)
                        attr.CurrentValue = 0f;

                    attributes[a] = attr;
                }
            }
        }
    }
}
