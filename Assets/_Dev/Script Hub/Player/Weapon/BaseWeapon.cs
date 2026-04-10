using UnityEngine;

namespace proscryption
{
    /// <summary>
    /// BaseWeapon - Detects hits and broadcasts damage events
    /// Refactored to use EventManager for decoupled damage handling
    /// </summary>
    public class BaseWeapon : MonoBehaviour
    {
        public const int MAX_BULLETS = 6;
        [SerializeField] private int minDamage = 5;
        [SerializeField] private int maxDamage = 10;
        [SerializeField]
        [Range(0, 100)]
        [Tooltip("Chance for a critical hit (0-100%)")]
        private int critChance = 10;

        public GameObject bulletPrefab;
        public Transform _bulletSpawnPoint;

        //Bullets Mechacnics
        [SerializeField] private int _currentBullets = MAX_BULLETS;
        [SerializeField] private float reloadTime = 2f;
        private int _currentBulletIndex;
        [SerializeField] private bool _isReloading = false;
        private float _reloadTimer = 0f;
        [SerializeField] private int[] _bullets = new int[MAX_BULLETS];




        void Start()
        {
            Reload();
            if (minDamage > maxDamage)
            {
                int temp = minDamage;
                minDamage = maxDamage;
                maxDamage = temp;
            }
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
                }
            }
        }


        public void OnAttack()
        {
            GameObject bullet = Instantiate(bulletPrefab, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation);
            bullet.GetComponent<SimpleBullet>().Initialize(CalculateDamage(), CalculateIsCritical());
        }

        public void Reload()
        {
            if (_isReloading) return;
            _isReloading = true;
            _reloadTimer = reloadTime;


        }

        /// <summary>
        /// Calculate damage with variance and crit (fallback if not using CombatSystem damage)
        /// </summary>
        int CalculateDamage()
        {
            int damage = Random.Range(minDamage, maxDamage);

            return damage;
        }
        bool CalculateIsCritical()
        {
            return Random.value < (critChance / 100f);
        }
        bool ConsumeBullet()
        {
            _currentBulletIndex++;
            if (_currentBulletIndex >= MAX_BULLETS)
            {
                _currentBulletIndex = 0;
            }

            if (_currentBullets > 0)
            {
                _currentBullets--;
                return true;
            }
            else
            {
                Reload();
                return false;
            }
        }

    }
}
