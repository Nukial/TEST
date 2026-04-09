using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace RPG.ECS.Inventory
{
    /// <summary>
    /// ECS equivalent of ItemContainer.AddItem() / RemoveItem() logic from
    /// DevionGames.InventorySystem.ItemContainer.
    ///
    /// Processes AddItemRequest and RemoveItemRequest components by modifying
    /// the target entity's ItemElement buffer. Handles stacking, slot limits,
    /// and produces ItemOperationResult events.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct InventorySystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            // Process add requests
            foreach (var (request, entity) in
                SystemAPI.Query<RefRO<AddItemRequest>>().WithEntityAccess())
            {
                var req = request.ValueRO;
                bool success = false;

                if (SystemAPI.HasBuffer<ItemElement>(req.TargetContainer) &&
                    SystemAPI.HasComponent<InventoryContainerTag>(req.TargetContainer))
                {
                    var container = SystemAPI.GetComponent<InventoryContainerTag>(req.TargetContainer);
                    var items = SystemAPI.GetBuffer<ItemElement>(req.TargetContainer);

                    // Try to stack with existing item first
                    for (int i = 0; i < items.Length; i++)
                    {
                        var item = items[i];
                        if (item.NameHash == req.ItemNameHash && item.Stack < item.MaxStack)
                        {
                            int canAdd = item.MaxStack - item.Stack;
                            int toAdd = math.min(req.Amount, canAdd);
                            item.Stack += toAdd;
                            items[i] = item;
                            success = true;
                            break;
                        }
                    }

                    // If couldn't stack, add new entry if there's room
                    if (!success && items.Length < container.MaxSlots)
                    {
                        int maxStack = req.MaxStack > 0 ? req.MaxStack : 1;
                        items.Add(new ItemElement
                        {
                            NameHash = req.ItemNameHash,
                            Stack = req.Amount,
                            MaxStack = maxStack,
                            Type = req.Type
                        });
                        success = true;
                    }
                }

                // Produce result event
                var resultEntity = ecb.CreateEntity();
                ecb.AddComponent(resultEntity, new ItemOperationResult
                {
                    OperationType = ItemOperationType.Add,
                    Success = success,
                    ItemNameHash = req.ItemNameHash
                });

                ecb.DestroyEntity(entity);
            }

            // Process remove requests
            foreach (var (request, entity) in
                SystemAPI.Query<RefRO<RemoveItemRequest>>().WithEntityAccess())
            {
                var req = request.ValueRO;
                bool success = false;

                if (SystemAPI.HasBuffer<ItemElement>(req.TargetContainer))
                {
                    var items = SystemAPI.GetBuffer<ItemElement>(req.TargetContainer);

                    for (int i = 0; i < items.Length; i++)
                    {
                        var item = items[i];
                        if (item.NameHash == req.ItemNameHash)
                        {
                            item.Stack -= req.Amount;
                            if (item.Stack <= 0)
                            {
                                items.RemoveAt(i);
                            }
                            else
                            {
                                items[i] = item;
                            }
                            success = true;
                            break;
                        }
                    }
                }

                var resultEntity = ecb.CreateEntity();
                ecb.AddComponent(resultEntity, new ItemOperationResult
                {
                    OperationType = ItemOperationType.Remove,
                    Success = success,
                    ItemNameHash = req.ItemNameHash
                });

                ecb.DestroyEntity(entity);
            }

            // Process use requests
            foreach (var (request, entity) in
                SystemAPI.Query<RefRO<UseItemRequest>>().WithEntityAccess())
            {
                var req = request.ValueRO;
                bool success = false;

                // Find the item in the user's inventory
                if (SystemAPI.HasBuffer<ItemElement>(req.User))
                {
                    var items = SystemAPI.GetBuffer<ItemElement>(req.User);

                    for (int i = 0; i < items.Length; i++)
                    {
                        var item = items[i];
                        if (item.NameHash == req.ItemNameHash &&
                            (item.Type == ItemType.UsableItem ||
                             item.Type == ItemType.EquipmentItem ||
                             item.Type == ItemType.Skill))
                        {
                            // Check cooldown
                            if (item.CooldownRemaining <= 0f)
                            {
                                item.CooldownRemaining = item.Cooldown;
                                items[i] = item;
                                success = true;
                            }
                            break;
                        }
                    }
                }

                var resultEntity = ecb.CreateEntity();
                ecb.AddComponent(resultEntity, new ItemOperationResult
                {
                    OperationType = ItemOperationType.Use,
                    Success = success,
                    ItemNameHash = req.ItemNameHash
                });

                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Ticks item cooldowns each frame, mirroring UsableItem cooldown behavior.
    /// </summary>
    [BurstCompile]
    public partial struct ItemCooldownSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            new TickCooldownsJob { DeltaTime = dt }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct TickCooldownsJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(ref DynamicBuffer<ItemElement> items)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    if (item.CooldownRemaining > 0f)
                    {
                        item.CooldownRemaining -= DeltaTime;
                        if (item.CooldownRemaining < 0f)
                            item.CooldownRemaining = 0f;
                        items[i] = item;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Cleans up one-frame ItemOperationResult entities.
    /// </summary>
    [UpdateAfter(typeof(InventorySystem))]
    public partial struct InventoryCleanupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (_, entity) in
                SystemAPI.Query<RefRO<ItemOperationResult>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
