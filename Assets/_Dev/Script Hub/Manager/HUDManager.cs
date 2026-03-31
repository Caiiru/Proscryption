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
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Roaming:
                    _isActive = false;
                    break;
                case GameState.Dead:
                    _isActive = false;
                    TurnOnDeathScreen();
                    break;
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

            _isActive = isActive;
            foreach (GameObject hudComponent in _hudComponents)
            {
                hudComponent.SetActive(isActive);
            }
        }

        private void TurnOnDeathScreen()
        {
            if (deathScreen != null)
            {
                deathScreen.SetActive(true);
                deathScreen.GetComponent<PlayerDeathScreenHUD>().ShowDeathScreen();
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
            SetHUDActive(false);
            //event subscribe
            // EventManager.OnPlayerStateChanged += OnPlayerStateChanged;
            GameManager.OnGameStateChanged += OnGameStateChanged;

            if (GameManager.Instance != null)
            {
                OnGameStateChanged(GameManager.Instance.CurrentState);
            }
        }
    }
}
