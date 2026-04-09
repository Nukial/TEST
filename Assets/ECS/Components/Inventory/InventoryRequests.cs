using Unity.Entities;

namespace RPG.ECS.Inventory
{
    /// <summary>
    /// Request to add an item to an inventory container.
    /// ECS equivalent of ItemContainer.AddItem() call.
    /// </summary>
    public struct AddItemRequest : IComponentData
    {
        /// <summary>Target inventory entity.</summary>
        public Entity TargetContainer;

        /// <summary>Hash of the item to add.</summary>
        public int ItemNameHash;

        /// <summary>Amount to add.</summary>
        public int Amount;

        /// <summary>Maximum stack size for the item. Defaults to 1 if not set.</summary>
        public int MaxStack;

        /// <summary>Item type.</summary>
        public ItemType Type;
    }

    /// <summary>
    /// Request to remove an item from an inventory container.
    /// ECS equivalent of ItemContainer.RemoveItem() call.
    /// </summary>
    public struct RemoveItemRequest : IComponentData
    {
        /// <summary>Target inventory entity.</summary>
        public Entity TargetContainer;

        /// <summary>Hash of the item to remove.</summary>
        public int ItemNameHash;

        /// <summary>Amount to remove.</summary>
        public int Amount;
    }

    /// <summary>
    /// Request to use an item in a container.
    /// ECS equivalent of the use-item flow.
    /// </summary>
    public struct UseItemRequest : IComponentData
    {
        /// <summary>Entity that is using the item.</summary>
        public Entity User;

        /// <summary>Hash of the item to use.</summary>
        public int ItemNameHash;
    }

    /// <summary>
    /// Request to craft an item.
    /// ECS equivalent of CraftingTrigger.CraftItem().
    /// </summary>
    public struct CraftItemRequest : IComponentData
    {
        /// <summary>Entity performing the crafting.</summary>
        public Entity Crafter;

        /// <summary>Hash of the item to craft.</summary>
        public int ItemNameHash;

        /// <summary>Remaining crafting time.</summary>
        public float RemainingTime;
    }

    /// <summary>
    /// Result event for item operations, consumed after one frame.
    /// </summary>
    public struct ItemOperationResult : IComponentData
    {
        /// <summary>Type of operation that completed.</summary>
        public ItemOperationType OperationType;

        /// <summary>Whether the operation succeeded.</summary>
        public bool Success;

        /// <summary>Hash of the item involved.</summary>
        public int ItemNameHash;
    }

    /// <summary>
    /// Types of inventory operations.
    /// </summary>
    public enum ItemOperationType : byte
    {
        Add = 0,
        Remove = 1,
        Use = 2,
        Craft = 3,
        Drop = 4,
        Equip = 5,
        Unequip = 6
    }

    /// <summary>
    /// ECS equivalent of DevionGames.InventorySystem.CurrencyConversion.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct CurrencyConversionElement : IBufferElementData
    {
        /// <summary>Hash of the source currency name.</summary>
        public int SourceCurrencyHash;

        /// <summary>Hash of the target currency name.</summary>
        public int TargetCurrencyHash;

        /// <summary>Conversion factor.</summary>
        public float Factor;
    }

    /// <summary>
    /// ECS equivalent of DevionGames.InventorySystem.Ingredient.
    /// Crafting recipe ingredient buffer.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct IngredientElement : IBufferElementData
    {
        /// <summary>Hash of the ingredient item name.</summary>
        public int ItemNameHash;

        /// <summary>Required amount.</summary>
        public int Amount;
    }
}
