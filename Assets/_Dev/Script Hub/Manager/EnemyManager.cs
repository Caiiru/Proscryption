using System;
using UnityEngine;
using UnityEngine.Pool;

namespace proscryption
{
    public class EnemyManager : MonoBehaviour
    {

        public ObjectPool<GameObject> enemyPool;

        public GameObject enemyPrefab;

        void Start()
        {
            Initialize();
            SetupEvents();
        }
        private void SetupEvents()
        {

            EventManager.OnGameWin += OnGameWinHandler;
        }



        void OnDisable()
        {

            EventManager.OnGameWin -= OnGameWinHandler;
        }
        public void Initialize()
        {
            enemyPool = new ObjectPool<GameObject>(
                createFunc: CreateEnemy,
                actionOnGet: OnGetEnemy,
                actionOnRelease: OnReleaseEnemy,
                defaultCapacity: 10,
                maxSize: 50
                );

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
            enemyPool.Clear();
            enemyPool.Dispose();
            var enemiesAlive = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemiesAlive)
            {
                enemy.SetActive(false);
                Destroy(enemy);
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
