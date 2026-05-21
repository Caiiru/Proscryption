using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace proscryption
{
    public class EnemyManager : MonoBehaviour
    {
        public ObjectPool<GameObject> enemyPool;

        public GameObject enemyPrefab;

        private GameObject[] _enemies;
        private int _enemiesAlive;

        void Start()
        {
            Initialize();
            SetupEvents();
        }

        private void SetupEvents()
        {
            EventManager.OnEntityDied += OnEntityDieHandler;
            EventManager.OnGameWin += OnGameWinHandler;
        }


        void OnDisable()
        {
            EventManager.OnEntityDied -= OnEntityDieHandler;
            EventManager.OnGameWin -= OnGameWinHandler;
        }

        public void Initialize()
        {
            // enemyPool = new ObjectPool<GameObject>(
            //     createFunc: CreateEnemy,
            //     actionOnGet: OnGetEnemy,
            //     actionOnRelease: OnReleaseEnemy,
            //     defaultCapacity: 10,
            //     maxSize: 50
            //     );

            _enemies = GameObject.FindGameObjectsWithTag("Enemy");
            _enemiesAlive = _enemies.Length;
        }

        GameObject CreateEnemy()
        {
            var go = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
            go.SetActive(false);
            return go;
        }

        void OnGetEnemy(GameObject enemy)
        {
            enemy.SetActive(true);
        }

        void OnReleaseEnemy(GameObject enemy)
        {
            enemy.SetActive(false);
        }

        private void OnGameWinHandler()
        {
            // enemyPool.Clear();
            // enemyPool.Dispose();
            // var enemiesAlive = GameObject.FindGameObjectsWithTag("Enemy");
            // foreach (var enemy in enemiesAlive)
            // {
            //     enemy.SetActive(false);
            //     Destroy(enemy);
            // }

            foreach (var enemy in _enemies)
            {
                enemy.SetActive(false);
            }
        }

        private void OnEntityDieHandler(GameObject entity)
        {
            if (entity.CompareTag("Player"))
            {
                OnGameWinHandler();
            }

            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i] == entity)
                {
                    _enemies[i].SetActive(false);
                    _enemiesAlive--;
                }
            }

            if (_enemiesAlive == 0)
            {
                //Game win 
                
                OnGameWinHandler();
            }
        }

        #region Singleton

        public static EnemyManager Instance { get; private set; }

        void Awake()
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