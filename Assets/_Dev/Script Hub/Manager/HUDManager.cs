using System;
using System.Collections.Generic;
using UnityEngine;

namespace proscryption
{
    public class HUDManager : MonoBehaviour
    {
        List<GameObject> _hudComponents = new List<GameObject>();
        [SerializeField] GameObject deathScreen;
        bool _isActive = true;


        void OnDisable()
        {
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
            UpdateDeathScreenVisibility(false);
            SetHUDActive(false);
            //event subscribe
            // EventManager.OnPlayerStateChanged += OnPlayerStateChanged;
            GameManager.OnGameStateChanged += OnGameStateChanged;

            if (GameManager.Instance != null)
            {
                OnGameStateChanged(GameManager.Instance.CurrentState);
            }
        }
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Roaming:
                    _isActive = false;
                    break;
                case GameState.Dead:
                    // _isActive = false;
                    UpdateDeathScreenVisibility(true);
                    return;
                case GameState.Combat:
                    _isActive = true;
                    break;
                    // Handle other states as needed
            }
            SetHUDActive(_isActive);
        }

        private void SetHUDActive(bool isActive)
        {
            if (_isActive == isActive && _hudComponents.TrueForAll(hud => hud.activeSelf == isActive)) return;
            Debug.Log("Update HUD, isActive: " + isActive);
            _isActive = isActive;
            foreach (GameObject hudComponent in _hudComponents)
            {
                if (hudComponent == deathScreen) continue; // Death screen is handled separately
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
