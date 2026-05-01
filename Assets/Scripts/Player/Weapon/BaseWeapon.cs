using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace proscryption
{
    /// <summary>
    /// BaseWeapon - Detects hits and broadcasts damage events
    /// Refactored to use EventManager for decoupled damage handling
    /// </summary>
    public class BaseWeapon : MonoBehaviour
    {
        private PlayerStance _currentStance;
        [SerializeField] private int minDamage = 5;
        [SerializeField] private int maxDamage = 10;

        [SerializeField] [Range(0, 100)] [Tooltip("Chance for a critical hit (0-100%)")]
        private int critChance = 10;

        public GameObject bulletPrefab;
        public Transform _bulletSpawnPoint;

        //Bullets Mechacnics
        public const int MAX_BULLETS = 6;
        [SerializeField] private int _currentBullets = MAX_BULLETS;
        private int _currentBulletIndex;
        public int bulletIndex => _currentBulletIndex;

        [Tooltip("0 = empty, 1 = standard, 2 = blood, 3=light")]
        public int[] bullets = new int[MAX_BULLETS];


        //Reload 
        [SerializeField] private bool _isReloading = false;
        [SerializeField] private float _reloadTimer = 0f;

        public Action OnShoot;

        [Header("Data")] public BulletData standardBulletData;
        public BulletData bloodBulletData;
        public BulletData lightBulletData;

        private BulletData currentData;

        private CombatSystem _combatSystem;
        private PlayerModel _playerModel;

        [Header("Effects")] public VisualEffect MuzzleFlashEffect;


        public void Setup(CombatSystem combatSystem)
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

            for (int i = 0; i < MAX_BULLETS; i++)
            {
                bullets[i] = 1;
            }

            PlayerEvents.OnPlayerStanceChanged += HandleStanceChanged;
            currentData = standardBulletData;

            _combatSystem = combatSystem;
            _playerModel = _combatSystem.GetModel();
        }

        private void OnDisable()
        {
            PlayerEvents.OnPlayerStanceChanged -= HandleStanceChanged;
        }

        private void HandleStanceChanged(PlayerStance oldStance, PlayerStance newStance)
        {
            _currentStance = newStance;
            switch (newStance)
            {
                case PlayerStance.Standard:
                    currentData = standardBulletData;
                    break;
                case PlayerStance.Blood:
                    currentData = bloodBulletData;
                    break;
                case PlayerStance.Light:
                    currentData = lightBulletData;
                    break;
            }
        }

        public void Update()
        {
            HandleReload();
        }


        public async void OnAttack()
        {
            if (!CanAttack()) return;

            switch (_currentStance)
            {
                case PlayerStance.Standard:
                    break;
                case PlayerStance.Blood: 
                    break;
                case PlayerStance.Light: 
                    break;
            }

            Quaternion bulletRotation = _bulletSpawnPoint.rotation;
            bulletRotation.x = 0;
            GameObject bullet = Instantiate(currentData.bulletPrefab, _bulletSpawnPoint.position, bulletRotation);
            bullet.GetComponent<SimpleBullet>().Initialize(CalculateDamage(), CalculateIsCritical(), currentData.speed,
                currentData.bulletForce);
            OnShoot?.Invoke();
            if (MuzzleFlashEffect != null)
            {
                MuzzleFlashEffect.gameObject.SetActive(true);
                MuzzleFlashEffect.Play();
            }

            Destroy(bullet, currentData.duration);

            // 0 = empty, 1 = standard, 2 = blood, 3=light
            bullets[_currentBulletIndex] = 0;

            _currentBullets--;
            _currentBulletIndex++;
            if (_currentBulletIndex >= MAX_BULLETS)
            {
                _currentBulletIndex = 0;
            }

            await UniTask.Delay(500);
            MuzzleFlashEffect.gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

       

        public void ReloadInput()
        {
            if (_isReloading) return;
            _isReloading = true;
            _reloadTimer = _playerModel.reloadCooldown;
        }

        private void HandleReload()
        {
            if (!_isReloading) return;

            _reloadTimer -= Time.deltaTime;

            if (_reloadTimer > 0f)
                return;

            _isReloading = false;
            _currentBullets = MAX_BULLETS;
            for (int i = 0; i < MAX_BULLETS; i++)
            {
                if (bullets[i] == 0)
                {
                    bullets[i] = 1; // Reset to standard bullets on reload
                }
            }

            PlayerEvents.BroadcastPlayerReloadEnded();
        }

        /// <summary>
        /// Calculate damage with variance and crit 
        /// </summary>
        int CalculateDamage()
        {
            int damage = UnityEngine.Random.Range(currentData.minDamage, currentData.maxDamage);

            return damage;
        }

        bool CalculateIsCritical()
        {
            return UnityEngine.Random.value < (currentData.critChance / 100f);
        }

        bool CanAttack()
        {
            if (_currentStance == PlayerStance.Blood)
            {
                PlayerBloodStanceData data = (PlayerBloodStanceData)_playerModel.GetCurrentData();

                if (_playerModel.CurrentHealth - data.bloodDamage <= 0)
                    return false;
                
                _playerModel.TakeDamage(data.bloodDamage);
            }

            return ConsumeBullet() && !_isReloading;
        }

        bool ConsumeBullet()
        {
            return _currentBullets > 0;
        }
    }
}