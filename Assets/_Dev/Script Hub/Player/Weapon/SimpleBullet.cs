using UnityEngine;

namespace proscryption
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SphereCollider))]
    public class SimpleBullet : MonoBehaviour
    {
        public float speed = 20f;
        private int _damage = 10;
        private bool _isCritical = false;

        public float lifetime = 0f;
        private Rigidbody _rigidbody;

        public void Initialize(int damage, bool isCritical = false)
        {
            this._damage = damage;
            this._isCritical = isCritical;

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.linearVelocity = transform.forward * speed;

            if (lifetime > 0)
                Destroy(gameObject, lifetime);
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) return;
            other.TryGetComponent<BaseEntity>(out BaseEntity entity);
            if (entity != null)
            {
                EventManager.BroadcastHitDetected(other.transform.position, _damage, other.gameObject);
                entity.TakeDamage(_damage, null, _isCritical);

            }

            Destroy(this.gameObject);
        }

    }

}
