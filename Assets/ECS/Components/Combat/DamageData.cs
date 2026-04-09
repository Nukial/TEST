using Unity.Entities;
using Unity.Mathematics;

namespace RPG.ECS.Combat
{
    /// <summary>
    /// ECS equivalent of damage request flow from DevionGames.StatSystem.DamageData.
    /// A request component placed on an entity to apply damage.
    /// </summary>
    public struct DamageRequest : IComponentData
    {
        /// <summary>Entity that is dealing the damage.</summary>
        public Entity Sender;

        /// <summary>Entity that receives the damage.</summary>
        public Entity Receiver;

        /// <summary>Hash of the sending stat (e.g. "Damage").</summary>
        public int SendingStatHash;

        /// <summary>Hash of the receiving stat (e.g. "Health").</summary>
        public int ReceivingStatHash;

        /// <summary>Hash of the critical strike stat.</summary>
        public int CriticalStrikeStatHash;

        /// <summary>Maximum distance for damage to apply.</summary>
        public float MaxDistance;

        /// <summary>Maximum angle for damage cone.</summary>
        public float MaxAngle;

        /// <summary>Whether knockback is enabled.</summary>
        public bool EnableKnockback;

        /// <summary>Knockback strength if enabled.</summary>
        public float KnockbackStrength;

        /// <summary>Knockback duration if enabled.</summary>
        public float KnockbackDuration;
    }

    /// <summary>
    /// Result of damage calculation, attached to receiver entity for one frame.
    /// </summary>
    public struct DamageResult : IComponentData
    {
        /// <summary>Entity that dealt the damage.</summary>
        public Entity Sender;

        /// <summary>Final damage amount applied.</summary>
        public float DamageAmount;

        /// <summary>Whether this was a critical hit.</summary>
        public bool IsCritical;

        /// <summary>Knockback direction if applicable.</summary>
        public float3 KnockbackDirection;

        /// <summary>Knockback force if applicable.</summary>
        public float KnockbackForce;
    }

    /// <summary>
    /// Tag to signal that a DamageRequest has been processed and should be cleaned up.
    /// </summary>
    public struct DamageProcessedTag : IComponentData { }
}
