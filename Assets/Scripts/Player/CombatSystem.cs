using proscryption;
using UnityEngine;

namespace proscryption
{
    /// <summary>
    /// CombatSystem - Handles all attack logic
    /// Separated from movement (PlayerController) and animations (PlayerView)
    /// Subscribes to attack input and manages damage
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        [SerializeField] private const int ATTACK_STAMINA_COST = 10;


        // ===== REFERENCES =====
        private PlayerModel _model;
        [SerializeField] private BaseWeapon _currentWeapon;

        // ===== STATE =====
        private float _lastAttackTime = 0f;

        void Awake()
        {
            _model = GetComponent<PlayerModel>();
            _currentWeapon = GetComponentInChildren<BaseWeapon>();

            if (_model == null)
                Debug.LogError("[CombatSystem] PlayerModel not found!", gameObject);

            if (_currentWeapon == null)
                Debug.LogWarning("[CombatSystem] BaseWeapon not found on children!", gameObject);



        }

        void OnEnable()
        {
            // Subscribe to attack input
            EventManager.OnPlayerAttackInput += HandleAttackInput;
            PlayerEvents.OnPlayerReloadInput += HandleReloadInput;
            // EventManager.OnPlayerParryInput += HandleParryInput;
        }

        void OnDisable()
        {
            EventManager.OnPlayerAttackInput -= HandleAttackInput;
            PlayerEvents.OnPlayerReloadInput -= HandleReloadInput;
            // EventManager.OnPlayerParryInput -= HandleParryInput;
        }

        // ===== ATTACK HANDLER =====

        private void HandleAttackInput()
        {
            // Validate: Can we attack?
            if (!_model.CanAttack())
            {

                // Debug.Log($"[CombatSystem] Cannot attack - invalid state - {_model.CurrentState}, ${gameObject} ");
                return;
            }
            // Validate: Do we have stamina?
            if (!_model.TryConsumeStamina(ATTACK_STAMINA_COST))
            {
                // Debug.Log("[CombatSystem] Not enough stamina", gameObject);
                return;
            }

            _model.SetState(PlayerState.Attacking);
            // Execute attack
            // ExecuteAttack();
        }
        /// <summary>
        /// Triggered by animation event, ensures damage is applied at the correct time in the animation
        /// </summary>
        private void ExecuteAttack()
        {
            _lastAttackTime = Time.time;
            _currentWeapon.OnAttack();

            EventManager.BroadcastPlayerAttack();

        }
        private void HandleReloadInput()
        {
            if (!CanReload()) return;

            _currentWeapon.ReloadInput();
        }
        private bool CanReload()
        {
            return _model.CanReload();
        }

        public BaseWeapon GetWeapon()
        {
            return _currentWeapon;
        }
    }
}
