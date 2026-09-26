using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;

        [Range(0, 1)]
        public float FootstepAudioVolume = 0.5f;

        [Space(10)]

        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value")]
        public float Gravity = -15.0f;

        [Space(10)]

        [Tooltip("Time required to pass before being able to jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state")]
        public float FallTimeout = 0.15f;


        // ============================================================
        // PLAYER GROUNDED
        // ============================================================

        [Header("Player Grounded")]

        [Tooltip("If the character is grounded or not")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;


        // ============================================================
        // CINEMACHINE
        // ============================================================

        [Header("Cinemachine")]

        [Tooltip("The follow target set in the Cinemachine Camera")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degrees to override the camera")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position")]
        public bool LockCameraPosition = false;

        public Vector2 LookSensitivity = new Vector2(7.5f, 5.0f);

        [Tooltip("Keyboard camera rotation speed")]
        public float CameraRotateSpeed = 120.0f;


        // ============================================================
        // CAMERA VARIABLES
        // ============================================================

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        private Vector3 _cameraStartingPosition;
        private Quaternion _cameraStartingRotation;

        public bool IsRespawning { get; set; } = false;


        // ============================================================
        // PLAYER VARIABLES
        // ============================================================

        private float _speed;
        private float _animationBlend;
        private float _targetRotation;
        private float _rotationVelocity;

        private float _verticalVelocity;

        private float _terminalVelocity = 53.0f;


        // ============================================================
        // TIMEOUTS
        // ============================================================

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;


        // ============================================================
        // ANIMATION IDS
        // ============================================================

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;


        // ============================================================
        // COMPONENTS
        // ============================================================

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;


        private const float _threshold = 0.01f;


        private bool _hasAnimator;


        // ============================================================
        // DEVICE
        // ============================================================

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput != null &&
                       _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            // Não usamos mais GameObject.FindGameObjectWithTag("MainCamera").
            // Cada player utiliza seu próprio CinemachineCameraTarget.
        }


        // ============================================================
        // START
        // ============================================================

        private void Start()
        {
            // Verifica se o CinemachineCameraTarget foi configurado
            if (CinemachineCameraTarget == null)
            {
                Debug.LogError(
                    "CinemachineCameraTarget não foi atribuído no " +
                    gameObject.name +
                    ". Arraste o objeto CinemachineCameraTarget para o campo no Inspector."
                );

                enabled = false;
                return;
            }


            _cinemachineTargetYaw =
                CinemachineCameraTarget.transform.rotation.eulerAngles.y;


            _hasAnimator = TryGetComponent(out _animator);

            _controller = GetComponent<CharacterController>();

            _input = GetComponent<StarterAssetsInputs>();


#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError(
                "Starter Assets package is missing dependencies. " +
                "Please use Tools/Starter Assets/Reinstall Dependencies to fix it"
            );
