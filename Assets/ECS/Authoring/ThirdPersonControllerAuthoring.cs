using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using RPG.ECS.Controller;

namespace RPG.ECS.Authoring
{
    /// <summary>
    /// Authoring component for converting ThirdPersonController MonoBehaviour to ECS.
    /// Mirrors DevionGames.ThirdPersonController with movement parameters,
    /// physics settings, and camera configuration.
    /// </summary>
    public class ThirdPersonControllerAuthoring : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Base movement speed.")]
        public float speed = 5f;

        [Tooltip("Speed multiplier applied to base speed.")]
        public float speedMultiplier = 1f;

        [Tooltip("Rotation speed for turning.")]
        public float rotationSpeed = 10f;

        [Tooltip("Jump force.")]
        public float jumpForce = 8f;

        [Tooltip("Gravity multiplier.")]
        public float gravityMultiplier = 2f;

        [Header("Physics")]
        [Tooltip("Capsule collider radius.")]
        public float capsuleRadius = 0.3f;

        [Tooltip("Capsule collider height.")]
        public float capsuleHeight = 1.8f;

        [Tooltip("Center offset of the capsule.")]
        public Vector3 capsuleCenter = new Vector3(0f, 0.9f, 0f);

        [Tooltip("Skin width for collision detection.")]
        public float skinWidth = 0.08f;

        [Tooltip("Maximum slope angle in degrees.")]
        public float slopeLimit = 45f;

        [Tooltip("Step offset height.")]
        public float stepOffset = 0.3f;

        [Tooltip("Ground check distance.")]
        public float groundCheckDistance = 0.2f;

        [Tooltip("Layer mask for ground detection.")]
        public LayerMask groundLayerMask = ~0;

        [Header("Camera")]
        [Tooltip("Camera offset from target.")]
        public Vector3 cameraOffset = new Vector3(0f, 1.5f, 0f);

        [Tooltip("Default camera distance.")]
        public float cameraDistance = 5f;

        [Tooltip("Camera smoothing factor.")]
        public float cameraSmoothing = 10f;

        [Tooltip("Camera pitch limits (min, max).")]
        public Vector2 pitchLimit = new Vector2(-30f, 60f);

        [Tooltip("Camera zoom limits (min, max).")]
        public Vector2 zoomLimit = new Vector2(2f, 10f);

        [Tooltip("Camera zoom speed.")]
        public float zoomSpeed = 2f;

        [Tooltip("Camera collision layer mask.")]
        public LayerMask collisionLayerMask = ~0;

        [Tooltip("Camera collision radius.")]
        public float collisionRadius = 0.2f;

        public class Baker : Baker<ThirdPersonControllerAuthoring>
        {
            public override void Bake(ThirdPersonControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Player tag
                AddComponent<PlayerTag>(entity);

                // Input data (zeroed, filled at runtime)
                AddComponent(entity, new InputData());

                // Movement data
                AddComponent(entity, new MovementData
                {
                    Speed = authoring.speed,
                    SpeedMultiplier = authoring.speedMultiplier,
                    RotationSpeed = authoring.rotationSpeed,
                    JumpForce = authoring.jumpForce,
                    GravityMultiplier = authoring.gravityMultiplier,
                    IsGrounded = true
                });

                // Character state
                AddComponent(entity, new CharacterStateData
                {
                    CurrentState = MotionStateType.Idle,
                    PreviousState = MotionStateType.Idle
                });

                // Physics data
                AddComponent(entity, new PhysicsData
                {
                    CapsuleRadius = authoring.capsuleRadius,
                    CapsuleHeight = authoring.capsuleHeight,
                    CapsuleCenter = authoring.capsuleCenter,
                    SkinWidth = authoring.skinWidth,
                    SlopeLimit = authoring.slopeLimit,
                    StepOffset = authoring.stepOffset,
                    GroundCheckDistance = authoring.groundCheckDistance,
                    GroundLayerMask = authoring.groundLayerMask.value
                });

                // Camera target data
                AddComponent(entity, new CameraTargetData
                {
                    Offset = authoring.cameraOffset,
                    Distance = authoring.cameraDistance,
                    Smoothing = authoring.cameraSmoothing,
                    PitchLimit = new float2(authoring.pitchLimit.x, authoring.pitchLimit.y),
                    ZoomLimit = new float2(authoring.zoomLimit.x, authoring.zoomLimit.y),
                    ZoomSpeed = authoring.zoomSpeed,
                    CollisionLayerMask = authoring.collisionLayerMask.value,
                    CollisionRadius = authoring.collisionRadius
                });
            }
        }
    }
}
