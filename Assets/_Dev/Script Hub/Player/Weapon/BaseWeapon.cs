using UnityEngine;

namespace proscryption
{
    /// <summary>
    /// BaseWeapon - Detects hits and broadcasts damage events
    /// Refactored to use EventManager for decoupled damage handling
    /// </summary>
    public class BaseWeapon : MonoBehaviour
    {
        [SerializeField] private int minDamage = 5;
        [SerializeField] private int maxDamage = 10;
        [SerializeField] private int critChance = 10;

        private bool _isAttacking = false;
        private bool _canHit = false;
        private Collider _collider;
        private int _currentDamage = 0;

        void Start()
        {
            _collider = GetComponent<Collider>();

            if (minDamage > maxDamage)
            {
                int temp = minDamage;
                minDamage = maxDamage;
                maxDamage = temp;
            }
        }

        /// <summary>
        /// Activate weapon for attacking (called by CombatSystem)
        /// </summary>
        public void ActivateHitbox(int damage)
        {
            _currentDamage = damage;
            _isAttacking = true;
            _canHit = true;
        }

        /// <summary>
        /// Deprecated - use ActivateHitbox instead
        /// </summary>
        [System.Obsolete("Use ActivateHitbox(int damage) instead")]
        public void OnAttack()
        {
            _isAttacking = true;
        }

        /// <summary>
        /// Set can hit state (for animation events)
        /// </summary>
        public void SetCanHitState(bool newState)
        {
            _canHit = newState;
        }

        /// <summary>
        /// Stop attacking (for animation events)
        /// </summary>
        public void StopAttacking()
        {
            _isAttacking = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_canHit) return;
            if (!_isAttacking) return;
            if (!other.CompareTag("Enemy")) return;

            // Prevent multi-hit in single attack
            _canHit = false;

            // Broadcast hit event (not direct damage call)
            EventManager.BroadcastHitDetected(
                transform.position,
                _currentDamage > 0 ? _currentDamage : CalculateDamage(),
                other.gameObject
            );

            Debug.Log($"[BaseWeapon] Hit detected on {other.name}", gameObject);
        }

        /// <summary>
        /// Calculate damage with variance and crit (fallback if not using CombatSystem damage)
        /// </summary>
        int CalculateDamage()
        {
            int damage = Random.Range(minDamage, maxDamage);
            if (Random.Range(0, 100) <= critChance)
            {
                damage *= 2;
            }

            return damage;
        }
    }

}
