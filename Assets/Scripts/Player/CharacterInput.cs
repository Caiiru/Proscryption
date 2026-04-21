using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace proscryption
{
    public class CharacterInput : MonoBehaviour
    {
        public static CharacterInput Instance;

        private PlayerInput _playerInput;

        #region Input Actions
        private InputAction _moveAction;
        private InputAction _rollAction;
        private InputAction _interactAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _aimAction;
        private InputAction _reloadAction;
        private InputAction _pauseAction;

        private InputAction _defaultStanceAction;
        private InputAction _bloodStanceAction;
        private InputAction _lightStanceAction;
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

        /// <summary>
        /// Valor de input: boolean 
        /// </summary>
        public bool Attackinput { get; private set; }
        public bool AimInput { get; private set; }
        public bool ReloadInput { get; private set; }
        public bool PauseInput { get; private set; }

        public bool DefaultStanceInput { get; private set; }
        public bool BloodStanceInput { get; private set; }
        public bool LightStanceInput { get; private set; }
        #endregion

        #region Public Callbacks
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

        /// <summary>
        /// Callback para quando o player apertar para atacar 
        /// </summary>
        public Action<Boolean> OnAttackInput;

        /// <summary>
        /// Callback para quando o player apertar para usar a habilidade de Parry  
        /// </summary>
        public Action<Boolean> OnAimInput;
        public Action OnReloadInput;
        public Action OnPauseInput;

        public Action OnDefaultStanceInput;
        public Action OnBloodStanceInput;
        public Action OnLightStanceInput;
        #endregion

        private void Awake()
        {
            InitializeInput();
            MakeSingleton();
        }

        private void MakeSingleton()
        {
            if (CharacterInput.Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            CharacterInput.Instance = this;
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
            _attackAction = _playerInput.actions["Attack"];
            _reloadAction = _playerInput.actions["Reload"];
            _pauseAction = _playerInput.actions["Pause"];

            //Stances
            _defaultStanceAction = _playerInput.actions["ChangeStance_Default"];
            _bloodStanceAction = _playerInput.actions["ChangeStance_Blood"];
            _lightStanceAction = _playerInput.actions["ChangeStance_Light"];

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
            }

            if (_lookAction != null)
            {
                _lookAction.performed += HandleLookInput;
                _lookAction.canceled += HandleLookInput;
            }
            if (_attackAction != null)
            {
                _attackAction.performed += HandleAttackInput;
                _attackAction.canceled += HandleAttackInput;
            }
            if (_aimAction != null)
            {
                _aimAction.performed += HandleAimInput;

            }
            if (_reloadAction != null)
            {
                _reloadAction.performed += HandleReloadInput;
            }
            if (_pauseAction != null)
            {
                _pauseAction.performed += HandlePauseInput;
            }
            if (_defaultStanceAction != null)
            {
                _defaultStanceAction.performed += (context) => HandleStanceInput(context, OnDefaultStanceInput);
            }
            if (_bloodStanceAction != null)
            {
                _bloodStanceAction.performed += (context) => HandleStanceInput(context, OnBloodStanceInput);
            }
            if (_lightStanceAction != null)
            {
                _lightStanceAction.performed += (context) => HandleStanceInput(context, OnLightStanceInput);
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
            }

            if (_lookAction != null)
            {
                _lookAction.performed -= HandleLookInput;
                _lookAction.canceled -= HandleLookInput;
            }
            if (_attackAction != null)
            {
                _attackAction.performed -= HandleAttackInput;
                _attackAction.canceled -= HandleAttackInput;
            }
            if (_aimAction != null)
            {
                _aimAction.performed -= HandleAimInput;

            }
            if (_reloadAction != null)
            {
                _reloadAction.performed -= HandleReloadInput;
            }

            if (_defaultStanceAction != null)
            {
                _defaultStanceAction.performed -= (context) => HandleStanceInput(context, OnDefaultStanceInput);
            }
            if (_bloodStanceAction != null)
            {
                _bloodStanceAction.performed -= (context) => HandleStanceInput(context, OnBloodStanceInput);
            }
            if (_lightStanceAction != null)
            {
                _lightStanceAction.performed -= (context) => HandleStanceInput(context, OnLightStanceInput);
            }

        }

        #region Input Handlers
        private void HandleMoveInput(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
            OnMoveInput?.Invoke(MoveInput);
            PlayerEvents.BroadcastPlayerMoveInput(MoveInput);
        }

        private void HandleRollInput(InputAction.CallbackContext context)
        {
            RollInput = context.ReadValueAsButton();
            OnRollInput?.Invoke(RollInput);
            // NEW: Broadcast to EventManager when roll is performed
            if (RollInput)
                PlayerEvents.BroadcastPlayerRollInput();
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

        private void HandleAttackInput(InputAction.CallbackContext context)
        {
            Attackinput = context.ReadValueAsButton();
            OnAttackInput?.Invoke(Attackinput);
            // NEW: Broadcast to EventManager when attack is performed
            if (Attackinput)
                PlayerEvents.BroadcastPlayerAttackInput();
        }

        private void HandleAimInput(InputAction.CallbackContext context)
        {
            AimInput = context.ReadValueAsButton();
            OnAimInput?.Invoke(AimInput);
            // if (AimInput)
                // EventManager.BroadcastPlayerParryInput();
        }
        private void HandlePauseInput(InputAction.CallbackContext context)
        {
            PauseInput = context.ReadValueAsButton();
            if (PauseInput)
            {
                OnPauseInput?.Invoke();

                EventManager.BroadcastPauseInput();
            }
        }

        private void HandleReloadInput(InputAction.CallbackContext context)
        {
            ReloadInput = context.ReadValueAsButton();
            if (ReloadInput)
            {
                OnReloadInput?.Invoke();
                PlayerEvents.BroadcastPlayerReloadInput();
            }
        }
        private void HandleStanceInput(InputAction.CallbackContext context, Action stanceInputCallback)
        {
            bool inputValue = context.ReadValueAsButton();
            if (inputValue)
            {
                stanceInputCallback?.Invoke();
            }
        }

        #endregion

        private bool IsPaused()
        {
            if (GameManager.Instance == null) return false;
            return GameManager.Instance.CurrentState == GameState.Paused;
        }

        private void OnDestroy()
        {
            UnregisterCallbacks();
        }

         
    }
}
