using Cysharp.Threading.Tasks;
using UnityEngine;

namespace proscryption
{
    public class Initializer : MonoBehaviour
    {
        public GameObject gameManagerPrefab;
        public GameObject environmentPrefab;
        public GameObject playerPrefab;
        public GameObject enemyManagerPrefab;
        public GameObject hudManagerPrefab;
        public GameObject mainCameraPrefab;


        public static Initializer Instance { get; private set; }
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public async void Initialize()
        {
            BindObjects();
            await InitializeObjects();
            await StartGame();
        }
        void BindObjects()
        {
            gameManagerPrefab = Instantiate(gameManagerPrefab);
            environmentPrefab = Instantiate(environmentPrefab);
            // enemyManagerPrefab = Instantiate(enemyManagerPrefab);
            playerPrefab = Instantiate(playerPrefab);
            hudManagerPrefab = Instantiate(hudManagerPrefab);
            mainCameraPrefab = Instantiate(mainCameraPrefab);
        }
        async UniTask InitializeObjects()
        {

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Initialize(playerPrefab);
            }
            if (mainCameraPrefab)
            {
                mainCameraPrefab.TryGetComponent<CameraController>(out var cameraController);
                if (cameraController != null)
                {
                    cameraController.SetTarget(playerPrefab.transform);
                }
            }
            hudManagerPrefab.TryGetComponent<HUDManager>(out var hudManager);
            if (hudManager != null)
            {
                hudManager.Initialize();
            }
        }
        async UniTask StartGame()
        {
            playerPrefab.transform.position = GameManager.Instance.GetPlayerSpawnPointPosition(0);
            GameManager.Instance.ChangeGameState(GameState.Roaming);
            await UniTask.Delay(100); // Small delay to ensure all initialization is complete
            EventManager.BroadcastGameLoaded();
        }



    }
}
