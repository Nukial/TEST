using Unity.Entities;
using Unity.Collections;

namespace RPG.ECS.Stats
{
    /// <summary>
    /// ECS equivalent of DevionGames.StatSystem.Stat (ScriptableObject).
    /// Represents a single stat on an entity (e.g. HP, ATK, DEF).
    /// Stored as a buffer element on the StatsHandler entity.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct StatElement : IBufferElementData
    {
        /// <summary>Unique hash of the stat name for fast lookup.</summary>
        public int NameHash;

        /// <summary>Base value before any modifiers are applied.</summary>
        public float BaseValue;

        /// <summary>Final calculated value after modifiers and formula evaluation.</summary>
        public float Value;

        /// <summary>Maximum cap for this stat. Negative means no cap.</summary>
        public float Cap;

        /// <summary>Whether this stat needs recalculation.</summary>
        public bool IsDirty;
    }

    /// <summary>
    /// ECS equivalent of DevionGames.StatSystem.Attribute : Stat.
    /// Extends a stat with a current value (e.g. current HP out of max HP).
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct AttributeElement : IBufferElementData
    {
        /// <summary>Hash of the stat name this attribute corresponds to.</summary>
        public int StatNameHash;

        /// <summary>Current value, clamped between 0 and the stat's Value.</summary>
        public float CurrentValue;

        /// <summary>Starting percentage (0–1) of the stat Value used at initialization.</summary>
        public float StartPercent;
    }

    /// <summary>
    /// ECS equivalent of DevionGames.StatSystem.StatModifier.
    /// Represents a single modifier applied to a stat.
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct StatModifierElement : IBufferElementData
    {
        /// <summary>Hash of the target stat name.</summary>
        public int StatNameHash;

        /// <summary>Modifier value.</summary>
        public float Value;

        /// <summary>Type of modifier: 0 = Flat, 1 = PercentAdd, 2 = PercentMult.</summary>
        public StatModType ModType;

        /// <summary>Entity that is the source of this modifier (for tracking/removal).</summary>
        public Entity Source;
    }

    /// <summary>
    /// Mirrors DevionGames.StatSystem.StatModType enum.
    /// </summary>
    public enum StatModType : byte
    {
        Flat = 0,
        PercentAdd = 1,
        PercentMult = 2
    }

    /// <summary>
    /// Tag component marking an entity as a stats handler.
    /// ECS equivalent of DevionGames.StatSystem.StatsHandler (MonoBehaviour).
    /// </summary>
    public struct StatsHandlerTag : IComponentData
    {
        /// <summary>Hash of the handler name for registry lookup.</summary>
        public int NameHash;
    }

    /// <summary>
    /// ECS equivalent of DevionGames.StatSystem.Level : Stat.
    /// Links a level stat to its experience attribute.
    /// </summary>
    public struct LevelData : IComponentData
    {
        /// <summary>Hash of the level stat name.</summary>
        public int LevelStatHash;

        /// <summary>Hash of the experience attribute stat name.</summary>
        public int ExperienceStatHash;
    }
}
