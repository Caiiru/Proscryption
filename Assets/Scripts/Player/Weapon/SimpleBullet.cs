using UnityEngine;

namespace proscryption
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SphereCollider))]
    public class SimpleBullet : MonoBehaviour
    {
        public float speed = 20f;
        private int _damage = 10;
        private float _bulletForce;
        private bool _isCritical = false;
        private Rigidbody _rigidbody;
        private SphereCollider _collider;

        public bool showDebug = false;

        private Vector3 moveDirection;
        public void Initialize(int damage,
                                     bool isCritical = false,
                                     float speed = 20f, float bulletForce = 0)
        {

            this._damage = damage;
            this._isCritical = isCritical;
            this._bulletForce = bulletForce;

            _rigidbody = GetComponent<Rigidbody>();
            moveDirection = transform.forward;
            moveDirection.y = 0;
            _rigidbody.linearVelocity = moveDirection * speed;

            _collider = GetComponent<SphereCollider>();
        }
        public void Initialize(int damage, bool isCritical = false, float speed = 20f)
        {
            Initialize(damage, isCritical, speed, 0);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) return;
            other.TryGetComponent<BaseEntity>(out BaseEntity entity);
            if (entity != null)
            {
                EventManager.BroadcastHitDetected(other.transform.position, _damage, other.gameObject);
                Vector3 damageForce = transform.position - other.transform.position;
                entity.TakeDamage(_damage, null, _isCritical, damageForce.normalized * _bulletForce, ForceMode.Impulse);


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
