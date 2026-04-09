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

        void Start()
        {

            if (minDamage > maxDamage)
            {
                int temp = minDamage;
                minDamage = maxDamage;
                maxDamage = temp;
            }
        }

        public void OnAttack()
        { 
            GameObject bullet = Instantiate(bulletPrefab, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation);
            bullet.GetComponent<SimpleBullet>().Initialize(CalculateDamage(), CalculateIsCritical());
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

    }
}
