using Unity.Entities;
using Unity.Collections;

namespace RPG.ECS.Inventory
{
    /// <summary>
    /// ECS equivalent of DevionGames.InventorySystem.Item (ScriptableObject).
    /// Core item data stored as a buffer element on inventory entities.
    /// The item hierarchy (Item → UsableItem → EquipmentItem, Currency, Skill)
    /// is flattened into a single struct with type flags.
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct ItemElement : IBufferElementData
    {
        /// <summary>Hash of the unique item ID.</summary>
        public int IdHash;

        /// <summary>Hash of the item name.</summary>
        public int NameHash;

        /// <summary>Hash of the item category.</summary>
        public int CategoryHash;

        /// <summary>Type of item, replacing the class hierarchy.</summary>
        public ItemType Type;

        /// <summary>Current stack size.</summary>
        public int Stack;

        /// <summary>Maximum stack size.</summary>
        public int MaxStack;

        /// <summary>Item rarity level.</summary>
        public int RarityIndex;

        /// <summary>Buy price.</summary>
        public int BuyPrice;

        /// <summary>Sell price.</summary>
        public int SellPrice;

        /// <summary>Whether the item is sellable.</summary>
        public bool IsSellable;

        /// <summary>Whether the item is droppable.</summary>
        public bool IsDroppable;

        /// <summary>Whether the item is craftable.</summary>
        public bool IsCraftable;

        /// <summary>Crafting duration in seconds.</summary>
        public float CraftingDuration;

        /// <summary>Cooldown duration for usable items.</summary>
        public float Cooldown;

        /// <summary>Remaining cooldown time.</summary>
        public float CooldownRemaining;

        /// <summary>Equipment region hash (for EquipmentItem type).</summary>
        public int EquipmentRegionHash;

        /// <summary>Current skill value (for Skill type).</summary>
        public float SkillCurrentValue;

        /// <summary>Fixed success chance for skills (0–100).</summary>
        public float SkillSuccessChance;
    }

    /// <summary>
    /// Replaces the Item → UsableItem → EquipmentItem / Currency / Skill class hierarchy.
    /// </summary>
    public enum ItemType : byte
    {
        Item = 0,
        UsableItem = 1,
        EquipmentItem = 2,
        Currency = 3,
        Skill = 4
    }

    /// <summary>
    /// Tag component marking an entity as an inventory container.
    /// ECS equivalent of DevionGames.InventorySystem.ItemContainer (UIWidget).
    /// </summary>
    public struct InventoryContainerTag : IComponentData
    {
        /// <summary>Hash of the container/window name.</summary>
        public int NameHash;

        /// <summary>Maximum number of slots in this container.</summary>
        public int MaxSlots;

        /// <summary>Whether this container uses references (not ownership).</summary>
        public bool UseReferences;

        /// <summary>Whether items can be dragged in.</summary>
        public bool CanDragIn;

        /// <summary>Whether items can be dragged out.</summary>
        public bool CanDragOut;

        /// <summary>Whether items can be dropped to the ground.</summary>
        public bool CanDropItems;

        /// <summary>Whether items can be sold.</summary>
        public bool CanSellItems;

        /// <summary>Whether items can be used.</summary>
        public bool CanUseItems;
    }

    /// <summary>
    /// ECS equivalent of DevionGames.InventorySystem.ItemCollection (MonoBehaviour).
    /// Marks an entity as having an item collection that can be serialized.
    /// </summary>
    public struct ItemCollectionTag : IComponentData
    {
        /// <summary>Whether this collection should be saved/loaded.</summary>
        public bool Saveable;
    }

    /// <summary>
    /// ECS equivalent of DevionGames.InventorySystem.InventoryManager (Singleton MonoBehaviour).
    /// Singleton component for global inventory settings.
    /// </summary>
    public struct InventoryManagerSingleton : IComponentData
    {
        /// <summary>Whether save data has been loaded.</summary>
        public bool IsLoaded;
    }
}
