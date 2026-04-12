
using UnityEngine;
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
        [Header("Movement Settings")]
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Roll Settings")]
        [SerializeField] private float rollForce = 20f;
        [SerializeField] private float rollDuration = 0.5f;
        [SerializeField] private float rollCooldown = 1.5f;
        [SerializeField] private const int ROLL_STAMINA_COST = 20;

        private float _rollCooldownTimer = 0f;
        private float _rollTimer = 0f;
        private bool _isRolling = false;


        // ===== REFERENCES =====
        private PlayerModel _model;
        private PlayerView _view;
        private Rigidbody _rigidbody;
        private Camera _mainCamera;

        // ===== STATE =====
        [SerializeField] private Vector2 _moveInput = Vector2.zero;
        private Vector3 _currentVelocity = Vector3.zero;
        private bool _canGetInput = true;


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

            if (_model == null) Debug.LogError("[PlayerController] PlayerModel not found!");
            if (_view == null) Debug.LogError("[PlayerController] PlayerView not found!");
            if (_rigidbody == null) Debug.LogError("[PlayerController] Rigidbody not found!");
            if (_characterInput == null) Debug.LogError("[PlayerController] CharacterInput not found!");
        }

        void OnEnable()
        {
            this._characterInput.OnLookInput += HandleLookInput;
            // Subscribe to movement and roll inputs via EventManager
            EventManager.OnPlayerMoveInput += HandleMoveInput;
            EventManager.OnPlayerRollInput += HandleRollInput;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            EventManager.OnGameWin += OnGameWin;
            EventManager.OnPlayerStateChanged += HandlePlayerState;

            _characterInput.OnDefaultStanceInput += () => _model.ChangeStance(PlayerStance.Standard);
            _characterInput.OnBloodStanceInput += () => _model.ChangeStance(PlayerStance.Blood);
            _characterInput.OnLightStanceInput += () => _model.ChangeStance(PlayerStance.Light);
            RefreshMainCamera();
        }


        void OnDisable()
        {
            this._characterInput.OnLookInput -= HandleLookInput;
            EventManager.OnPlayerMoveInput -= HandleMoveInput;
            EventManager.OnPlayerRollInput -= HandleRollInput;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
            EventManager.OnPlayerStateChanged -= HandlePlayerState;
            EventManager.OnGameWin -= OnGameWin;

            
            _characterInput.OnDefaultStanceInput -= () => _model.ChangeStance(PlayerStance.Standard);
            _characterInput.OnBloodStanceInput -= () => _model.ChangeStance(PlayerStance.Blood);
            _characterInput.OnLightStanceInput -= () => _model.ChangeStance(PlayerStance.Light);
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
                    _model.SetState(PlayerState.Moving);
            }
            else
            {
                if (_model.CurrentState == PlayerState.Moving)
                    _model.SetState(PlayerState.Idle);
            }
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
                Debug.Log("[PlayerController] Cannot roll - state doesn't allow it or not enough stamina", gameObject);
                return;
            }

            // Consume stamina
            if (!_model.TryConsumeStamina(ROLL_STAMINA_COST))
            {
                Debug.Log("[PlayerController] Not enough stamina to roll", gameObject);
                return;
            }

            // Start roll
            _model.SetState(PlayerState.Rolling);
            _model.SetInvulnerable(true, rollDuration);
            _rollTimer = rollDuration;
            _rollCooldownTimer = rollCooldown;
            _isRolling = true;
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
            if (_isRolling)
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

            // Update animation with current velocity
            // _view.UpdateMovementAnimation(_currentVelocity);
            _view.UpdateInputAnimation(_moveInput);
            // Debug.Log("late update");
        }

        // ===== MOVEMENT LOGIC =====

        private void HandleMovement()
        {
            // Calculate camera-relative movement direction
            if (!_model.CanMove) return;
            Vector3 movement = GetCameraRelativeMovement(_moveInput);
            _currentVelocity = movement * _model.MoveSpeed;

            // Preserve Y velocity (gravity)
            _currentVelocity.y = _rigidbody.linearVelocity.y;

            // Apply velocity
            _rigidbody.linearVelocity = _currentVelocity;

            // Rotate player toward movement direction
            // if (_moveInput.magnitude > 0.1f)
            // {
            //     RotateTowardsVelocity(_currentVelocity);
            // }
        }

        private void HandleRolling()
        {
            // Apply roll force in the direction we're rolling
            Vector3 rollDirection = GetCameraRelativeMovement(_moveInput);
            if (rollDirection.magnitude < 0.1f)
            {
                // If no input, roll forward
                rollDirection = transform.forward;
            }

            // Apply roll velocity
            Vector3 rollVelocity = rollDirection * rollForce;
            rollVelocity.y = _rigidbody.linearVelocity.y; // Preserve gravity

            _rigidbody.linearVelocity = rollVelocity;
            _currentVelocity = rollVelocity;

            // Check if roll is finished
            _rollTimer -= Time.fixedDeltaTime;
            if (_rollTimer <= 0)
            {
                _isRolling = false;
                if (_moveInput.magnitude < 0.1f)
                {
                    _model.SetState(PlayerState.Idle);
                }
                else
                    _model.SetState(PlayerState.Moving);
            }
        }
        private void HandleLookInput(Vector2 input)
        {
            if (_mainCamera == null) return;
            RotateTowardsMousePosition(input);
        }

        // ===== HELPER METHODS =====

        private Vector3 GetCameraRelativeMovement(Vector2 input)
        {
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
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, _mouseLayerMask))
            {
                Vector3 targetPoint = hitInfo.point;
                targetPoint.y = transform.position.y; // Keep player rotation on horizontal plane
                EventManager.BroadcastMouseLookInput(new Vector2(hitInfo.point.x, hitInfo.point.z));
                Vector3 direction = targetPoint - transform.position;
                RotateTowardsDirection(direction);
                // Debug.DrawLine(transform.position, transform.position + direction * 2f, Color.green, 0.5f);
            }
            // Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100f, Color.red, 0.5f);
            // Debug.DrawLine(transform.position, transform.position + transform.forward * 2f, Color.blue, 0.5f);
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

        // ===== PUBLIC DEBUG METHODS =====

        public void PrintState()
        {
            Debug.Log($"[PlayerController] State: {_model.CurrentState} | Health: {_model.CurrentHealth}/{_model.MaxHealth} | Stamina: {_model.CurrentStamina}/{_model.MaxStamina}", gameObject);
        }

        public void OnGameWin()
        {
            _model.SetCantMove();
        }
    }
}
