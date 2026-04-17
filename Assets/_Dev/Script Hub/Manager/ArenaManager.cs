using System;
using UnityEngine;

namespace proscryption
{
    public class ArenaManager : MonoBehaviour
    {
        private EnemyManager _enemyManager;
        [Header("Arena Settings")]
        public float spawnInterval = 8;
        private float _currentInterval = 0;
        private bool _canSpawn = false;

        [Header("Wave Settings")]
        public WaveData[] wavesData;
        private int _currentWaveIndex = -1;

        //Spawn Settings
        [Tooltip("Number of enemies to spawn in the current wave")]
        [SerializeField] int enemiesToSpawn = 2;
        public int enemiesAlive = 0;

        GameObject[] _spawnPoint;

        void OnEnable()
        {
            ArenaEvents.OnArenaStart += StartArena;
            EventManager.OnEntityDied += HandleEntityDied;
            GetEnemySpawnPoints();
        }



        void OnDisable()
        {
            ArenaEvents.OnArenaStart -= StartArena;
        }

        public void StartArena()
        {
            Debug.Log("[Arena Manager] Arena started!");
            if (_enemyManager == null)
                _enemyManager = EnemyManager.Instance;
            StartNextWave();
        }

        void Update()
        {
            HandleSpawn();
        }


        private void GetEnemySpawnPoints()
        {
            _spawnPoint = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
        }
        private Vector3 EnemyGetRandomSpawnPoint()
        {
            int r = UnityEngine.Random.Range(0, _spawnPoint.Length);

            return _spawnPoint[r].transform.position;
        }
        private void HandleEntityDied(GameObject entity)
        {
            if (entity.CompareTag("Enemy"))
            {
                enemiesAlive--;
                if (enemiesAlive <= 0)
                {
                    ArenaEvents.BroadcastArenaWaveEnded();
                }
                return;
            }
        }

        private void StartNextWave()
        {
            _currentWaveIndex++;
            if (_currentWaveIndex >= wavesData.Length)
            {
                Debug.Log("All waves completed!");
                return;
            }
            enemiesToSpawn = wavesData[_currentWaveIndex].enemyCount;
            spawnInterval = wavesData[_currentWaveIndex].spawnInterval;
            _currentInterval = spawnInterval - 1;


            ArenaEvents.BroadcastWaveStart();

            _canSpawn = true;
            enemiesAlive = enemiesToSpawn;
        }

        private void HandleSpawn()
        {
            if (!_canSpawn) return;

            _currentInterval += Time.deltaTime;
            if (_currentInterval < spawnInterval) return;

            SpawnEnemy();
            _currentInterval = 0;

        }
        private void SpawnEnemy()
        {
            if (enemiesToSpawn <= 0)
            {
                _canSpawn = false;
                return;
            }
            var enemy = _enemyManager.enemyPool.Get();
            enemy.transform.position = EnemyGetRandomSpawnPoint();
            enemiesToSpawn--;

        }


        #region Singleton

        public static ArenaManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion
    }
}
[Serializable]
public struct WaveData
{
    public int enemyCount;
    public float spawnInterval;
    public GameObject enemyPrefab;
}
