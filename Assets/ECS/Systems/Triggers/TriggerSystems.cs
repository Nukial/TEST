using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace RPG.ECS.Triggers
{
    /// <summary>
    /// ECS equivalent of BaseTrigger proximity detection (OnTriggerEnter/Exit)
    /// and TriggerRaycaster.
    ///
    /// Checks distance between player entities and trigger entities,
    /// producing TriggerEventData when players enter/exit range.
    /// Mirrors BaseTrigger.Update() + OnTriggerEnter/Exit flow.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct TriggerDetectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TriggerData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            // Gather all player positions
            var playerEntities = new NativeList<Entity>(Allocator.Temp);
            var playerPositions = new NativeList<float3>(Allocator.Temp);

            foreach (var (transform, entity) in
                SystemAPI.Query<RefRO<LocalTransform>>()
                    .WithAll<RPG.ECS.Controller.PlayerTag>()
                    .WithEntityAccess())
            {
                playerEntities.Add(entity);
                playerPositions.Add(transform.ValueRO.Position);
            }

            // Check each trigger against all players
            foreach (var (triggerRef, triggerTransform, triggerEntity) in
                SystemAPI.Query<RefRW<TriggerData>, RefRO<LocalTransform>>()
                    .WithEntityAccess())
            {
                var trigger = triggerRef.ValueRW;
                float3 triggerPos = triggerTransform.ValueRO.Position;

                bool wasInRange = trigger.InRange;
                bool nowInRange = false;
                Entity closestPlayer = Entity.Null;
                float closestDist = float.MaxValue;

                for (int p = 0; p < playerEntities.Length; p++)
                {
                    float dist = math.distance(triggerPos, playerPositions[p]);
                    if (dist <= trigger.UseDistance && dist < closestDist)
                    {
                        nowInRange = true;
                        closestDist = dist;
                        closestPlayer = playerEntities[p];
                    }
                }

                // Detect range transitions
                if (nowInRange && !wasInRange)
                {
                    trigger.InRange = true;
                    trigger.CurrentUser = closestPlayer;

                    var evt = ecb.CreateEntity();
                    ecb.AddComponent(evt, new TriggerEventData
                    {
                        TriggerEntity = triggerEntity,
                        PlayerEntity = closestPlayer,
                        EventType = TriggerEventType.CameInRange
                    });
                }
                else if (!nowInRange && wasInRange)
                {
                    trigger.InRange = false;
                    var prevUser = trigger.CurrentUser;
                    trigger.CurrentUser = Entity.Null;

                    var evt = ecb.CreateEntity();
                    ecb.AddComponent(evt, new TriggerEventData
                    {
                        TriggerEntity = triggerEntity,
                        PlayerEntity = prevUser,
                        EventType = TriggerEventType.WentOutOfRange
                    });

                    // If trigger was in use, also fire UnUsed event
                    if (trigger.InUse)
                    {
                        trigger.InUse = false;
                        var unusedEvt = ecb.CreateEntity();
                        ecb.AddComponent(unusedEvt, new TriggerEventData
                        {
                            TriggerEntity = triggerEntity,
                            PlayerEntity = prevUser,
                            EventType = TriggerEventType.UnUsed
                        });
                    }
                }

                triggerRef.ValueRW = trigger;
            }

            playerEntities.Dispose();
            playerPositions.Dispose();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// ECS equivalent of DevionGames.Sequence tick logic from BehaviorTrigger.
    ///
    /// Advances active action sequences step by step. Each frame, the current
    /// action is ticked. When it completes, the sequence advances to the next action.
    /// Mirrors Sequence.Tick() which processes actions[actionIndex].OnUpdate().
    /// </summary>
    [BurstCompile]
    [UpdateAfter(typeof(TriggerDetectionSystem))]
    public partial struct ActionSequenceSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ActionSequenceData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            new TickSequencesJob { DeltaTime = dt }.ScheduleParallel();
        }

        /// <summary>
        /// Ticks all running action sequences.
        /// Mirrors Sequence.Tick() → Action.OnUpdate() → advance index.
        /// </summary>
        [BurstCompile]
        public partial struct TickSequencesJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(
                ref ActionSequenceData sequence,
                ref DynamicBuffer<ActionElement> actions)
            {
                if (sequence.Status != ActionSequenceStatus.Running)
                    return;

                if (sequence.CurrentActionIndex >= sequence.TotalActions ||
                    sequence.CurrentActionIndex >= actions.Length)
                {
                    sequence.Status = ActionSequenceStatus.Completed;
                    return;
                }

                var currentAction = actions[sequence.CurrentActionIndex];

                // Start action if not yet started
                if (currentAction.Status == ActionStatus.NotStarted)
                {
                    currentAction.Status = ActionStatus.Running;
                    actions[sequence.CurrentActionIndex] = currentAction;
                }

                // Check if running action has completed
                // (In a full implementation, each action type would have its own completion logic.
                //  Here we use a timer-based approach via FloatParam as duration.)
                if (currentAction.Status == ActionStatus.Running)
                {
                    currentAction.FloatParam -= DeltaTime;
                    if (currentAction.FloatParam <= 0f)
                    {
                        currentAction.Status = ActionStatus.Success;
                    }
                    actions[sequence.CurrentActionIndex] = currentAction;
                }

                // Advance to next action if current succeeded
                if (currentAction.Status == ActionStatus.Success)
                {
                    sequence.CurrentActionIndex++;

                    if (sequence.CurrentActionIndex >= sequence.TotalActions)
                    {
                        sequence.Status = ActionSequenceStatus.Completed;
                    }
                }
                else if (currentAction.Status == ActionStatus.Failure)
                {
                    sequence.Status = ActionSequenceStatus.Completed;
                }
            }
        }
    }

    /// <summary>
    /// Cleans up one-frame TriggerEventData entities.
    /// </summary>
    [UpdateAfter(typeof(ActionSequenceSystem))]
    public partial struct TriggerEventCleanupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (_, entity) in
                SystemAPI.Query<RefRO<TriggerEventData>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
