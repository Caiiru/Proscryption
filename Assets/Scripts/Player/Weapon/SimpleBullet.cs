using UnityEngine;

namespace proscryption
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SphereCollider))]
    public class SimpleBullet : MonoBehaviour
    {
        private float _speed = 20f;
        private int _damage = 10;
        private float m_bulletForce;
        private bool _isCritical = false;
        private Rigidbody _rigidbody;
        private SphereCollider _collider;

        private Vector3 moveDirection;

        private PlayerStance _bulletStance;

        public void Initialize(int damage,
            bool isCritical = false,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            float speed = 20f, float bulletForce = 1, PlayerStance bulletStance = PlayerStance.Standard)
        {
            this._damage = damage;
            this._isCritical = isCritical;
            this.m_bulletForce = bulletForce;
            this._speed = speed;

            _rigidbody = GetComponent<Rigidbody>();
            moveDirection = transform.forward;
            moveDirection.y = 0;
            _rigidbody.linearVelocity = moveDirection * speed;

            _collider = GetComponent<SphereCollider>();

            this._bulletStance = bulletStance;

            Debug.Log($"[SimpleBullet.Initialize - BULLET {transform.position} ]");
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[SimpleBullet.Initialize - BULLET Collided {other.gameObject.name}] ]");
            if (other.CompareTag("Player")) return;
            other.TryGetComponent<BaseEntity>(out BaseEntity entity);
            if (entity == null)
            {
                Destroy(this.gameObject);
                return;
            }

            EventManager.BroadcastHitDetected(other.transform.position, _damage, other.gameObject);
            Vector3 damageDirection = moveDirection;
            damageDirection.y = 0;
            entity.TakeDamage(_damage, null, _isCritical, damageDirection.normalized * m_bulletForce,
                ForceMode.Impulse);

            if (_bulletStance == PlayerStance.Light)
            {
                PlayerEvents.BroadcastPlayerHitLightShot();
            }

            this._collider.enabled = false;
            Destroy(this.gameObject);
        }
    }
}