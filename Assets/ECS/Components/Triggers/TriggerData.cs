using Unity.Entities;
using Unity.Mathematics;

namespace RPG.ECS.Triggers
{
    /// <summary>
    /// ECS equivalent of DevionGames.BaseTrigger (CallbackHandler).
    /// Core trigger data for proximity-based interaction.
    /// </summary>
    public struct TriggerData : IComponentData
    {
        /// <summary>Hash of the trigger name/identifier.</summary>
        public int NameHash;

        /// <summary>Distance at which the trigger activates.</summary>
        public float UseDistance;

        /// <summary>Input type required to activate: 0=LeftClick, 1=RightClick, 2=MiddleClick, 3=Key, 4=OnTriggerEnter.</summary>
        public TriggerInputType InputType;

        /// <summary>Whether the trigger is currently in range of a player.</summary>
        public bool InRange;

        /// <summary>Whether the trigger is currently in use.</summary>
        public bool InUse;

        /// <summary>Whether the trigger has been started (initialized).</summary>
        public bool IsStarted;

        /// <summary>Entity of the player currently interacting.</summary>
        public Entity CurrentUser;
    }

    /// <summary>
    /// Mirrors DevionGames.BaseTrigger.TriggerInputType.
    /// </summary>
    public enum TriggerInputType : byte
    {
        LeftClick = 0,
        RightClick = 1,
        MiddleClick = 2,
        Key = 3,
        OnTriggerEnter = 4
    }

    /// <summary>
    /// Event component fired when a trigger state changes.
    /// Consumed by systems after one frame.
    /// </summary>
    public struct TriggerEventData : IComponentData
    {
        /// <summary>The trigger entity that fired the event.</summary>
        public Entity TriggerEntity;

        /// <summary>The player entity involved.</summary>
        public Entity PlayerEntity;

        /// <summary>Type of trigger event.</summary>
        public TriggerEventType EventType;
    }

    /// <summary>
    /// Types of trigger events, mirroring BaseTrigger callbacks.
    /// </summary>
    public enum TriggerEventType : byte
    {
        CameInRange = 0,
        WentOutOfRange = 1,
        Used = 2,
        UnUsed = 3,
        Interrupted = 4
    }

    /// <summary>
    /// ECS equivalent of DevionGames.Sequence and Actions.
    /// Represents an action sequence attached to a trigger.
    /// </summary>
    public struct ActionSequenceData : IComponentData
    {
        /// <summary>Current action index in the sequence.</summary>
        public int CurrentActionIndex;

        /// <summary>Total number of actions in the sequence.</summary>
        public int TotalActions;

        /// <summary>Current status of the sequence.</summary>
        public ActionSequenceStatus Status;

        /// <summary>Whether the sequence can be interrupted.</summary>
        public bool Interruptable;
    }

    /// <summary>
    /// Individual action element in a sequence.
    /// ECS equivalent of DevionGames.Action (abstract).
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct ActionElement : IBufferElementData
    {
        /// <summary>Hash of the action type name.</summary>
        public int ActionTypeHash;

        /// <summary>Current status of this action.</summary>
        public ActionStatus Status;

        /// <summary>Generic float parameter for the action.</summary>
        public float FloatParam;

        /// <summary>Generic int parameter for the action.</summary>
        public int IntParam;

        /// <summary>Target type: 0 = Self, 1 = Player.</summary>
        public byte TargetType;

        /// <summary>Hash of the target stat/item name if applicable.</summary>
        public int TargetNameHash;
    }

    /// <summary>
    /// Status of an action sequence.
    /// </summary>
    public enum ActionSequenceStatus : byte
    {
        Inactive = 0,
        Running = 1,
        Completed = 2,
        Interrupted = 3
    }

    /// <summary>
    /// Status of an individual action within a sequence.
    /// </summary>
    public enum ActionStatus : byte
    {
        NotStarted = 0,
        Running = 1,
        Success = 2,
        Failure = 3
    }

    /// <summary>
    /// ECS equivalent of DevionGames.BehaviorTrigger, extending trigger with action sequences.
    /// </summary>
    public struct BehaviorTriggerData : IComponentData
    {
        /// <summary>Entity holding the onUsed action sequence.</summary>
        public Entity OnUsedSequence;

        /// <summary>Entity holding the onUnused action sequence.</summary>
        public Entity OnUnusedSequence;
    }

    /// <summary>
    /// Tag for inventory-specific triggers (VendorTrigger, CraftingTrigger base).
    /// </summary>
    public struct InventoryTriggerTag : IComponentData
    {
        /// <summary>Hash of the window to open when triggered.</summary>
        public int WindowNameHash;
    }

    /// <summary>
    /// ECS equivalent of DevionGames.VendorTrigger.
    /// </summary>
    public struct VendorData : IComponentData
    {
        /// <summary>Price multiplier when buying from vendor.</summary>
        public float BuyPriceFactor;

        /// <summary>Price multiplier when selling to vendor.</summary>
        public float SellPriceFactor;
    }

    /// <summary>
    /// ECS equivalent of DevionGames.CraftingTrigger.
    /// </summary>
    public struct CraftingData : IComponentData
    {
        /// <summary>Hash of the required ingredients window.</summary>
        public int IngredientsWindowHash;
    }
}
