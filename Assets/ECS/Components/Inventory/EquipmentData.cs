using Unity.Entities;

namespace RPG.ECS.Inventory
{
    /// <summary>
    /// ECS equivalent of DevionGames.InventorySystem.EquipmentHandler (MonoBehaviour).
    /// Marks an entity as having equipment capabilities.
    /// </summary>
    public struct EquipmentHandlerTag : IComponentData
    {
        /// <summary>Hash of the equipment UI window name.</summary>
        public int WindowNameHash;
    }

    /// <summary>
    /// Buffer element representing an equipment slot on an entity.
    /// Maps equipment regions to currently equipped items.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct EquipmentSlotElement : IBufferElementData
    {
        /// <summary>Hash of the equipment region (e.g. "Head", "Chest").</summary>
        public int RegionHash;

        /// <summary>Hash of the equipped item name. 0 if empty.</summary>
        public int EquippedItemHash;

        /// <summary>Entity reference to the visual instance (if any).</summary>
        public Entity VisualInstance;

        /// <summary>Whether this slot is currently occupied.</summary>
        public bool IsOccupied;
    }

    /// <summary>
    /// Request component to equip an item. Consumed by EquipmentSystem.
    /// </summary>
    public struct EquipRequest : IComponentData
    {
        /// <summary>Entity that should equip the item.</summary>
        public Entity Target;

        /// <summary>Hash of the item to equip.</summary>
        public int ItemNameHash;

        /// <summary>Hash of the equipment region.</summary>
        public int RegionHash;
    }

    /// <summary>
    /// Request component to unequip an item. Consumed by EquipmentSystem.
    /// </summary>
    public struct UnequipRequest : IComponentData
    {
        /// <summary>Entity that should unequip the item.</summary>
        public Entity Target;

        /// <summary>Hash of the equipment region to clear.</summary>
        public int RegionHash;
    }

    /// <summary>
    /// ECS equivalent of DevionGames.InventorySystem.Restriction.
    /// Defines item restrictions for inventory slots.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct SlotRestrictionElement : IBufferElementData
    {
        /// <summary>Hash of the required category (0 = no restriction).</summary>
        public int CategoryHash;

        /// <summary>Required item type (0 = any).</summary>
        public ItemType RequiredType;
    }
}
