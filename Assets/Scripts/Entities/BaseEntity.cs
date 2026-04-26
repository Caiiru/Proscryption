using UnityEngine;

namespace proscryption
{
    /// <summary>
    /// BaseEntity - Base class for all damageable entities
    /// Refactored to integrate with EventManager
    /// </summary>
    public abstract class BaseEntity : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] protected int health = 10;
        [SerializeField] protected int maxHealth = 10;
        [SerializeField] protected bool canTakeDamage = true;

        protected Animator _animator;
        protected Rigidbody _rigidbody;
        protected bool _isDead = false;

        public virtual void Start()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            health = maxHealth; // Initialize health
        }

        /// <summary>
        /// Take damage. Called via EventManager for decoupled damage handling.
        /// </summary>
        public virtual void TakeDamage(
            int damage,
            GameObject source = null,
            bool isCritical = false)
        {
            TakeDamage(damage, source, isCritical, Vector3.zero, null);
        }
        public virtual void TakeDamage(
            int damage,
            GameObject source = null,
            bool isCritical = false,
            Vector3? dmgForce = null,
            ForceMode? forceMode = null
        )
        {
            if (!canTakeDamage) return;
            if (_isDead) return;


            Vector3 _dmgForce = (Vector3)(dmgForce == null ? Vector3.zero : dmgForce);
            ForceMode _forceMode = (ForceMode)(forceMode == null ? ForceMode.Impulse : forceMode);
 
            health -= isCritical ? damage * 2 : damage;

            // Broadcast damage event
            EventManager.BroadcastEntityDamaged(damage, source != null ? source : gameObject);

            _rigidbody.AddForce(_dmgForce, _forceMode);
            // Play hit animation
            if (_animator)
            {
                _animator.SetTrigger("TakeDamage");
            }

            // Check death
            if (health <= 0)
            {
                _isDead = true;
                OnDeath();
            }
        }

        /// <summary>
        /// Called when entity dies
        /// </summary>
        public virtual void OnDeath()
        {
            _isDead = true;
            EventManager.BroadcastEntityDied(gameObject);

        }

        // ===== GETTERS =====
        public int CurrentHealth => health;
        public int MaxHealth => maxHealth;
        public bool IsDead => _isDead;
    }
}
