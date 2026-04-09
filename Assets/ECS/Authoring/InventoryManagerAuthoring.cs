using Unity.Entities;
using UnityEngine;
using RPG.ECS.Inventory;

namespace RPG.ECS.Authoring
{
    /// <summary>
    /// Authoring component for converting inventory-related MonoBehaviours
    /// to ECS components. Mirrors DevionGames.InventorySystem.InventoryManager
    /// and ItemContainer/ItemCollection setup.
    ///
    /// Attach this to a GameObject to bake an inventory container entity
    /// with item buffers and configuration.
    /// </summary>
    public class InventoryManagerAuthoring : MonoBehaviour
    {
        [Tooltip("Name of this inventory container/window.")]
        public string containerName = "Inventory";

        [Tooltip("Maximum number of item slots.")]
        public int maxSlots = 20;

        [Tooltip("Whether this collection should be saved/loaded.")]
        public bool saveable = true;

        [Header("Container Permissions")]
        public bool useReferences = false;
        public bool canDragIn = true;
        public bool canDragOut = true;
        public bool canDropItems = true;
        public bool canSellItems = true;
        public bool canUseItems = true;

        [Header("Starting Items")]
        [Tooltip("Items to add to the inventory at startup.")]
        public StartingItem[] startingItems;

        [System.Serializable]
        public struct StartingItem
        {
            public string itemName;
            public ItemType type;
            public int stack;
            public int maxStack;
            public int buyPrice;
            public int sellPrice;
            public float cooldown;
            public bool isSellable;
            public bool isDroppable;
        }

        public class Baker : Baker<InventoryManagerAuthoring>
        {
            public override void Bake(InventoryManagerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new InventoryContainerTag
                {
                    NameHash = authoring.containerName.GetHashCode(),
                    MaxSlots = authoring.maxSlots,
                    UseReferences = authoring.useReferences,
                    CanDragIn = authoring.canDragIn,
                    CanDragOut = authoring.canDragOut,
                    CanDropItems = authoring.canDropItems,
                    CanSellItems = authoring.canSellItems,
                    CanUseItems = authoring.canUseItems
                });

                AddComponent(entity, new ItemCollectionTag
                {
                    Saveable = authoring.saveable
                });

                var itemBuffer = AddBuffer<ItemElement>(entity);

                if (authoring.startingItems != null)
                {
                    foreach (var startItem in authoring.startingItems)
                    {
                        itemBuffer.Add(new ItemElement
                        {
                            NameHash = startItem.itemName.GetHashCode(),
                            Type = startItem.type,
                            Stack = startItem.stack,
                            MaxStack = startItem.maxStack > 0 ? startItem.maxStack : 1,
                            BuyPrice = startItem.buyPrice,
                            SellPrice = startItem.sellPrice,
                            Cooldown = startItem.cooldown,
                            IsSellable = startItem.isSellable,
                            IsDroppable = startItem.isDroppable
                        });
                    }
                }
            }
        }
    }
}
