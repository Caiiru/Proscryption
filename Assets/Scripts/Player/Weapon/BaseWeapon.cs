using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
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


        [Tooltip("Only visual for raycast shots")]
        public GameObject bulletPrefabVISUAL;

        //Bullets Mechacnics
        public const int MAX_BULLETS = 6;
        [SerializeField] private int _currentBullets = MAX_BULLETS;
        [SerializeField] private int _currentBulletIndex;
        public int bulletIndex => _currentBulletIndex;

        [Tooltip("0 = empty, 1 = standard, 2 = blood, 3=light")]
        public int[] bullets = new int[MAX_BULLETS];


        public Action<int> OnShootAction;
        public Action<int, bool> OnReloadBulletAction;

        [Header("Data")] [SerializeField] private PlayerStanceData currentStanceData;


        private CombatSystem _combatSystem;
        private PlayerModel _playerModel;

        [Header("Effects")] public VisualEffect StandardMuzzleFlashEffect;
        public VisualEffect BloodMuzzleFlashEffect;
        public VisualEffect FaithMuzzleFlashEffect;


        public void Setup(CombatSystem combatSystem)
        {
            if (minDamage > maxDamage)
            {
                int temp = minDamage;
                minDamage = maxDamage;
                maxDamage = temp;
            }

            _currentBulletIndex = 0;

            for (int i = 0; i < MAX_BULLETS; i++)
            {
                bullets[i] = 1;
            }

            int b = 0;
            foreach (int bullet in bullets)
            {
                if (bullet != 0)
                {
                    b++;
                }
            }

            _currentBullets = b;

            PlayerEvents.OnPlayerStanceChanged += HandleStanceChanged;

            _combatSystem = combatSystem;
            _playerModel = _combatSystem.GetModel();
        }

        private void OnDisable()
        {
            PlayerEvents.OnPlayerStanceChanged -= HandleStanceChanged;
        }

        private async void HandleStanceChanged(PlayerStance oldStance, PlayerStance newStance)
        {
            _currentStance = newStance;
            await UniTask.WaitForEndOfFrame();
            currentStanceData = _playerModel.GetCurrentData();
        }

        public void Update()
        {
            HandleReload();
        }


        public async void OnAttack()
        {
            if (!CanAttack()) return;

            if (Camera.main == null)
            {
                return;
            }

            Camera mainCamera = Camera.main;

            Vector3 raycastVec = Vector3.forward;


            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");


            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (currentStanceData == null)
                currentStanceData = _playerModel.GetCurrentData();

            Quaternion bulletRotation = _bulletSpawnPoint.rotation;
            bulletRotation.x = 0;

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, 1 << 6))
            {
                Vector3 targetPoint = hitInfo.point;
                targetPoint.y = transform.position.y; // Keep player rotation on horizontal plane 
                raycastVec = targetPoint - transform.position;
                raycastVec = raycastVec.normalized;


                Debug.DrawRay(transform.position, raycastVec * _combatSystem.attackRaycastLimit, Color.red, 1f);

                foreach (var enemy in enemies)
                {
                    if (Vector3.Distance(enemy.transform.position, targetPoint) < 3f)
                    {
                        if (enemy.transform.GetComponent<EnemyEntity>())
                        {
                            OnAttackRaycast(enemy.GetComponent<EnemyEntity>());
                            GameObject b = Instantiate(currentStanceData.bulletPrefab, _bulletSpawnPoint.position,
                                bulletRotation);
                            SimpleBullet bulletComponent = b.GetComponent<SimpleBullet>();
                            bulletComponent.Initialize(CalculateDamage(), CalculateIsCritical(),
                                currentStanceData.bulletSpeed,
                                currentStanceData.bulletForce, _currentStance);

                            bulletComponent.SetMoveDirection(raycastVec);
                            bulletComponent.DisableDamage();
                            return;
                        }
                    }
                }
            }


            GameObject bullet = Instantiate(currentStanceData.bulletPrefab, _bulletSpawnPoint.position, bulletRotation);
            bullet.GetComponent<SimpleBullet>().Initialize(CalculateDamage(), CalculateIsCritical(),
                currentStanceData.bulletSpeed,
                currentStanceData.bulletForce, _currentStance);
            bullet.GetComponent<SimpleBullet>().SetMoveDirection(raycastVec);
            OnShootAction?.Invoke(_currentBulletIndex);
            // if (StandardMuzzleFlashEffect != null)
            // {
            //     StandardMuzzleFlashEffect.gameObject.SetActive(true);
            //     StandardMuzzleFlashEffect.Play();
            // }

            ShowMuzzle();

            Destroy(bullet, currentStanceData.bulletDuration);

            // 0 = empty, 1 = standard, 2 = blood, 3=light
            bullets[_currentBulletIndex] = 0;

            _currentBullets--;

            //
            // if (_currentBulletIndex < 0)
            // {
            //     _currentBulletIndex = MAX_BULLETS - 1;
            // }
            LoseBullet();
            await UniTask.Delay(500);
            HiddeMuzzle();
            await UniTask.CompletedTask;
        }

        private void LoseBullet()
        {
            bool hasNextBullet = false;
            while (!hasNextBullet && _currentBullets > 0)
            {
                int nextBulletIndex = _currentBulletIndex - 1 < 0 ? MAX_BULLETS - 1 : _currentBulletIndex - 1;
                if (bullets[nextBulletIndex] != 0)
                {
                    hasNextBullet = true;
                }

                _currentBulletIndex = nextBulletIndex;
            }

            PlayerEvents.BroadcastBulletChanged(_currentBulletIndex);
        }

        public async UniTask OnAttackRaycast(EnemyEntity entity)
        {
            Debug.Log("Raycast Attack");
            Vector3 forceDir = entity.transform.position - _playerModel.gameObject.transform.position;
            forceDir.y = 0;
            int dmg = CalculateDamage();
            entity.TakeDamage(dmg, _playerModel.gameObject, CalculateIsCritical(),
                forceDir.normalized * currentStanceData.bulletForce, ForceMode.Impulse);


            OnShootAction?.Invoke(_currentBulletIndex);
            ShowMuzzle();
            bullets[_currentBulletIndex] = 0;

            _currentBullets--;

            LoseBullet();

            int child = currentStanceData.bulletPrefab.transform.childCount;
            for (int i = 0; i < child; i++)
            {
                if (currentStanceData.bulletPrefab.transform.GetChild(i).GetComponent<VisualEffect>())
                {
                    var hitVFX = Instantiate(currentStanceData.bulletPrefab.transform.GetChild(i).gameObject,
                        entity.transform);
                    hitVFX.transform.position = Vector3.zero; //Enemy child
                    hitVFX.gameObject.SetActive(true);
                    hitVFX.GetComponent<VisualEffect>().Play();

                    Destroy(hitVFX, 2f);
                }
            }

            await UniTask.Delay(500);
            HiddeMuzzle();

            EventManager.BroadcastHitDetected(entity.transform.position, dmg, entity.gameObject);
            if (_currentStance == PlayerStance.Light)
            {
                PlayerEvents.BroadcastPlayerHitLightShot();
            }

            await UniTask.CompletedTask;
        }

        public void ReloadOneBullet()
        {
            if (_currentBullets == MAX_BULLETS)
            {
                //ended
                PlayerEvents.BroadcastPlayerReloadEnded();
                return;
            }

            for (int i = 0; i < MAX_BULLETS; i++)
            {
                if (bullets[i] == 0)
                {
                    _currentBulletIndex = i;
                }
            }

            bullets[_currentBulletIndex] = 1;
            _currentBullets++;
            OnReloadBulletAction?.Invoke(_currentBulletIndex, true);
        }


        private void HandleReload()
        {
        }

        /// <summary>
        /// Calculate damage with variance and crit 
        /// </summary>
        public int CalculateDamage()
        {
            int damage = UnityEngine.Random.Range(currentStanceData.minDamage, currentStanceData.maxDamage);

            return damage;
        }

        public bool CalculateIsCritical()
        {
            return UnityEngine.Random.value < (currentStanceData.critChance / 100f);
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

            return CanConsumeBullet();
        }

        public bool CanConsumeBullet()
        {
            return _currentBullets > 0;
        }

        public bool IsFull()
        {
            return _currentBullets == MAX_BULLETS;
        }

        private void HiddeMuzzle()
        {
            StandardMuzzleFlashEffect.gameObject.SetActive(false);
            BloodMuzzleFlashEffect.gameObject.SetActive(false);
            FaithMuzzleFlashEffect.gameObject.SetActive(false);
        }

        private void ShowMuzzle()
        {
            switch (_currentStance)
            {
                case PlayerStance.Standard:
                    if (StandardMuzzleFlashEffect != null)
                    {
                        StandardMuzzleFlashEffect.gameObject.SetActive(true);
                        StandardMuzzleFlashEffect.Play();
                    }

                    break;
                case PlayerStance.Blood:
                    if (BloodMuzzleFlashEffect != null)
                    {
                        BloodMuzzleFlashEffect.gameObject.SetActive(true);
                        BloodMuzzleFlashEffect.Play();
                    }

                    break;
                case PlayerStance.Light:
                    if (FaithMuzzleFlashEffect != null)
                    {
                        FaithMuzzleFlashEffect.gameObject.SetActive(true);
                        FaithMuzzleFlashEffect.Play();
                    }

                    break;
            }
        }
    }
}