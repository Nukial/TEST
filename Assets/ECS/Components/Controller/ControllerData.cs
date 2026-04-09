using Unity.Entities;
using Unity.Mathematics;

namespace RPG.ECS.Controller
{
    /// <summary>
    /// ECS equivalent of input handling in DevionGames.ThirdPersonController.
    /// Gathers raw input data for the character controller.
    /// </summary>
    public struct InputData : IComponentData
    {
        /// <summary>Forward/backward input axis (-1 to 1).</summary>
        public float Forward;

        /// <summary>Horizontal/strafe input axis (-1 to 1).</summary>
        public float Horizontal;

        /// <summary>Mouse X delta.</summary>
        public float MouseX;

        /// <summary>Mouse Y delta.</summary>
        public float MouseY;

        /// <summary>Whether jump was pressed this frame.</summary>
        public bool JumpPressed;

        /// <summary>Whether sprint is held.</summary>
        public bool SprintHeld;

        /// <summary>Whether the use/interact key was pressed.</summary>
        public bool UsePressed;

        /// <summary>Whether aim is active.</summary>
        public bool AimHeld;
    }

    /// <summary>
    /// ECS equivalent of movement-related data from ThirdPersonController.
    /// </summary>
    public struct MovementData : IComponentData
    {
        /// <summary>Current movement speed.</summary>
        public float Speed;

        /// <summary>Speed multiplier (mirrors ThirdPersonController.SpeedMultiplier).</summary>
        public float SpeedMultiplier;

        /// <summary>Current velocity of the character.</summary>
        public float3 Velocity;

        /// <summary>Desired movement direction in world space.</summary>
        public float3 MoveDirection;

        /// <summary>Rotation speed for turning.</summary>
        public float RotationSpeed;

        /// <summary>Target rotation angle.</summary>
        public float TargetRotationY;

        /// <summary>Whether the character is grounded.</summary>
        public bool IsGrounded;

        /// <summary>Whether the character is on a step.</summary>
        public bool IsOnStep;

        /// <summary>Gravity multiplier.</summary>
        public float GravityMultiplier;

        /// <summary>Jump force.</summary>
        public float JumpForce;
    }

    /// <summary>
    /// ECS equivalent of the MotionState state machine from ThirdPersonController.
    /// Tracks the current motion state of the character.
    /// </summary>
    public struct CharacterStateData : IComponentData
    {
        /// <summary>Current active motion state.</summary>
        public MotionStateType CurrentState;

        /// <summary>Previous motion state (for transitions).</summary>
        public MotionStateType PreviousState;

        /// <summary>Time spent in current state.</summary>
        public float StateTime;

        /// <summary>Whether a state transition is pending.</summary>
        public bool TransitionPending;

        /// <summary>Target state for transition.</summary>
        public MotionStateType TargetState;
    }

    /// <summary>
    /// Replaces the MotionState class hierarchy with enum-based state identification.
    /// Each state in the original system (Idle, Walk, Run, Jump, Fall, etc.)
    /// becomes an enum value.
    /// </summary>
    public enum MotionStateType : byte
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Sprint = 3,
        Jump = 4,
        Fall = 5,
        Land = 6,
        Crouch = 7,
        Climb = 8,
        Swim = 9,
        Push = 10
    }

    /// <summary>
    /// ECS equivalent of physics data from ThirdPersonController.
    /// Used by the CharacterControllerSystem.
    /// </summary>
    public struct PhysicsData : IComponentData
    {
        /// <summary>Capsule collider radius.</summary>
        public float CapsuleRadius;

        /// <summary>Capsule collider height.</summary>
        public float CapsuleHeight;

        /// <summary>Center offset of the capsule.</summary>
        public float3 CapsuleCenter;

        /// <summary>Skin width for collision detection.</summary>
        public float SkinWidth;

        /// <summary>Slope limit in degrees.</summary>
        public float SlopeLimit;

        /// <summary>Step offset height.</summary>
        public float StepOffset;

        /// <summary>Ground check distance.</summary>
        public float GroundCheckDistance;

        /// <summary>Layer mask for ground detection.</summary>
        public int GroundLayerMask;
    }

    /// <summary>
    /// ECS equivalent of camera tracking data from ThirdPersonCamera.
    /// </summary>
    public struct CameraTargetData : IComponentData
    {
        /// <summary>Camera offset from target.</summary>
        public float3 Offset;

        /// <summary>Camera distance from target.</summary>
        public float Distance;

        /// <summary>Current camera yaw angle.</summary>
        public float Yaw;

        /// <summary>Current camera pitch angle.</summary>
        public float Pitch;

        /// <summary>Camera smoothing factor.</summary>
        public float Smoothing;

        /// <summary>Minimum and maximum pitch.</summary>
        public float2 PitchLimit;

        /// <summary>Minimum and maximum distance (zoom).</summary>
        public float2 ZoomLimit;

        /// <summary>Zoom speed.</summary>
        public float ZoomSpeed;

        /// <summary>Layer mask for collision avoidance.</summary>
        public int CollisionLayerMask;

        /// <summary>Collision check radius.</summary>
        public float CollisionRadius;
    }

    /// <summary>
    /// Tag marking an entity as a player-controlled character.
    /// </summary>
    public struct PlayerTag : IComponentData { }

    /// <summary>
    /// ECS equivalent of CharacterIK data for inverse kinematics.
    /// </summary>
    public struct CharacterIKData : IComponentData
    {
        /// <summary>Look-at target position.</summary>
        public float3 LookTarget;

        /// <summary>Body IK weight.</summary>
        public float BodyWeight;

        /// <summary>Head IK weight.</summary>
        public float HeadWeight;

        /// <summary>Eyes IK weight.</summary>
        public float EyesWeight;

        /// <summary>Clamp weight for look-at.</summary>
        public float ClampWeight;

        /// <summary>Overall IK weight.</summary>
        public float Weight;
    }
}
