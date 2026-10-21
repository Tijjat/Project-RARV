using System;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Gameplay
{
    public class PlayerMovementController : NetworkBehaviour
    {
        #region Attributes
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.5f;

        [Header("Audio elements")]
        [Tooltip("Audio played on landing")]
        public AudioClip LandingAudioClip;
        [Tooltip("Audio played on walking")]
        public AudioClip[] FootstepAudioClips;
        [Tooltip("Volume at which the footsteps are played")]
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = true;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        public bool cursorLocked = true;
        ActionInput _actionInput;

        [SerializeField] protected float _jumpForce = 5;


        private Rigidbody _rigidbody;
        private Vector2 m_Rotation;

        [SerializeField] private Animator _animator;

        // animation
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        private float _animationBlend;

        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private PlayerInput _playerInput;
        private CharacterController _controller;
        private const float _threshold = 0.01f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Header("Knockback")]
        [SerializeField] private float knockbackDamping = 5f;

        private Vector3 _knockbackVelocity;

        [Header("Air Control")]
        [SerializeField, Range(0f, 1f)]
        private float airControl = 0.4f;
        private MovingPlatform currentPlatform;
        private MovingPlatform previousPlatform;
        private Vector3 previousPlatformPosition;
        #endregion

        #region Unity Methods
        private void Update()
        {
            if (!IsOwner)
                return;

            GroundedCheck();
            Jump();
            Move();
        }

        private void LateUpdate()
        {
            if (!IsOwner)
                return;

            Look();
        }

        #endregion

        #region Netcode
        public override void OnNetworkSpawn()
        {
            if (_controller == null)
            {
                _controller = GetComponent<CharacterController>();
            }
            if (!IsOwner)
            {
                return;
            }
            AssignAnimationIDs();

            if (_actionInput == null)
            {
                _actionInput = GetComponent<ActionInput>();
            }

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
            if (CinemachineCameraTarget == null)
            {
                Debug.LogError("No cinemachine camera target assigned.");
            }
            _playerInput = GetComponent<PlayerInput>();
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            Cursor.lockState = CursorLockMode.Locked;
        }


        #endregion

        #region Movements


        private void Move()
        {
            Vector2 move = _actionInput.move;

            float targetSpeed = _actionInput.sprint
                ? SprintSpeed
                : MoveSpeed;

            if (move == Vector2.zero)
                targetSpeed = 0f;

            _speed = Mathf.MoveTowards(
                _speed,
                targetSpeed,
                SpeedChangeRate * Time.deltaTime
            );

            _animationBlend = _speed;

            if (_animationBlend < 0.01f)
                _animationBlend = 0f;

            // Movement direction relative to the player
            Vector3 inputDirection = Vector3.zero;

            if (move != Vector2.zero)
            {
                inputDirection =
                    transform.right * move.x +
                    transform.forward * move.y;

                inputDirection.Normalize();
            }

            // Normal movement
            float controlMultiplier = Grounded ? 1f : airControl;

            Vector3 normalMovement =
                inputDirection * _speed * controlMultiplier;

            // Cannon knockback
            Vector3 externalMovement = _knockbackVelocity;

            // Gravity and jumping
            Vector3 verticalMovement =
                Vector3.up * _verticalVelocity;

            // Detect platform before moving
            DetectMovingPlatform();

            // Carry the player with the platform
            Vector3 platformMovement = Vector3.zero;

            if (currentPlatform != null && Grounded)
            {
                if (currentPlatform == previousPlatform)
                {
                    platformMovement =
                        currentPlatform.transform.position -
                        previousPlatformPosition;
                }

                previousPlatformPosition =
                    currentPlatform.transform.position;
            }

            previousPlatform = currentPlatform;

            // Combine velocity-based movement
            Vector3 finalMovement =
                normalMovement +
                externalMovement +
                verticalMovement;

            // Apply everything in one CharacterController.Move()
            if (_controller.enabled)
            {
                _controller.Move(
                    finalMovement * Time.deltaTime +
                    platformMovement
                );
            }

            // Reduce knockback over time
            _knockbackVelocity = Vector3.MoveTowards(
                _knockbackVelocity,
                Vector3.zero,
                knockbackDamping * Time.deltaTime
            );

            // Update animations
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, 1f);
        }


        protected void Look()
        {
            if (!cursorLocked)
            {
                return;
            }
            var look = _actionInput.look;
            if (look.sqrMagnitude < _threshold)
                return;
            _cinemachineTargetPitch += look.y * RotationSpeed * Time.deltaTime;
            _rotationVelocity =  look.x * RotationSpeed * Time.deltaTime;

            // clamp our pitch rotation
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Update Cinemachine camera target pitch
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);
        }

        public void Jump()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
                if (_actionInput.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    _actionInput.jump = false;
                    _animator.SetBool(_animIDJump, true);
                }
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }

                // if we are not grounded, do not jump
                _actionInput.jump = false;
                _animator.SetBool(_animIDFreeFall, true);
            }
            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }



        #endregion

        #region HelperFunctions
        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset + (GroundedRadius / 2),
                transform.position.z);
            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                ~LayerMask.GetMask("Player"),
                QueryTriggerInteraction.Ignore
            );
            _animator.SetBool(_animIDGrounded, Grounded);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
        #endregion

        #region Events
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Cursor.lockState = CursorLockMode.Locked;
                return;
            }
            Cursor.lockState = CursorLockMode.None;
        }


        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = UnityEngine.Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public void ApplyKnockback(Vector3 force)
        {
            if (!IsOwner)
                return;

            _knockbackVelocity = new Vector3(force.x, 0f, force.z);

            if (force.y > 0f)
            {
                _verticalVelocity = force.y;
                Grounded = false;
            }

            Debug.Log($"Knockback applied: {force}");
        }

        private void DetectMovingPlatform()
        {
            currentPlatform = null;

            if (!Grounded || !_controller.enabled)
                return;

            // Bottom of the CharacterController capsule
            Vector3 feetPosition =
                transform.TransformPoint(_controller.center) -
                Vector3.up * (_controller.height * 0.5f);

            Vector3 rayOrigin = feetPosition + Vector3.up * 0.2f;

            if (Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                0.5f,
                ~LayerMask.GetMask("Player"),
                QueryTriggerInteraction.Ignore
            ))
            {
                currentPlatform =
                    hit.collider.GetComponentInParent<MovingPlatform>();
            }
        }

        public void Bounce(float launchForce)
        {
            if (!IsOwner)
                return;

            // Replace vertical velocity with upward launch velocity
            _verticalVelocity = launchForce;

            Grounded = false;

            // Prevent the player from jumping again immediately
            _jumpTimeoutDelta = JumpTimeout;

            Debug.Log($"Trampoline bounce: {launchForce}");
        }

        public void ApplyWindForce(Vector3 acceleration)
        {
            if (!IsOwner)
                return;

            // Continuous horizontal acceleration
            Vector3 horizontalAcceleration =
                Vector3.ProjectOnPlane(acceleration, Vector3.up);

            _knockbackVelocity +=
                horizontalAcceleration * Time.fixedDeltaTime;

            // Prevent excessive wind speed
            const float maxExternalSpeed = 15f;

            _knockbackVelocity = Vector3.ClampMagnitude(
                _knockbackVelocity,
                maxExternalSpeed
            );
        }
        public void ResetMovementState()
        {
            _verticalVelocity = 0f;
            _knockbackVelocity = Vector3.zero;
            _speed = 0f;

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            currentPlatform = null;
            previousPlatform = null;

            Grounded = false;
        }


        #endregion
    }
}