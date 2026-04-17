using System;
using UnityEngine;

namespace proscryption
{
    [Serializable]
    public enum GameState { Starting, Roaming, Combat, Dead, Paused }
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState CurrentState { get; private set; } = GameState.Starting;

        // Events
        public static Action<GameState> OnGameStateChanged;
        public static Action OnGameLoaded;


        //Ref
        GameObject _playerObject;

        [Header("Level Settings")]
        public Transform playerSpawnPoint;
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        void Start()
        {
            if (Initializer.Instance == null)
            {
                InitializeGameSession();
            }
        }

        public void Initialize(GameObject playerObject)
        {
            _playerObject = playerObject;
            InitializeGameSession();

        }
        public void InitializeGameSession()
        {
            Debug.Log("initializing...");
            SetupEvents();

            CurrentState = GameState.Starting;
            OnGameStateChanged?.Invoke(CurrentState);


            ChangeGameState(GameState.Roaming);
            OnGameLoaded?.Invoke();
        }
        private void SetupEvents()
        {
            EventManager.OnPlayerStateChanged += OnPlayerStateChanged;
            EventManager.OnHitDetected += HandleHitDetected;
            EventManager.OnGamePauseInput += HandleGamePauseToggle;
        }



        private void OnDestroy()
        {
            // Clean up event subscriptions
            EventManager.OnHitDetected -= HandleHitDetected;
            EventManager.OnPlayerStateChanged -= OnPlayerStateChanged;
            EventManager.OnGamePauseInput -= HandleGamePauseToggle;


            if (Instance == this)
            {
                Instance = null;
            }
        }



        public void ChangeGameState(GameState newState)
        {
            if (CurrentState == newState) return;
            // Debug.Log("Game State: " + CurrentState + " → " + newState);
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
        }
        private void OnPlayerStateChanged(PlayerState oldState, PlayerState newState)
        {
            switch (newState)
            {
                case PlayerState.Dead:
                    ChangeGameState(GameState.Dead);
                    break;

                default:
                    ChangeGameState(GameState.Roaming);
                    break;
            }
        }
        private void HandleHitDetected(Vector3 vector, int arg2, GameObject @object)
        {
            ChangeGameState(GameState.Combat);

        }
        public GameObject GetPlayerObject()
        {
            return _playerObject;
        }

        public Vector3 GetPlayerSpawnPointPosition(int index)
        {
            var _spawnPoint = GameObject.FindGameObjectsWithTag("PlayerSpawnPoint");
            if (index < 0 || index >= _spawnPoint.Length)
            {
                Debug.LogWarning($"Invalid spawn point index: {index}. Returning default position.");
                return new Vector3(0, 0.5f, 0); ;
            }
            this.playerSpawnPoint = _spawnPoint[index].transform;
            return _spawnPoint[index].transform.position;
        }


        private void HandleGamePauseToggle()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeGameState(GameState.Roaming);
            }
            else
                ChangeGameState(GameState.Paused);
        }

       
    }
}
