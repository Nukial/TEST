using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace RPG.ECS.Stats
{
    /// <summary>
    /// ECS equivalent of Stat.CalculateValue() and modifier application logic
    /// from DevionGames.StatSystem.Stat / StatsHandler.
    ///
    /// Processes all stat entities whose stats are marked dirty,
    /// applies modifiers (Flat → PercentAdd → PercentMult), and updates the final value.
    /// Mirrors the modifier stacking order from the original StatModifier system.
    /// </summary>
    [BurstCompile]
    public partial struct StatCalculationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StatsHandlerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new CalculateStatsJob().ScheduleParallel();
        }

        /// <summary>
        /// Iterates over all stats handler entities and recalculates dirty stats.
        /// Modifier application order mirrors Stat.CalculateValue():
        /// 1. Start with BaseValue
        /// 2. Add all Flat modifiers
        /// 3. Multiply by (1 + sum of PercentAdd modifiers)
        /// 4. Multiply by product of (1 + each PercentMult modifier)
        /// 5. Clamp to Cap if Cap >= 0
        /// </summary>
        [BurstCompile]
        public partial struct CalculateStatsJob : IJobEntity
        {
            public void Execute(
                ref DynamicBuffer<StatElement> stats,
                in DynamicBuffer<StatModifierElement> modifiers,
                in StatsHandlerTag handler)
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    var stat = stats[i];
                    if (!stat.IsDirty)
                        continue;

                    float baseValue = stat.BaseValue;
                    float flatSum = 0f;
                    float percentAddSum = 0f;
                    float percentMultProduct = 1f;

                    // Gather modifiers for this stat
                    for (int m = 0; m < modifiers.Length; m++)
                    {
                        var mod = modifiers[m];
                        if (mod.StatNameHash != stat.NameHash)
                            continue;

                        switch (mod.ModType)
                        {
                            case StatModType.Flat:
                                flatSum += mod.Value;
                                break;
                            case StatModType.PercentAdd:
                                percentAddSum += mod.Value;
                                break;
                            case StatModType.PercentMult:
                                percentMultProduct *= (1f + mod.Value);
                                break;
                        }
                    }

                    // Apply modifiers in order: Flat → PercentAdd → PercentMult
                    float finalValue = baseValue;
                    finalValue += flatSum;
                    finalValue *= (1f + percentAddSum);
                    finalValue *= percentMultProduct;

                    // Apply cap
                    if (stat.Cap >= 0f && finalValue > stat.Cap)
                        finalValue = stat.Cap;

                    stat.Value = finalValue;
                    stat.IsDirty = false;
                    stats[i] = stat;
                }
            }
        }
    }
}
