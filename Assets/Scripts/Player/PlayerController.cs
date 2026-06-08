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

        private bool isBackwards = false;
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
        [SerializeField] private bool _isRunningInput;

        // Ref
        [SerializeField] LayerMask _mouseLayerMask = 1 << 6; // Assuming "Ground" layer is layer 6
        CharacterInput _characterInput;
        [SerializeField] private GameObject _camera;

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
            PlayerEvents.OnPlayerRunInput += HandleRunInput;
            PlayerEvents.OnPlayerReleaseRunInput += HandleReleaseRunInput;
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
        }


        void OnDisable()
        {
            PlayerEvents.OnPlayerCloseRewardScreen -= HandleCloseRewardScreen;
            PlayerEvents.OnPlayerOpenRewardScreen -= HandleOpenRewardScreen;
            PlayerEvents.OnPlayerMoveInput -= HandleMoveInput;
            PlayerEvents.OnPlayerRunInput -= HandleRunInput;
            PlayerEvents.OnPlayerReleaseRunInput -= HandleReleaseRunInput;
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

            _camera.transform.SetParent(GameManager.Instance.gameObject.transform);
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


        private void HandleInteractInput(bool isPressed)
        {
            if (!_canGetInput) return;
            PlayerEvents.BroadcastPlayerCastInteract();
        }

        private void HandleRunInput()
        {
            if (!_canGetInput) return;
            // Ask model if we can roll
            if (!_model.CanRun())
            {
                return;
            }

            _isRunningInput = true;
            // Consume stamina


            // Start roll
            // _model.ChangeState(PlayerState.Rolling);
            // _model.SetInvulnerable(true, _model.rollDuration);
            // _rollTimer = _model.rollDuration;
            // _rollCooldownTimer = _model.rollCooldown; 

            //
            // if (_model.CurrentState == PlayerState.Reloading)
            //     _view.StopReloading();

            // Vector2 rollDirection = GetCameraRelativeMovement(_moveInput);
            // _view.RollAnimation(rollDirection);
        }

        private void HandleReleaseRunInput()
        {
            if (!_canGetInput) return;
            _isRunningInput = false;
            if (_moveInput != Vector2.zero)
                _model.ChangeState(PlayerState.Moving);
            else
            {
                _model.ChangeState(PlayerState.Idle);
            }
        }


        private void HandleReloadInput()
        {
            if (!_canGetInput) return;
            if (!_model.CanReload()) return;
            if (_model.CurrentState != PlayerState.Reloading)
            {
                _model.ChangeState(PlayerState.Reloading);
            }
        }

        private void HandleAttackInput()
        {
            if (!_canGetInput) return;

            if (!_model.CanAttack()) return;

            // if (!_isAiming) return;

            if (!_model.TryConsumeStamina(_model.GetCurrentData().attackStaminaCost)) return;
            if (_model.CanAttack())
                _model.ChangeState(PlayerState.Attacking);
        }

        void Update()
        {
            RotateTowardsMousePosition(Mouse.current.position.ReadValue());

            if (_moveInput == Vector2.zero && _isRunningInput && _model.CurrentState == PlayerState.Running)
            {
                //end run
                _isRunningInput = false;
                _model.ChangeState(PlayerState.Idle);
            }

            if (_moveInput == Vector2.zero || !_isRunningInput) return;


            if (_model.CurrentState != PlayerState.Running)
                _model.ChangeState(PlayerState.Running);
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


            HandleMovement();
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

            if (_model.isRunning)
            {
                //RotateTowardsDirection(new Vector3(transform.position.x + _moveInput.x, transform.position.y,
                //  transform.position.z + _moveInput.y));
                _currentVelocity = movement * _model.MoveSpeed;
                RotateTowardsVelocity(_currentVelocity);
            }
            else
            {
                _currentVelocity = isBackwards
                    ? movement * (_model.MoveSpeed * _model.moveSpeedReloadMultiplier)
                    : movement * _model.MoveSpeed;
            }

            _currentVelocity.y = _rigidbody.linearVelocity.y;

            _rigidbody.linearVelocity = _currentVelocity;
        }

        // private void HandleRolling()
        // {
        //     // Apply roll force in the direction we're rolling
        //     Vector3 rollDirection = GetCameraRelativeMovement(_moveInput);
        //     // Vector3 rollDirection = _moveInput;
        //     if (rollDirection.magnitude < 0.1f)
        //     {
        //         // If no input, roll forward
        //         rollDirection = transform.forward;
        //     }
        //
        //     // Apply roll velocity
        //     Vector3 rollVelocity = rollDirection * _model.rollForce;
        //     rollVelocity.y = _rigidbody.linearVelocity.y; // Preserve gravity
        //
        //     _rigidbody.linearVelocity = rollVelocity;
        //     _currentVelocity = rollVelocity;
        //
        //     // // Check if roll is finished
        //     // _rollTimer -= Time.fixedDeltaTime;
        //     // Debug.Log(_rollTimer);
        //     // if (_rollTimer <= 0)
        //     // {
        //     //     if (_moveInput.magnitude < 0.1f)
        //     //     {
        //     //         _model.ChangeState(PlayerState.Idle);
        //     //     }
        //     //     else
        //     //         _model.ChangeState(PlayerState.Moving);
        //     // }
        // }


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

            if (movement.magnitude > 0.1f)
            {
                float dotProduct = Vector3.Dot(transform.forward, movement.normalized);
                isBackwards = dotProduct < 0.5f;
                // Debug.Log(
                // $"is Backwards: {isBackwards},  forward: {transform.forward}, right: {right}, dotProduct: {dotProduct}");
            }

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
            if (_mainCamera == null) return;
            if (_model.CurrentState != PlayerState.Running)
                _view.UpdateInputAnimation(_moveInput.normalized, _lookingDirection.normalized);

            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, _mouseLayerMask))
            {
                Vector3 targetPoint = hitInfo.point;
                targetPoint.y = transform.position.y; // Keep player rotation on horizontal plane
                PlayerEvents.BroadcastMouseLookInput(new Vector2(hitInfo.point.x, hitInfo.point.z));
                Vector3 direction = targetPoint - transform.position;
                _lookingDirection = direction;

                if (_model.CurrentState != PlayerState.Running)
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