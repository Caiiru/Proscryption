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
        protected bool _isDead = false;

        public virtual void Start()
        {
            _animator = GetComponent<Animator>();
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
            if (!canTakeDamage) return;
            if (_isDead) return;


            health -= isCritical ? damage * 2 : damage;

            // Broadcast damage event
            EventManager.BroadcastEntityDamaged(damage, source != null ? source : gameObject);

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
            EventManager.BroadcastEntityDied(gameObject);

            if (_animator)
            {
                _animator.SetTrigger("die");
            }

            // Destroy after a delay (let death animation play)
            Destroy(gameObject, 1f);
        }

        // ===== GETTERS =====
        public int CurrentHealth => health;
        public int MaxHealth => maxHealth;
        public bool IsDead => _isDead;
    }
}
