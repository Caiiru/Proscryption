using Cysharp.Threading.Tasks;
using UnityEngine;

namespace proscryption
{
    /// <summary>
    /// BaseEntity - Base class for all damageable entities
    /// Refactored to integrate with EventManager
    /// </summary>
    public abstract class BaseEntity : MonoBehaviour
    {
        [Header("Status")] [SerializeField] protected int health = 10;
        [SerializeField] protected int maxHealth = 10;
        [SerializeField] protected bool canTakeDamage = true;
        [SerializeField] protected float takeDamageDelay = 0.25f;

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

            canTakeDamage = false;
            StartTakeDamageCooldown().Forget();

            health -= isCritical ? damage * 2 : damage;

            EventManager.BroadcastEntityDamaged(damage, source != null ? source : gameObject);


            if (health > 0) return;

            _isDead = true;
            Death(dmgForce, forceMode);
        }

        /// <summary>
        /// Called when entity dies
        /// </summary>
        public virtual void Death()
        {
            Death(Vector3.zero, null);
        }

        /// <summary>
        /// Handle Death when is pushed - uses Rigidbody
        /// </summary>
        /// <param name="force">ForceDirection + Intensity</param>
        /// <param name="mode">Rigidbody ForceMode</param>
        public virtual void Death(Vector3? force, ForceMode? mode)
        {
            _isDead = true;
            EventManager.BroadcastEntityDied(gameObject);
        }

        protected async UniTask StartTakeDamageCooldown()
        {
            await UniTask.Delay((int)(1000 * takeDamageDelay));
            canTakeDamage = true;
        }

        // ===== GETTERS =====
        public int CurrentHealth => health;
        public int MaxHealth => maxHealth;
        public bool IsDead => _isDead;
    }
}