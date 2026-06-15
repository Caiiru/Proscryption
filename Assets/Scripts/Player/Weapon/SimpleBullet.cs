using System;
using UnityEngine;
using UnityEngine.VFX;

namespace proscryption
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SphereCollider))]
    public class SimpleBullet : MonoBehaviour
    {
        public float _speed = 20f;
        private int _damage = 10;
        private float m_bulletForce;
        private bool _isCritical = false;
        private SphereCollider _collider;

        private Vector3 moveDirection;

        private PlayerStance _bulletStance;

        public bool isVisual;

        [Header("VFX")] public GameObject HitVFX;

        public void Initialize(int damage,
            bool isCritical = false,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            float speed = 20f, float bulletForce = 1, PlayerStance bulletStance = PlayerStance.Standard)
        {
            this._damage = damage;
            this._isCritical = isCritical;
            this.m_bulletForce = bulletForce;
            this._speed = speed;

            moveDirection = transform.forward;
            moveDirection.y = 0;
            this._speed = speed;


            _collider = GetComponent<SphereCollider>();

            this._bulletStance = bulletStance;

            // Debug.Log($"[SimpleBullet.Initialize - BULLET {transform.position} ]");
        }

        public void SetMoveDirection(Vector3 moveDirection)
        {
            this.moveDirection = moveDirection;
        }

        public void DisableDamage()
        {
            isVisual = true;
        }

        private void Update()
        {
            if (moveDirection == Vector3.zero) return;
            transform.position += moveDirection.normalized * _speed * Time.deltaTime;
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[ BULLET Collided {other.gameObject.name}] ]");
            if (other.CompareTag("Player")) return;


            other.TryGetComponent<BaseEntity>(out BaseEntity entity);
            if (entity == null)
            {
                if (HitVFX)
                {
                    HitVFX.SetActive(true);
                    HitVFX.TryGetComponent(out VisualEffect vfx);

                    if (vfx)
                        vfx.Play();
                }

                // DisableBullet();
                return;
            }

            if (isVisual)
            {
                // gameObject.SetActive(false);
                // DisableBullet();
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

            // this._collider.enabled = false;
            DisableBullet();
        }

        void DisableBullet()
        {
            moveDirection = Vector3.zero;
            this._speed = 0;
            this._collider.radius = 0;
            this._collider.enabled = false;
            Destroy(gameObject, 5f);
        }
    }
}