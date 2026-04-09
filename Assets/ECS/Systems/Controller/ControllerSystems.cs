using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace RPG.ECS.Controller
{
    /// <summary>
    /// ECS equivalent of ThirdPersonController.Update() input gathering.
    ///
    /// In the original system, ThirdPersonController reads Input.GetAxis for
    /// movement and mouse, then passes data to MotionState. Here we read
    /// InputData (written by a managed system or MonoBehaviour bridge) and
    /// compute the movement direction.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct InputGatheringSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new GatherInputJob().ScheduleParallel();
        }

        /// <summary>
        /// Converts raw InputData into a movement direction vector on MovementData.
        /// Mirrors ThirdPersonController's input processing in Update().
        /// </summary>
        [BurstCompile]
        public partial struct GatherInputJob : IJobEntity
        {
            public void Execute(
                in InputData input,
                ref MovementData movement,
                in LocalTransform transform)
            {
                // Compute movement direction from input axes relative to entity's transform
                float3 forward = math.mul(transform.Rotation, new float3(0f, 0f, 1f));
                float3 right = math.mul(transform.Rotation, new float3(1f, 0f, 0f));

                // Project to horizontal plane
                forward.y = 0f;
                forward = math.normalizesafe(forward);
                right.y = 0f;
                right = math.normalizesafe(right);

                movement.MoveDirection = forward * input.Forward + right * input.Horizontal;

                // Normalize if magnitude > 1
                float sqrMag = math.lengthsq(movement.MoveDirection);
                if (sqrMag > 1f)
                    movement.MoveDirection = math.normalize(movement.MoveDirection);
            }
        }
    }

    /// <summary>
    /// ECS equivalent of ThirdPersonController.FixedUpdate() movement logic.
    ///
    /// Applies movement direction and speed to update the entity's position.
    /// Handles gravity, grounding, and basic character physics.
    /// Mirrors the original controller's velocity calculation and rigidbody movement.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(InputGatheringSystem))]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            new MoveCharacterJob { DeltaTime = dt }.ScheduleParallel();
        }

        /// <summary>
        /// Applies movement, gravity, and rotation to character entities.
        /// Mirrors ThirdPersonController.FixedUpdate() + MotionState.UpdateVelocity().
        /// </summary>
        [BurstCompile]
        public partial struct MoveCharacterJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(
                ref LocalTransform transform,
                ref MovementData movement,
                in PhysicsData physics,
                in InputData input)
            {
                // Apply gravity if not grounded
                float3 velocity = movement.Velocity;
                if (!movement.IsGrounded)
                {
                    velocity.y -= 9.81f * movement.GravityMultiplier * DeltaTime;
                }
                else
                {
                    velocity.y = -0.1f; // Small downward force to keep grounded

                    // Apply jump
                    if (input.JumpPressed)
                    {
                        velocity.y = movement.JumpForce;
                    }
                }

                // Calculate horizontal speed
                float targetSpeed = movement.Speed * movement.SpeedMultiplier;
                if (input.SprintHeld)
                    targetSpeed *= 1.5f;

                // Apply horizontal movement
                float3 horizontalVelocity = movement.MoveDirection * targetSpeed;
                velocity.x = horizontalVelocity.x;
                velocity.z = horizontalVelocity.z;

                // Update position
                float3 newPos = transform.Position + velocity * DeltaTime;
                transform.Position = newPos;
                movement.Velocity = velocity;

                // Update rotation to face movement direction
                if (math.lengthsq(movement.MoveDirection) > 0.01f)
                {
                    float targetAngle = math.atan2(movement.MoveDirection.x, movement.MoveDirection.z);
                    float currentAngle = math.atan2(
                        math.mul(transform.Rotation, new float3(0, 0, 1)).x,
                        math.mul(transform.Rotation, new float3(0, 0, 1)).z);

                    float angle = math.lerp(currentAngle, targetAngle,
                        DeltaTime * movement.RotationSpeed);
                    transform.Rotation = quaternion.RotateY(angle);
                }
            }
        }
    }

    /// <summary>
    /// ECS equivalent of the MotionState state machine from ThirdPersonController.
    ///
    /// Manages character state transitions (Idle, Walk, Run, Sprint, Jump, Fall, etc.)
    /// based on movement data and input. Mirrors the CanStart/OnStart/CanStop/OnStop
    /// pattern from MotionState.
    /// </summary>
    [BurstCompile]
    [UpdateAfter(typeof(MovementSystem))]
    public partial struct CharacterStateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            new UpdateCharacterStateJob { DeltaTime = dt }.ScheduleParallel();
        }

        /// <summary>
        /// Evaluates state transitions based on movement and input.
        /// Mirrors the MotionState lifecycle: CanStart → OnStart → OnUpdate → CanStop → OnStop.
        /// </summary>
        [BurstCompile]
        public partial struct UpdateCharacterStateJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(
                ref CharacterStateData charState,
                in MovementData movement,
                in InputData input)
            {
                charState.StateTime += DeltaTime;
                float moveSpeed = math.length(new float2(movement.Velocity.x, movement.Velocity.z));

                // Determine desired state based on conditions
                MotionStateType desiredState = charState.CurrentState;

                if (!movement.IsGrounded && movement.Velocity.y < -0.5f)
                {
                    desiredState = MotionStateType.Fall;
                }
                else if (!movement.IsGrounded && movement.Velocity.y > 0.1f)
                {
                    desiredState = MotionStateType.Jump;
                }
                else if (movement.IsGrounded)
                {
                    if (charState.CurrentState == MotionStateType.Fall ||
                        charState.CurrentState == MotionStateType.Jump)
                    {
                        desiredState = MotionStateType.Land;
                    }
                    else if (charState.CurrentState == MotionStateType.Land &&
                             charState.StateTime > 0.2f)
                    {
                        desiredState = moveSpeed > 0.1f ? MotionStateType.Walk : MotionStateType.Idle;
                    }
                    else if (charState.CurrentState != MotionStateType.Land)
                    {
                        if (moveSpeed < 0.1f)
                        {
                            desiredState = MotionStateType.Idle;
                        }
                        else if (input.SprintHeld && moveSpeed > 3f)
                        {
                            desiredState = MotionStateType.Sprint;
                        }
                        else if (moveSpeed > 2f)
                        {
                            desiredState = MotionStateType.Run;
                        }
                        else
                        {
                            desiredState = MotionStateType.Walk;
                        }
                    }
                }

                // Apply state transition
                if (desiredState != charState.CurrentState)
                {
                    charState.PreviousState = charState.CurrentState;
                    charState.CurrentState = desiredState;
                    charState.StateTime = 0f;
                }
            }
        }
    }
}
