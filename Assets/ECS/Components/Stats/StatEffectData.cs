using Unity.Entities;

namespace RPG.ECS.Stats
{
    /// <summary>
    /// ECS equivalent of DevionGames.StatSystem.StatEffect (ScriptableObject).
    /// Represents an active stat effect with repeating actions on an entity.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct StatEffectElement : IBufferElementData
    {
        /// <summary>Hash of the effect name.</summary>
        public int NameHash;

        /// <summary>Total number of repeats. Negative means infinite.</summary>
        public int TotalRepeats;

        /// <summary>Current repeat count.</summary>
        public int CurrentRepeat;

        /// <summary>Time elapsed since last tick.</summary>
        public float ElapsedTime;

        /// <summary>Interval between ticks in seconds.</summary>
        public float TickInterval;

        /// <summary>Whether the effect is currently active.</summary>
        public bool IsActive;
    }

    /// <summary>
    /// Stat callback condition, ECS equivalent of DevionGames.StatSystem.StatCallback.
    /// Triggers actions when a stat value meets a condition.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct StatCallbackElement : IBufferElementData
    {
        /// <summary>Hash of the stat to monitor.</summary>
        public int StatNameHash;

        /// <summary>Which value to monitor: 0 = Value, 1 = CurrentValue.</summary>
        public ValueType ValueType;

        /// <summary>Condition type for comparison.</summary>
        public ConditionType Condition;

        /// <summary>Value to compare against.</summary>
        public float CompareValue;

        /// <summary>Whether this callback has been triggered.</summary>
        public bool Triggered;
    }

    /// <summary>
    /// Mirrors DevionGames.StatSystem.ValueType enum.
    /// </summary>
    public enum ValueType : byte
    {
        Value = 0,
        CurrentValue = 1
    }

    /// <summary>
    /// Mirrors DevionGames.StatSystem.ConditionType enum.
    /// </summary>
    public enum ConditionType : byte
    {
        Greater = 0,
        GreaterOrEqual = 1,
        Less = 2,
        LessOrEqual = 3
    }
}
