using Unity.Entities;
using UnityEngine;
using RPG.ECS.Stats;

namespace RPG.ECS.Authoring
{
    /// <summary>
    /// Authoring component for converting a StatsHandler MonoBehaviour
    /// to ECS components. Mirrors DevionGames.StatSystem.StatsHandler.
    ///
    /// Attach this to a GameObject in the SubScene to bake it into an ECS entity
    /// with StatElement, AttributeElement, StatModifierElement, and StatEffectElement buffers.
    /// </summary>
    public class StatsHandlerAuthoring : MonoBehaviour
    {
        [Tooltip("Name of this stats handler for registry lookup.")]
        public string handlerName = "Player";

        [Tooltip("Stat definitions with base values.")]
        public StatDefinition[] stats = new StatDefinition[]
        {
            new StatDefinition { name = "Health", baseValue = 100f, cap = -1f, isAttribute = true, startPercent = 1f },
            new StatDefinition { name = "Mana", baseValue = 50f, cap = -1f, isAttribute = true, startPercent = 1f },
            new StatDefinition { name = "Damage", baseValue = 10f, cap = -1f },
            new StatDefinition { name = "Defense", baseValue = 5f, cap = -1f },
            new StatDefinition { name = "Critical Strike", baseValue = 5f, cap = 100f },
        };

        [Tooltip("Whether this entity has leveling (Level stat + Experience attribute).")]
        public bool hasLevel = false;

        [Tooltip("Level stat name.")]
        public string levelStatName = "Level";

        [Tooltip("Experience attribute name.")]
        public string experienceStatName = "Experience";

        /// <summary>
        /// Definition of a single stat, used in the inspector.
        /// </summary>
        [System.Serializable]
        public struct StatDefinition
        {
            public string name;
            public float baseValue;
            public float cap;
            public bool isAttribute;
            [Range(0f, 1f)]
            public float startPercent;
        }

        /// <summary>
        /// Baker that converts the StatsHandlerAuthoring data into ECS components and buffers.
        /// </summary>
        public class Baker : Baker<StatsHandlerAuthoring>
        {
            public override void Bake(StatsHandlerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add tag
                AddComponent(entity, new StatsHandlerTag
                {
                    NameHash = authoring.handlerName.GetHashCode()
                });

                // Add stat buffer
                var statBuffer = AddBuffer<StatElement>(entity);
                var attrBuffer = AddBuffer<AttributeElement>(entity);
                var modBuffer = AddBuffer<StatModifierElement>(entity);
                var effectBuffer = AddBuffer<StatEffectElement>(entity);
                var callbackBuffer = AddBuffer<StatCallbackElement>(entity);

                foreach (var def in authoring.stats)
                {
                    int nameHash = def.name.GetHashCode();

                    statBuffer.Add(new StatElement
                    {
                        NameHash = nameHash,
                        BaseValue = def.baseValue,
                        Value = def.baseValue,
                        Cap = def.cap,
                        IsDirty = true
                    });

                    if (def.isAttribute)
                    {
                        attrBuffer.Add(new AttributeElement
                        {
                            StatNameHash = nameHash,
                            CurrentValue = def.baseValue * def.startPercent,
                            StartPercent = def.startPercent
                        });
                    }
                }

                // Add level data if applicable
                if (authoring.hasLevel)
                {
                    AddComponent(entity, new LevelData
                    {
                        LevelStatHash = authoring.levelStatName.GetHashCode(),
                        ExperienceStatHash = authoring.experienceStatName.GetHashCode()
                    });
                }
            }
        }
    }
}
