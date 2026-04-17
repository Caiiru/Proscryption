using UnityEngine;

namespace proscryption
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SphereCollider))]
    public class SimpleBullet : MonoBehaviour
    {
        public float speed = 20f;
        private int _damage = 10;
        private bool _isCritical = false;
        private Rigidbody _rigidbody;
        private SphereCollider _collider;

        public bool showDebug = false;

        private Vector3 moveDirection;

        public void Initialize(int damage, bool isCritical = false, float speed = 20f)
        {
            this._damage = damage;
            this._isCritical = isCritical;

            _rigidbody = GetComponent<Rigidbody>();
            moveDirection = transform.forward;
            moveDirection.y = 0;
            _rigidbody.linearVelocity = moveDirection * speed;

            _collider = GetComponent<SphereCollider>();

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
        void OnDrawGizmos()
        {
            if (!showDebug) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _collider.radius);
        }
    }

}
