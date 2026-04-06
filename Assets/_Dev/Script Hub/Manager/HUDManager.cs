using System;
using System.Collections.Generic;
using UnityEngine;

namespace proscryption
{
    public class HUDManager : MonoBehaviour
    {
        List<GameObject> _hudComponents = new List<GameObject>();
        [SerializeField] GameObject deathScreen;
        [SerializeField] GameObject winScreen;
        //Pause Menu
        [SerializeField] GameObject pauseScreen;
        PauseManager _pauseManager;
        bool _isActive = true;


        void OnDisable()
        {
            EventManager.OnGameWin -= OnGameWin;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }

        void Start()
        {
            if (Initializer.Instance == null)
            {
                Initialize();
                EventManager.OnEntityDied += (entity) =>
                {
                    if (entity.CompareTag("Player"))
                    {
                        UpdateDeathScreenVisibility(true);
                    }
                };

            }
        }

        internal void Initialize()
        {
            foreach (Transform child in transform)
            {
                _hudComponents.Add(child.gameObject);
            }
            if (deathScreen == null)
            {
                Debug.LogWarning("Death Screen reference is missing in HUDManager.");

            }
            if (pauseScreen == null)
            {
                Debug.LogWarning("Pause Screen reference is missing in HUDManager.");
            }
            if (winScreen == null)
            {
                Debug.LogWarning("Win Screen reference is missing in HUDManager.");
            }
            _pauseManager = pauseScreen.GetComponent<PauseManager>();
            SetupEvents();
            UpdateDeathScreenVisibility(false);
            SetHUDActive(false);
        }
        private void SetupEvents()
        {
            //event subscribe
            // EventManager.OnPlayerStateChanged += OnPlayerStateChanged;
            GameManager.OnGameStateChanged += OnGameStateChanged;
            EventManager.OnGameWin += OnGameWin;


            if (GameManager.Instance != null)
            {
                OnGameStateChanged(GameManager.Instance.CurrentState);
            }


        }

        private void OnGameWin()
        {
            if (winScreen != null)
            {
                winScreen.SetActive(true);

                winScreen.GetComponent<PlayerDeathScreenHUD>().ShowDeathScreen();

            }

            _isActive = false;
            SetHUDActive(_isActive);
        }

        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Roaming:
                    // _isActive = false;
                    break;
                case GameState.Dead:
                    // _isActive = false;
                    UpdateDeathScreenVisibility(true);
                    return;
                case GameState.Combat:
                    // _isActive = true;
                    break;
                case GameState.Paused:
                    return;

                    // Handle other states as needed
            }
            // SetHUDActive(_isActive);
        }

        private void SetHUDActive(bool isActive)
        {
            if (_isActive == isActive && _hudComponents.TrueForAll(hud => hud.activeSelf == isActive)) return;
            Debug.Log("Update HUD, isActive: " + isActive);
            _isActive = isActive;
            foreach (GameObject hudComponent in _hudComponents)
            {
                if (hudComponent == deathScreen ||
                    hudComponent == pauseScreen ||
                    hudComponent == winScreen) continue;
                hudComponent.SetActive(isActive);
            }


        }

        private void UpdateDeathScreenVisibility(bool newVisibility)
        {
            if (deathScreen != null)
            {
                deathScreen.SetActive(newVisibility);
                if (newVisibility)
                {
                    deathScreen.GetComponent<PlayerDeathScreenHUD>().ShowDeathScreen();
                }
            }

            _isActive = false;
            SetHUDActive(_isActive);
        }


    }
}
