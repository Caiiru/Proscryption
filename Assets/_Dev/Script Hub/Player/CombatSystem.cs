using Unity.VisualScripting;
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
        [SerializeField] private int baseAttackDamage = 10;
        [SerializeField] private int critDamage = 20;
        [SerializeField] private float critChance = 0.1f; // 10%
        [SerializeField] private float attackCooldown = 0.5f;
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
            // EventManager.OnPlayerParryInput += HandleParryInput;
        }

        void OnDisable()
        {
            EventManager.OnPlayerAttackInput -= HandleAttackInput;
            // EventManager.OnPlayerParryInput -= HandleParryInput;
        }

        // ===== ATTACK HANDLER =====

        private void HandleAttackInput()
        {
            // Validate: Can we attack?
            if (!_model.CanAttack())
            {

                Debug.Log($"[CombatSystem] Cannot attack - invalid state - {_model.CurrentState}, ${gameObject} ");
                return;
            }

            // // Validate: Is cooldown finished?
            // if (Time.time - _lastAttackTime < attackCooldown)
            // {
            //     Debug.Log("[CombatSystem] Attack on cooldown", gameObject);
            //     return;
            // }

            // Validate: Do we have stamina?
            if (!_model.TryConsumeStamina(ATTACK_STAMINA_COST))
            {
                Debug.Log("[CombatSystem] Not enough stamina", gameObject);
                return;
            }

            // Execute attack
            ExecuteAttack();
        }

        private void ExecuteAttack()
        {
            _lastAttackTime = Time.time;

            // Update model state
            _model.SetState(PlayerState.Attacking);

            // Calculate damage
            int damage = CalculateDamage();

            // Activate weapon hitbox
            // if (_currentWeapon != null)
            // {
            //     _currentWeapon.ActivateHitbox(damage);
            // }

            // Broadcast to all listeners
            EventManager.BroadcastPlayerAttack(damage);

            // Debug.Log($"[CombatSystem] Attack executed! Damage: {damage}", gameObject);
        }
        public void ActivateWeaponHitbox()
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.ActivateHitbox(CalculateDamage());
            }
        }
        public void DesactivateWeaponHitbox()
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.DesactivateHitBox();
            }
        }

        // // ===== PARRY =====

        // private void HandleParryInput()
        // {
        //     if (!_model.CanAttack())
        //     {
        //         return;
        //     }
        //     _model.SetState(PlayerState.Parrying);
        // }
        // ===== DAMAGE CALCULATION =====

        private int CalculateDamage()
        {
            // Check for crit
            bool isCrit = Random.value < critChance;

            if (isCrit)
            {
                return critDamage;
            }

            // Add some variance
            int variance = Random.Range(-2, 3);
            return Mathf.Max(1, baseAttackDamage + variance);
        }

        // ===== PUBLIC DEBUG METHODS =====

        public void PrintCombatState()
        {
            float timeSinceLastAttack = Time.time - _lastAttackTime;
            bool canAttackNow = timeSinceLastAttack >= attackCooldown;

            Debug.Log(
                $"[CombatSystem] " +
                $"Damage: {baseAttackDamage} | " +
                $"Cooldown: {attackCooldown}s | " +
                $"Time Since Attack: {timeSinceLastAttack:F2}s | " +
                $"Can Attack: {canAttackNow}",
                gameObject
            );
        }
    }
}
