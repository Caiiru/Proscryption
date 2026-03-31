using System;
using UnityEngine;

namespace proscryption
{
    public enum GameState {Starting, Roaming, Combat, Dead }
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState CurrentState { get; private set; } = GameState.Starting;
        public static Action<GameState> OnGameStateChanged;
        public static Action OnGameLoaded;
        void Awake()
        {
            Instance = this;
            this.SetupEvents();
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
    }
}
