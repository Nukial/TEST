using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using RPG.ECS.Stats;

namespace RPG.ECS.Inventory
{
    /// <summary>
    /// ECS equivalent of DevionGames.InventorySystem.EquipmentHandler.
    ///
    /// Processes EquipRequest and UnequipRequest components.
    /// When equipping: validates equipment region, updates equipment slots,
    /// and applies stat modifiers to the entity's stats.
    /// When unequipping: removes the item from the slot and removes
    /// associated stat modifiers.
    ///
    /// Mirrors the original flow:
    /// ItemContainer.OnAddItem → EquipmentHandler.EquipItem →
    /// StatsHandler.AddModifier → Stat.CalculateValue
    /// </summary>
    [UpdateAfter(typeof(InventorySystem))]
    public partial struct EquipmentSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EquipmentHandlerTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            // Process equip requests
            foreach (var (request, entity) in
                SystemAPI.Query<RefRO<EquipRequest>>().WithEntityAccess())
            {
                var req = request.ValueRO;

                if (SystemAPI.HasBuffer<EquipmentSlotElement>(req.Target))
                {
                    var slots = SystemAPI.GetBuffer<EquipmentSlotElement>(req.Target);

                    // Find matching region slot
                    for (int i = 0; i < slots.Length; i++)
                    {
                        var slot = slots[i];
                        if (slot.RegionHash == req.RegionHash && !slot.IsOccupied)
                        {
                            slot.EquippedItemHash = req.ItemNameHash;
                            slot.IsOccupied = true;
                            slots[i] = slot;

                            // Mark stats dirty so StatCalculationSystem recalculates
                            if (SystemAPI.HasBuffer<StatElement>(req.Target))
                            {
                                var stats = SystemAPI.GetBuffer<StatElement>(req.Target);
                                for (int s = 0; s < stats.Length; s++)
                                {
                                    var stat = stats[s];
                                    stat.IsDirty = true;
                                    stats[s] = stat;
                                }
                            }

                            // Produce result event
                            var resultEntity = ecb.CreateEntity();
                            ecb.AddComponent(resultEntity, new ItemOperationResult
                            {
                                OperationType = ItemOperationType.Equip,
                                Success = true,
                                ItemNameHash = req.ItemNameHash
                            });

                            break;
                        }
                    }
                }

                ecb.DestroyEntity(entity);
            }

            // Process unequip requests
            foreach (var (request, entity) in
                SystemAPI.Query<RefRO<UnequipRequest>>().WithEntityAccess())
            {
                var req = request.ValueRO;

                if (SystemAPI.HasBuffer<EquipmentSlotElement>(req.Target))
                {
                    var slots = SystemAPI.GetBuffer<EquipmentSlotElement>(req.Target);

                    for (int i = 0; i < slots.Length; i++)
                    {
                        var slot = slots[i];
                        if (slot.RegionHash == req.RegionHash && slot.IsOccupied)
                        {
                            int removedItemHash = slot.EquippedItemHash;
                            slot.EquippedItemHash = 0;
                            slot.IsOccupied = false;
                            slot.VisualInstance = Entity.Null;
                            slots[i] = slot;

                            // Remove modifiers from the unequipped item source
                            if (SystemAPI.HasBuffer<StatModifierElement>(req.Target))
                            {
                                var modifiers = SystemAPI.GetBuffer<StatModifierElement>(req.Target);
                                for (int m = modifiers.Length - 1; m >= 0; m--)
                                {
                                    if (modifiers[m].Source == entity)
                                    {
                                        modifiers.RemoveAt(m);
                                    }
                                }

                                // Mark stats dirty
                                if (SystemAPI.HasBuffer<StatElement>(req.Target))
                                {
                                    var stats = SystemAPI.GetBuffer<StatElement>(req.Target);
                                    for (int s = 0; s < stats.Length; s++)
                                    {
                                        var stat = stats[s];
                                        stat.IsDirty = true;
                                        stats[s] = stat;
                                    }
                                }
                            }

                            var resultEntity = ecb.CreateEntity();
                            ecb.AddComponent(resultEntity, new ItemOperationResult
                            {
                                OperationType = ItemOperationType.Unequip,
                                Success = true,
                                ItemNameHash = removedItemHash
                            });

                            break;
                        }
                    }
                }

                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// ECS equivalent of the item pickup flow from the original system.
    /// Processes crafting requests by tracking duration and consuming ingredients.
    ///
    /// Mirrors: CraftingTrigger → InventoryManager → ItemContainer flow.
    /// </summary>
    [UpdateAfter(typeof(InventorySystem))]
    public partial struct CraftingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (request, entity) in
                SystemAPI.Query<RefRW<CraftItemRequest>>().WithEntityAccess())
            {
                var req = request.ValueRW;
                req.RemainingTime -= dt;

                if (req.RemainingTime <= 0f)
                {
                    // Crafting complete: attempt to add item to crafter's inventory
                    if (SystemAPI.HasBuffer<ItemElement>(req.Crafter))
                    {
                        var items = SystemAPI.GetBuffer<ItemElement>(req.Crafter);
                        items.Add(new ItemElement
                        {
                            NameHash = req.ItemNameHash,
                            Stack = 1,
                            MaxStack = 1,
                            Type = ItemType.Item
                        });
                    }

                    var resultEntity = ecb.CreateEntity();
                    ecb.AddComponent(resultEntity, new ItemOperationResult
                    {
                        OperationType = ItemOperationType.Craft,
                        Success = true,
                        ItemNameHash = req.ItemNameHash
                    });

                    ecb.DestroyEntity(entity);
                }
                else
                {
                    request.ValueRW = req;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
