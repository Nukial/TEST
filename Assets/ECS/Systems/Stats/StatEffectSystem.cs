using Unity.Burst;
using Unity.Entities;

namespace RPG.ECS.Stats
{
    /// <summary>
    /// ECS equivalent of StatEffect.Execute() from DevionGames.StatSystem.StatEffect.
    ///
    /// Ticks active stat effects each frame, advancing their repeat count
    /// and deactivating them when completed. In the original system, StatEffect
    /// runs an action sequence on a timer; here we track elapsed time and ticks.
    /// </summary>
    [BurstCompile]
    [UpdateAfter(typeof(AttributeSystem))]
    public partial struct StatEffectSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StatsHandlerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            new TickEffectsJob { DeltaTime = deltaTime }.ScheduleParallel();
        }

        /// <summary>
        /// Advances all active stat effects. When elapsed time exceeds the tick interval,
        /// increments the repeat count. Deactivates the effect when repeats are exhausted.
        /// Mirrors StatEffect.Execute() which ticks a Sequence and increments m_CurrentRepeat.
        /// </summary>
        [BurstCompile]
        public partial struct TickEffectsJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(ref DynamicBuffer<StatEffectElement> effects)
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    var effect = effects[i];
                    if (!effect.IsActive)
                        continue;

                    effect.ElapsedTime += DeltaTime;

                    if (effect.TickInterval > 0f && effect.ElapsedTime >= effect.TickInterval)
                    {
                        effect.ElapsedTime -= effect.TickInterval;
                        effect.CurrentRepeat++;

                        // Check if effect should deactivate
                        // TotalRepeats < 0 means infinite
                        if (effect.TotalRepeats >= 0 && effect.CurrentRepeat >= effect.TotalRepeats)
                        {
                            effect.IsActive = false;
                        }
                    }

                    effects[i] = effect;
                }
            }
        }
    }

    /// <summary>
    /// ECS equivalent of Level's experience-to-level-up logic from
    /// DevionGames.StatSystem.Level.
    ///
    /// When experience (attribute) reaches its max value, increments the level stat
    /// and resets experience to 0.
    /// </summary>
    [BurstCompile]
    [UpdateAfter(typeof(AttributeSystem))]
    public partial struct LevelUpSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LevelData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new CheckLevelUpJob().ScheduleParallel();
        }

        /// <summary>
        /// Checks if the experience attribute has reached or exceeded the stat max,
        /// and if so, increments the level and resets experience.
        /// Mirrors Level.Initialize() which hooks into Attribute.onCurrentValueChange.
        /// </summary>
        [BurstCompile]
        public partial struct CheckLevelUpJob : IJobEntity
        {
            public void Execute(
                in LevelData levelData,
                ref DynamicBuffer<StatElement> stats,
                ref DynamicBuffer<AttributeElement> attributes)
            {
                // Find the experience attribute
                int expAttrIdx = -1;
                float expMax = 0f;
                for (int a = 0; a < attributes.Length; a++)
                {
                    if (attributes[a].StatNameHash == levelData.ExperienceStatHash)
                    {
                        expAttrIdx = a;
                        break;
                    }
                }

                if (expAttrIdx < 0)
                    return;

                // Find the experience stat to get max value
                for (int s = 0; s < stats.Length; s++)
                {
                    if (stats[s].NameHash == levelData.ExperienceStatHash)
                    {
                        expMax = stats[s].Value;
                        break;
                    }
                }

                var expAttr = attributes[expAttrIdx];
                if (expAttr.CurrentValue < expMax)
                    return;

                // Level up: increment level base value, reset experience
                for (int s = 0; s < stats.Length; s++)
                {
                    if (stats[s].NameHash == levelData.LevelStatHash)
                    {
                        var levelStat = stats[s];
                        levelStat.BaseValue += 1f;
                        levelStat.IsDirty = true;
                        stats[s] = levelStat;
                        break;
                    }
                }

                expAttr.CurrentValue = 0f;
                attributes[expAttrIdx] = expAttr;
            }
        }
    }
}
