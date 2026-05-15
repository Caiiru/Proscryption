using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace proscryption
{
    /// <summary>
    /// PlayerController - MVC Controller Layer (Refactored)
    /// Handles movement input and physics
    /// Asks permission from PlayerModel before acting
    /// Broadcasts events when important things happen
    /// Does NOT handle combat (CombatSystem does that)
    /// Does NOT handle animations (PlayerView does that)
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")] [SerializeField]
        private float rotationSpeed = 10f;


        private float _rollCooldownTimer = 0f;
        private float _rollTimer = 0f;

        private Vector3 _lookingDirection;

        private bool _isAiming = false;
        //Data SO


        // ===== REFERENCES =====
        private PlayerModel _model;
        private PlayerView _view;
        private Rigidbody _rigidbody;
        private Camera _mainCamera;

        // ===== STATE =====
        [SerializeField] private Vector2 _moveInput = Vector2.zero;
        private Vector3 _currentVelocity = Vector3.zero;
        [SerializeField] private bool _canGetInput = true;


        // Ref
        [SerializeField] LayerMask _mouseLayerMask = 1 << 6; // Assuming "Ground" layer is layer 6
        CharacterInput _characterInput;

        void Awake()
        {
            _model = GetComponent<PlayerModel>();
            _view = GetComponent<PlayerView>();
            _rigidbody = GetComponent<Rigidbody>();
            _characterInput = GetComponent<CharacterInput>();
            RefreshMainCamera();
        }

        void OnEnable()
        {
            // Subscribe to movement and roll inputs via EventManager
            SetupEvents();
            RefreshMainCamera();
        }

        void SetupEvents()
        {
            PlayerEvents.OnPlayerMoveInput += HandleMoveInput;
            PlayerEvents.OnPlayerRollInput += HandleRollInput;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            EventManager.OnGameWin += OnGameWin;
            PlayerEvents.OnPlayerStateChanged += HandlePlayerState;
            PlayerEvents.OnPlayerAttackInput += HandleAttackInput;
            PlayerEvents.OnPlayerReloadInput += HandleReloadInput;

            _characterInput.OnDefaultStanceInput += () => _model.ChangeStance(PlayerStance.Standard);
            _characterInput.OnBloodStanceInput += () => _model.ChangeStance(PlayerStance.Blood);
            _characterInput.OnLightStanceInput += () => _model.ChangeStance(PlayerStance.Light);

            _characterInput.OnInteractInput += HandleInteractInput;
            PlayerEvents.OnPlayerCloseRewardScreen += HandleCloseRewardScreen;
            PlayerEvents.OnPlayerOpenRewardScreen += HandleOpenRewardScreen;
            PlayerEvents.OnPlayerAimInput += HandleAiming;
            PlayerEvents.OnPlayerReleaseAimInput += HandleReleaseAiming;
        }


        void OnDisable()
        {
            PlayerEvents.OnPlayerReleaseAimInput -= HandleReleaseAiming;
            PlayerEvents.OnPlayerAimInput += HandleAiming;
            PlayerEvents.OnPlayerCloseRewardScreen -= HandleCloseRewardScreen;
            PlayerEvents.OnPlayerOpenRewardScreen -= HandleOpenRewardScreen;
            PlayerEvents.OnPlayerMoveInput -= HandleMoveInput;
            PlayerEvents.OnPlayerRollInput -= HandleRollInput;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
            PlayerEvents.OnPlayerStateChanged -= HandlePlayerState;
            EventManager.OnGameWin -= OnGameWin;

            PlayerEvents.OnPlayerAttackInput -= HandleAttackInput;
            PlayerEvents.OnPlayerReloadInput -= HandleReloadInput;

            _characterInput.OnDefaultStanceInput -= () => _model.ChangeStance(PlayerStance.Standard);
            _characterInput.OnBloodStanceInput -= () => _model.ChangeStance(PlayerStance.Blood);
            _characterInput.OnLightStanceInput -= () => _model.ChangeStance(PlayerStance.Light);

            _characterInput.OnInteractInput -= HandleInteractInput;
        }

        void Start()
        {
            if (GameManager.Instance != null)
            {
                transform.position = GameManager.Instance.GetPlayerSpawnPointPosition(0);
            }

            _canGetInput = true;
        }

        // ===== INPUT HANDLERS =====

        /// <summary>
        /// Handle movement input from EventManager
        /// </summary>
        private void HandleMoveInput(Vector2 input)
        {
            if (!_canGetInput) return;
            _moveInput = input;

            // // Ask model if we can move
            // if (!_model.GetCanMove())
            // {
            //     _moveInput = Vector2.zero;
            //     return;
            // }

            // Update model state
            if (_moveInput.magnitude > 0.1f)
            {
                if (_model.CurrentState == PlayerState.Idle)
                    _model.ChangeState(PlayerState.Moving);
            }
            else
            {
                if (_model.CurrentState == PlayerState.Moving)
                    _model.ChangeState(PlayerState.Idle);
            }
        }

        private void HandleAiming()
        {
            _isAiming = true;
            _view.SetAiming(true);
        }

        private void HandleReleaseAiming()
        {
            _isAiming = false;
            _view.SetAiming(false);
        }


        private void HandleInteractInput(bool isPressed)
        {
            if (!_canGetInput) return;
            PlayerEvents.BroadcastPlayerCastInteract();
        }

        /// <summary>
        /// Handle roll input from EventManager
        /// </summary>
        private void HandleRollInput()
        {
            if (!_canGetInput) return;
            // Ask model if we can roll
            if (!_model.CanRoll())
            {
                return;
            }

            // Consume stamina
            if (!_model.TryConsumeStamina(_model.ROLL_STAMINA_COST))
            {
                return;
            }

            // Start roll
            // _model.ChangeState(PlayerState.Rolling);
            _model.SetInvulnerable(true, _model.rollDuration);
            _rollTimer = _model.rollDuration;
            _rollCooldownTimer = _model.rollCooldown;
            _model.isRolling = true;
            _model.ChangeState(PlayerState.Rolling);

            Vector2 rollDirection = GetCameraRelativeMovement(_moveInput);
            _view.RollAnimation(rollDirection);
        }


        private void HandleReloadInput()
        {
            if (!_canGetInput) return;
            if (!_model.CanReload()) return;
            if (!_model.TryConsumeStamina(_model.GetCurrentData().rollStaminaCost)) return;
            if (_model.CurrentState != PlayerState.Reloading)
            {
                _model.ChangeState(PlayerState.Reloading);
            }
        }

        private void HandleAttackInput()
        {
            if (!_canGetInput) return;

            if (!_model.CanAttack()) return;

            if (!_isAiming) return;

            if (!_model.TryConsumeStamina(_model.GetCurrentData().attackStaminaCost)) return;

            _model.ChangeState(PlayerState.Attacking);
        }

        void Update()
        {
            RotateTowardsMousePosition(Mouse.current.position.ReadValue());
            _view.UpdateInputAnimation(_moveInput.normalized, _lookingDirection.normalized);
        }

        // ===== PHYSICS LOOP =====

        void FixedUpdate()
        {
            if (!_canGetInput) return;
            // Always regenerate stamina
            _model.RegenerateStamina(Time.fixedDeltaTime);

            if (!_model.IsAlive) return;
            if (!_model.CanMove) return;


            UpdateTimers();

            // Handle rolling
            if (_model.isRolling)
            {
                HandleRolling();
            }
            else
            {
                // Handle normal movement
                HandleMovement();
            }
        }

        void LateUpdate()
        {
            if (!_model.IsAlive) return;
        }

        // ===== MOVEMENT LOGIC =====

        private void HandleMovement()
        {
            if (!_model.CanMove) return;
            Vector3 movement = GetCameraRelativeMovement(_moveInput);
            _currentVelocity = movement * _model.MoveSpeed;

            _currentVelocity.y = _rigidbody.linearVelocity.y;

            _rigidbody.linearVelocity = _currentVelocity;
        }

        private void HandleRolling()
        {
            // Apply roll force in the direction we're rolling
            Vector3 rollDirection = GetCameraRelativeMovement(_moveInput);
            // Vector3 rollDirection = _moveInput;
            if (rollDirection.magnitude < 0.1f)
            {
                // If no input, roll forward
                rollDirection = transform.forward;
            }

            // Apply roll velocity
            Vector3 rollVelocity = rollDirection * _model.rollForce;
            rollVelocity.y = _rigidbody.linearVelocity.y; // Preserve gravity

            _rigidbody.linearVelocity = rollVelocity;
            _currentVelocity = rollVelocity;

            // // Check if roll is finished
            // _rollTimer -= Time.fixedDeltaTime;
            // Debug.Log(_rollTimer);
            // if (_rollTimer <= 0)
            // {
            //     if (_moveInput.magnitude < 0.1f)
            //     {
            //         _model.ChangeState(PlayerState.Idle);
            //     }
            //     else
            //         _model.ChangeState(PlayerState.Moving);
            // }
        }


        // ===== HELPER METHODS =====

        private Vector3 GetCameraRelativeMovement(Vector2 input)
        {
            if (!_mainCamera)
                RefreshMainCamera();

            if (_mainCamera == null)
            {
                return Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);
            }

            // Get camera forward and right projected onto ground plane
            Vector3 forward = Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_mainCamera.transform.right, Vector3.up).normalized;

            // Combine based on input magnitude
            Vector3 movement = (forward * input.y + right * input.x).normalized * input.magnitude;

            return movement;
        }

        private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            RefreshMainCamera();
        }

        private void RefreshMainCamera()
        {
            if (_mainCamera != null && _mainCamera.isActiveAndEnabled)
                return;

            _mainCamera = Camera.main;

            if (_mainCamera != null)
                return;

            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cameraCandidate in cameras)
            {
                if (cameraCandidate.isActiveAndEnabled)
                {
                    _mainCamera = cameraCandidate;
                    break;
                }
            }
        }

        private void RotateTowardsVelocity(Vector3 velocity)
        {
            if (velocity.magnitude < 0.1f) return;

            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }

        private void RotateTowardsDirection(Vector3 direction)
        {
            if (direction.magnitude < 0.1f) return;
            if (!_model.CanRotate()) return;

            // Vector3 flatDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            // if (flatDirection.magnitude < 0.1f) return;

            transform.rotation = Quaternion.LookRotation(direction.normalized);
            // Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            // transform.rotation = Quaternion.Lerp(
            //     transform.rotation,
            //     targetRotation,
            //     // rotationSpeed * Time.fixedDeltaTime
            //     0.05f
            // );
        }

        private void RotateTowardsMousePosition(Vector2 mousePosition)
        {
            if (_model.CurrentState == PlayerState.Rolling) return;
            if (_mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, _mouseLayerMask))
            {
                Vector3 targetPoint = hitInfo.point;
                targetPoint.y = transform.position.y; // Keep player rotation on horizontal plane
                PlayerEvents.BroadcastMouseLookInput(new Vector2(hitInfo.point.x, hitInfo.point.z));
                Vector3 direction = targetPoint - transform.position;
                _lookingDirection = direction;
                RotateTowardsDirection(_lookingDirection);
                // Debug.DrawLine(transform.position, transform.position + direction * 2f, Color.green, 0.5f);
            }
            // Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100f, Color.red, 0.5f);
            // Debug.DrawLine(transform.position, transform.position + transform.forward * 2f, Color.blue, 0.5f);
        }

        public Vector3 GetLookingDirection()
        {
            return _lookingDirection;
        }

        private void UpdateTimers()
        {
            if (_rollCooldownTimer > 0)
            {
                _rollCooldownTimer -= Time.fixedDeltaTime;
            }
        }

        private void HandlePlayerState(PlayerState oldstate, PlayerState newState)
        {
            if (newState == PlayerState.Dead)
            {
                _model.SetCantMove();
            }
        }

        private void HandleOpenRewardScreen()
        {
            _canGetInput = false;
            _model.ChangeState(PlayerState.Menu);
        }

        private void HandleCloseRewardScreen()
        {
            _canGetInput = true;
            _model.ChangeState(PlayerState.Idle);
        }


        public void OnGameWin()
        {
            _model.SetCantMove();
        }
    }
}