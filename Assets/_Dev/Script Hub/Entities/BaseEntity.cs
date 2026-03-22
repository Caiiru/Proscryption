using UnityEngine;

namespace proscryption
{
    public abstract class BaseEntity : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] private int health = 10;
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private bool canTakeDamage = true;

        private Animator _animator;

        public virtual void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public virtual void TakeDamage(int damage)
        {
            if (!canTakeDamage) return;
            health -= damage;
            if (health <= 0)
            {
                OnDeath();
            }

            if (_animator)
            {
                // _animator.SetBool("Hit", true); 
                _animator.SetTrigger("TakeDamage");
            }
        }



        public virtual void OnDeath()
        {
            Destroy(gameObject);
        }
    }
}