#endif


            if (_input == null)
            {
                Debug.LogError(
                    "StarterAssetsInputs não encontrado no " +
                    gameObject.name +
                    ". Adicione o componente StarterAssetsInputs ao Player."
                );

                enabled = false;
                return;
            }


            AssignAnimationIDs();


            // Salva posição inicial da câmera
            _cameraStartingPosition =
                CinemachineCameraTarget.transform.position;

            _cameraStartingRotation =
                CinemachineCameraTarget.transform.rotation;


            // Reseta os timers
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }


        // ============================================================
        // UPDATE
        // ============================================================

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();

            GroundedCheck();

            Move();
        }


        // ============================================================
        // LATE UPDATE
        // ============================================================

        private void LateUpdate()
        {
            CameraRotation();
        }


        // ============================================================
        // ANIMATION IDS
        // ============================================================

        private void AssignAnimationIDs()
        {
            _animIDSpeed =
                Animator.StringToHash("Speed");

            _animIDGrounded =
                Animator.StringToHash("Grounded");

            _animIDJump =
                Animator.StringToHash("Jump");

            _animIDFreeFall =
                Animator.StringToHash("FreeFall");

            _animIDMotionSpeed =
                Animator.StringToHash("MotionSpeed");
        }


        // ============================================================
        // GROUNDED CHECK
        // ============================================================

        private void GroundedCheck()
        {
            Vector3 spherePosition =
                new Vector3(
                    transform.position.x,
                    transform.position.y - GroundedOffset,
                    transform.position.z
                );


            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );


            if (_hasAnimator)
            {
                _animator.SetBool(
                    _animIDGrounded,
                    Grounded
                );
            }
        }


        // ============================================================
        // CAMERA ROTATION
        // ============================================================

        private void CameraRotation()
        {
            if (CinemachineCameraTarget == null)
                return;


            // --------------------------------------------------------
            // RESPawn
            // --------------------------------------------------------

            if (IsRespawning)
            {
                _cinemachineTargetYaw = 0f;

                _cinemachineTargetPitch = 0f;


                CinemachineCameraTarget.transform.position =
                    _cameraStartingPosition;


                CinemachineCameraTarget.transform.rotation =
                    _cameraStartingRotation;


                IsRespawning = false;

                return;
            }


            // --------------------------------------------------------
            // MOUSE / GAMEPAD
            // --------------------------------------------------------

            if (_input.look.sqrMagnitude >= _threshold &&
                !LockCameraPosition)
            {
                float deltaTimeMultiplier =
                    IsCurrentDeviceMouse
                        ? 1.0f
                        : Time.deltaTime;


                _cinemachineTargetYaw +=
                    _input.look.x *
                    deltaTimeMultiplier *
                    LookSensitivity.x;


                _cinemachineTargetPitch +=
                    _input.look.y *
                    deltaTimeMultiplier *
                    LookSensitivity.y;
            }


            // --------------------------------------------------------
            // TECLADO A / D
            // --------------------------------------------------------

            if (Mathf.Abs(_input.cameraRotate) > 0.01f &&
                !LockCameraPosition)
            {
                _cinemachineTargetYaw +=
                    _input.cameraRotate *
                    CameraRotateSpeed *
                    Time.deltaTime;
            }


            // --------------------------------------------------------
            // LIMITES
            // --------------------------------------------------------

            _cinemachineTargetYaw =
                ClampAngle(
                    _cinemachineTargetYaw,
                    float.MinValue,
                    float.MaxValue
                );


            _cinemachineTargetPitch =
                ClampAngle(
                    _cinemachineTargetPitch,
                    BottomClamp,
                    TopClamp
                );


            // --------------------------------------------------------
            // APLICA ROTAÇÃO
            // --------------------------------------------------------

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(
                    _cinemachineTargetPitch +
                    CameraAngleOverride,

                    _cinemachineTargetYaw,

                    0.0f
                );
        }


        // ============================================================
        // MOVEMENT
        // ============================================================

        private void Move()
        {
            float targetSpeed =
                _input.sprint
                    ? SprintSpeed
                    : MoveSpeed;


            if (_input.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }


            float currentHorizontalSpeed =
                new Vector3(
                    _controller.velocity.x,
                    0.0f,
                    _controller.velocity.z
                ).magnitude;


            float speedOffset = 0.1f;


            float inputMagnitude =
                _input.analogMovement
                    ? _input.move.magnitude
                    : 1f;


            // --------------------------------------------------------
            // ACELERAÇÃO / DESACELERAÇÃO
            // --------------------------------------------------------

            if (currentHorizontalSpeed <
                    targetSpeed - speedOffset ||

                currentHorizontalSpeed >
                    targetSpeed + speedOffset)
            {
                _speed =
                    Mathf.Lerp(
                        currentHorizontalSpeed,
                        targetSpeed * inputMagnitude,
                        Time.deltaTime *
                        SpeedChangeRate
                    );


                _speed =
                    Mathf.Round(
                        _speed * 1000f
                    ) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }


            _animationBlend =
                Mathf.Lerp(
                    _animationBlend,
                    targetSpeed,
                    Time.deltaTime *
                    SpeedChangeRate
                );


            if (_animationBlend < 0.01f)
            {
                _animationBlend = 0f;
            }


            // --------------------------------------------------------
            // DIREÇÃO DO INPUT
            // --------------------------------------------------------

            Vector3 inputDirection =
                new Vector3(
                    _input.move.x,
                    0.0f,
                    _input.move.y
                ).normalized;


            // --------------------------------------------------------
            // ROTAÇÃO DO PLAYER
            // --------------------------------------------------------

            if (_input.move != Vector2.zero)
            {
                _targetRotation =
                    Mathf.Atan2(
                        inputDirection.x,
                        inputDirection.z
                    ) * Mathf.Rad2Deg

                    +

                    // IMPORTANTE:
                    // Usa a câmera/pivô deste player,
                    // e não uma MainCamera global.
                    CinemachineCameraTarget
                        .transform
                        .eulerAngles
                        .y;


                float rotation =
                    Mathf.SmoothDampAngle(
                        transform.eulerAngles.y,
                        _targetRotation,
                        ref _rotationVelocity,
                        RotationSmoothTime
                    );


                transform.rotation =
                    Quaternion.Euler(
                        0.0f,
                        rotation,
                        0.0f
                    );
            }


            // --------------------------------------------------------
            // DIREÇÃO FINAL
            // --------------------------------------------------------

            Vector3 targetDirection =
                Quaternion.Euler(
                    0.0f,
                    _targetRotation,
                    0.0f
                ) * Vector3.forward;


            // --------------------------------------------------------
            // MOVIMENTA O PLAYER
            // --------------------------------------------------------

            _controller.Move(
                targetDirection.normalized *
                (_speed * Time.deltaTime)

                +

                new Vector3(
                    0.0f,
                    _verticalVelocity,
                    0.0f
                ) *
                Time.deltaTime
            );


            // --------------------------------------------------------
            // ANIMAÇÃO
            // --------------------------------------------------------

            if (_hasAnimator)
            {
                _animator.SetFloat(
                    _animIDSpeed,
                    _animationBlend
                );


                _animator.SetFloat(
                    _animIDMotionSpeed,
                    inputMagnitude
                );
            }
        }


        // ============================================================
        // JUMP AND GRAVITY
        // ============================================================

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;


                if (_hasAnimator)
                {
                    _animator.SetBool(
                        _animIDJump,
                        false
                    );

                    _animator.SetBool(
                        _animIDFreeFall,
                        false
                    );
                }


                // Mantém o personagem levemente preso ao chão
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }


                // ----------------------------------------------------
                // JUMP
                // ----------------------------------------------------

                if (_input.jump &&
                    _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity =
                        Mathf.Sqrt(
                            JumpHeight *
                            -2f *
                            Gravity
                        );


                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDJump,
                            true
                        );
                    }
                }


                // ----------------------------------------------------
                // JUMP TIMEOUT
                // ----------------------------------------------------

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -=
                        Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta =
                    JumpTimeout;


                // ----------------------------------------------------
                // FALL TIMEOUT
                // ----------------------------------------------------

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -=
                        Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDFreeFall,
                            true
                        );
                    }
                }


                // Não pode pular no ar
                _input.jump = false;
            }


            // --------------------------------------------------------
            // GRAVIDADE
            // --------------------------------------------------------

            if (_verticalVelocity <
                _terminalVelocity)
            {
                _verticalVelocity +=
                    Gravity *
                    Time.deltaTime;
            }
        }


        // ============================================================
        // CLAMP ANGLE
        // ============================================================

        private static float ClampAngle(
            float lfAngle,
            float lfMin,
            float lfMax)
        {
            if (lfAngle < -360f)
            {
                lfAngle += 360f;
            }


            if (lfAngle > 360f)
            {
                lfAngle -= 360f;
            }


            return Mathf.Clamp(
                lfAngle,
                lfMin,
                lfMax
            );
        }


        // ============================================================
        // GIZMOS
        // ============================================================

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen =
                new Color(
                    0.0f,
                    1.0f,
                    0.0f,
                    0.35f
                );


            Color transparentRed =
                new Color(
                    1.0f,
                    0.0f,
                    0.0f,
                    0.35f
                );


            Gizmos.color =
                Grounded
                    ? transparentGreen
                    : transparentRed;


            Gizmos.DrawSphere(
                new Vector3(
                    transform.position.x,
                    transform.position.y -
                    GroundedOffset,
                    transform.position.z
                ),
                GroundedRadius
            );
        }


        // ============================================================
        // FOOTSTEP
        // ============================================================

        private void OnFootstep(
            AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index =
                        Random.Range(
                            0,
                            FootstepAudioClips.Length
                        );


                    AudioSource.PlayClipAtPoint(
                        FootstepAudioClips[index],
                        transform.TransformPoint(
                            _controller.center
                        ),
                        FootstepAudioVolume
                    );
                }
            }
        }


        // ============================================================
        // LAND
        // ============================================================

        private void OnLand(
            AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(
                    LandingAudioClip,
                    transform.TransformPoint(
                        _controller.center
                    ),
                    FootstepAudioVolume
                );
            }
        }


        // ============================================================
        // RESET CAMERA
        // ============================================================

        public void ResetCameraRotation(
            float targetYaw)
        {
            if (CinemachineCameraTarget == null)
                return;


            _cinemachineTargetYaw =
                targetYaw;


            _cinemachineTargetPitch =
                0f;


            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(
                    _cinemachineTargetPitch,
                    _cinemachineTargetYaw,
                    0f
                );


            Debug.Log(
                $"Camera Yaw reset to {targetYaw} degrees."
            );
        }
    }
}