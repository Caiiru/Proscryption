using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace proscryption
{
    public class CharacterInput : MonoBehaviour
    {
        private PlayerInput _playerInput;

        #region Input Actions
        private InputAction _moveAction;
        private InputAction _rollAction;
        private InputAction _interactAction;
        private InputAction _lookAction;
        #endregion

        #region Public Input Properties
        /// <summary>
        /// Valor atual do input de movimento (X: horizontal, Y: vertical)
        /// </summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>
        /// Valor atual do input de roll
        /// </summary>
        public bool RollInput { get; private set; }

        /// <summary>
        /// Valor atual do input de interação
        /// </summary>
        public bool InteractInput { get; private set; }

        /// <summary>
        /// Valor atual do input de câmera (X: horizontal, Y: vertical)
        /// </summary>
        public Vector2 LookInput { get; private set; }
        #endregion

        #region Public Callbacks (Opcional)
        /// <summary>
        /// Callback acionado quando o movimento muda
        /// </summary>
        public Action<Vector2> OnMoveInput;

        /// <summary>
        /// Callback acionado quando roll é pressionado
        /// </summary>
        public Action<bool> OnRollInput;

        /// <summary>
        /// Callback acionado quando interact é pressionado
        /// </summary>
        public Action<bool> OnInteractInput;

        /// <summary>
        /// Callback acionado quando o look muda
        /// </summary>
        public Action<Vector2> OnLookInput;
        #endregion

        private void Awake()
        {
            InitializeInput();
        }

        private void InitializeInput()
        {
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput == null)
            {
                Debug.LogError("PlayerInput component não encontrado!");
                return;
            }

            // Obter referências das ações
            _moveAction = _playerInput.actions["Move"];
            _rollAction = _playerInput.actions["Roll"];
            _interactAction = _playerInput.actions["Interact"];
            _lookAction = _playerInput.actions["Look"];

            // Registrar callbacks
            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            if (_moveAction != null)
            {
                _moveAction.performed += HandleMoveInput;
                _moveAction.canceled += HandleMoveInput;
            }

            if (_rollAction != null)
            {
                _rollAction.performed += HandleRollInput;
                _rollAction.canceled += HandleRollInput;
            }

            if (_interactAction != null)
            {
                _interactAction.performed += HandleInteractInput;
                _interactAction.canceled += HandleInteractInput;
            }

            if (_lookAction != null)
            {
                _lookAction.performed += HandleLookInput;
                _lookAction.canceled += HandleLookInput;
            }
        }

        private void UnregisterCallbacks()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= HandleMoveInput;
                _moveAction.canceled -= HandleMoveInput;
            }

            if (_rollAction != null)
            {
                _rollAction.performed -= HandleRollInput;
                _rollAction.canceled -= HandleRollInput;
            }

            if (_interactAction != null)
            {
                _interactAction.performed -= HandleInteractInput;
                _interactAction.canceled -= HandleInteractInput;
            }

            if (_lookAction != null)
            {
                _lookAction.performed -= HandleLookInput;
                _lookAction.canceled -= HandleLookInput;
            }
        }

        #region Input Handlers
        private void HandleMoveInput(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
            OnMoveInput?.Invoke(MoveInput);
        }

        private void HandleRollInput(InputAction.CallbackContext context)
        {
            RollInput = context.ReadValueAsButton();
            Debug.Log("RollInput: " + RollInput.ToString());
            OnRollInput?.Invoke(RollInput);
        }

        private void HandleInteractInput(InputAction.CallbackContext context)
        {
            InteractInput = context.ReadValueAsButton();
            OnInteractInput?.Invoke(InteractInput);
        }

        private void HandleLookInput(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
            OnLookInput?.Invoke(LookInput);
        }
        #endregion

        private void OnDestroy()
        {
            UnregisterCallbacks();
        }
    }
}
