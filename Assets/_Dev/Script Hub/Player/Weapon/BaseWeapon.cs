using System;
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
        [SerializeField]
        [Range(0, 100)]
        [Tooltip("Chance for a critical hit (0-100%)")]
        private int critChance = 10;

        public GameObject bulletPrefab;
        public Transform _bulletSpawnPoint;

        //Bullets Mechacnics
        public const int MAX_BULLETS = 6;
        [SerializeField] private int _currentBullets = MAX_BULLETS;
        [SerializeField] private float reloadTime = 2f;
        private int _currentBulletIndex;
        public int bulletIndex => _currentBulletIndex;
        [SerializeField] private bool _isReloading = false;
        private float _reloadTimer = 0f;
        [SerializeField] private int[] _bullets = new int[MAX_BULLETS];

        public Action OnShoot;


        void Start()
        {
            if (minDamage > maxDamage)
            {
                int temp = minDamage;
                minDamage = maxDamage;
                maxDamage = temp;
            }
            _currentBullets = MAX_BULLETS;
            _isReloading = false;
            _reloadTimer = 0f;
            _currentBulletIndex = 0;
        }
        public void Update()
        {
            if (_isReloading)
            {
                _reloadTimer -= Time.deltaTime;
                if (_reloadTimer <= 0f)
                {
                    _isReloading = false;
                    _currentBullets = MAX_BULLETS;
                    PlayerEvents.BroadcastPlayerReloadEnded();

                }
            }
        }


        public void OnAttack()
        {
            if (_isReloading) return;
            if (!ConsumeBullet()) return;

            GameObject bullet = Instantiate(bulletPrefab, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation);
            bullet.GetComponent<SimpleBullet>().Initialize(CalculateDamage(), CalculateIsCritical());
            OnShoot?.Invoke();
            _currentBullets--;
            _currentBulletIndex++;
            if (_currentBulletIndex >= MAX_BULLETS)
            {
                _currentBulletIndex = 0;
            }
        }

        public void Reload()
        {
            if (_isReloading) return;
            _isReloading = true;
            _reloadTimer = reloadTime;


        }

        /// <summary>
        /// Calculate damage with variance and crit 
        /// </summary>
        int CalculateDamage()
        {
            int damage = UnityEngine.Random.Range(minDamage, maxDamage);

            return damage;
        }
        bool CalculateIsCritical()
        {
            return UnityEngine.Random.value < (critChance / 100f);
        }
        bool ConsumeBullet()
        {
            return _currentBullets > 0;
        }

    }
}
