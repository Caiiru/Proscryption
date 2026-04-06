
using UnityEngine;

namespace proscryption
{
    public class ArenaManager : MonoBehaviour
    {
        private EnemyManager _enemyManager;
        [Header("Arena Settings")]
        public float spawnInterval = 8;
        private float _currentInterval = 0;
        private bool _wasStarted;

        [Header("Spawn Point")]
        public GameObject[] spawnPoint;

        void OnEnable()
        {
            EventManager.OnArenaStart += StartArena;

            GetEnemySpawnPoints();
        }

        void OnDisable()
        {
            EventManager.OnArenaStart -= StartArena;
        }

        public void StartArena()
        {
            Debug.Log("[Arena Manager] Arena started!");
            _enemyManager = EnemyManager.Instance;
            _wasStarted = true;
            _currentInterval = 0;
        }

        void Update()
        {
            if (!_wasStarted) return;
            _currentInterval += Time.deltaTime;
            if (_currentInterval < spawnInterval) ;
            else
            {
                SpawnEnemy();
                _currentInterval = 0;
            }
        }

        private void SpawnEnemy()
        {
            var enemy = _enemyManager.enemyPool.Get();
            enemy.transform.position = EnemyGetRandomSpawnPoint();


        }
        private void GetEnemySpawnPoints()
        {
            spawnPoint = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
        }
        private Vector3 EnemyGetRandomSpawnPoint()
        {
            int r = Random.Range(0, spawnPoint.Length);

            return spawnPoint[r].transform.position;
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
