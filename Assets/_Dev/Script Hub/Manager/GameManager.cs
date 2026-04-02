using System;
using UnityEngine;

namespace proscryption
{
    public enum GameState { Starting, Roaming, Combat, Dead }
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState CurrentState { get; private set; } = GameState.Starting;

        // Events
        public static Action<GameState> OnGameStateChanged;
        public static Action OnGameLoaded;


        //Ref
        GameObject _playerObject;

        [Header("level")]
        public Transform[] playerSpawnPoints;
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


        }
        private void OnDestroy()
        {
            // Clean up event subscriptions
            EventManager.OnHitDetected -= HandleHitDetected;
            EventManager.OnPlayerStateChanged -= OnPlayerStateChanged;

            if (Instance == this)
            {
                Instance = null;
            }
        }



        public void ChangeGameState(GameState newState)
        {
            if (CurrentState == newState) return;
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
            if (index < 0 || index >= playerSpawnPoints.Length)
            {
                // Debug.LogWarning($"Requested spawn point index {index} is out of range. Returning default spawn point.");
                return new Vector3(0, 0.5f, 0);
            }
            return playerSpawnPoints[index].position;
        }


    }
}
