using System;
using Unity.VisualScripting;
using UnityEngine;

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

        // ===== REFERENCES =====
        private PlayerModel _model;
        private PlayerView _view;
        private Rigidbody _rigidbody;
        private Camera _mainCamera;

        // ===== STATE =====
        [SerializeField] private Vector2 _moveInput = Vector2.zero;
        private Vector3 _currentVelocity = Vector3.zero;
        private float _rollCooldownTimer = 0f;
        private float _rollTimer = 0f;
        private bool _isRolling = false;

        void Awake()
        {
            _model = GetComponent<PlayerModel>();
            _view = GetComponent<PlayerView>();
            _rigidbody = GetComponent<Rigidbody>();
            _mainCamera = Camera.main;

            if (_model == null) Debug.LogError("[PlayerController] PlayerModel not found!");
            if (_view == null) Debug.LogError("[PlayerController] PlayerView not found!");
            if (_rigidbody == null) Debug.LogError("[PlayerController] Rigidbody not found!");
        }

        void OnEnable()
        {
            // Subscribe to movement and roll inputs via EventManager
            EventManager.OnPlayerMoveInput += HandleMoveInput;
            EventManager.OnPlayerRollInput += HandleRollInput;
        }

        void OnDisable()
        {
            EventManager.OnPlayerMoveInput -= HandleMoveInput;
            EventManager.OnPlayerRollInput -= HandleRollInput;
        }

        // ===== INPUT HANDLERS =====

        /// <summary>
        /// Handle movement input from EventManager
        /// </summary>
        private void HandleMoveInput(Vector2 input)
        {
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
            if (_moveInput.magnitude > 0.1f)
            {
                RotateTowardsVelocity(_currentVelocity);
            }
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
                _model.SetState(PlayerState.Idle);
            }
        }

        // ===== HELPER METHODS =====

        private Vector3 GetCameraRelativeMovement(Vector2 input)
        {
            if (_mainCamera == null) return Vector3.zero;

            // Get camera forward and right projected onto ground plane
            Vector3 forward = Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_mainCamera.transform.right, Vector3.up).normalized;

            // Combine based on input magnitude
            Vector3 movement = (forward * input.y + right * input.x).normalized * input.magnitude;

            return movement;
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

        private void UpdateTimers()
        {
            if (_rollCooldownTimer > 0)
            {
                _rollCooldownTimer -= Time.fixedDeltaTime;
            }
        }

        // ===== PUBLIC DEBUG METHODS =====

        public void PrintState()
        {
            Debug.Log($"[PlayerController] State: {_model.CurrentState} | Health: {_model.CurrentHealth}/{_model.MaxHealth} | Stamina: {_model.CurrentStamina}/{_model.MaxStamina}", gameObject);
        }
    }
}
