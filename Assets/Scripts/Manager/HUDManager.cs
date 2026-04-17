using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace proscryption
{
    public class HUDManager : MonoBehaviour
    {
        List<GameObject> _hudScreens = new List<GameObject>();
        public Transform screensParent;
        [SerializeField] GameObject deathScreen;
        [SerializeField] GameObject winScreen;
        [SerializeField] GameObject pauseScreen;
        [SerializeField] GameObject aimIndicator;
        [SerializeField] GameObject bulletCounter;
        [SerializeField] GameObject rewardScreen;
        private PlayerBulletCounterHUD playerBulletCounterHUD;
        PauseManager _pauseManager;
        bool _isActive = true;

        //Data
        [Header("Data")]
        public RewardsListIcon rewardListIconData;

        //Refs
        GameObject _playerRef;
        CombatSystem _combatSystem;
        CharacterInput _characterInput;

        #region Singleton
        public static HUDManager Instance { get; private set; }
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
            foreach (Transform child in screensParent)
            {
                _hudScreens.Add(child.gameObject);
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
            if (rewardScreen == null)
            {
                Debug.LogWarning("Reward Screen reference is missing in HUDManager.");
            }
            if (rewardListIconData == null)
            {
                Debug.LogWarning("Reward List Icon Data reference is missing in HUDManager.");
            }

            _pauseManager = pauseScreen.GetComponent<PauseManager>();

            _playerRef = GameObject.FindWithTag("Player");
            _combatSystem = _playerRef.GetComponent<CombatSystem>();
            _characterInput = _playerRef.GetComponent<CharacterInput>();
            playerBulletCounterHUD = bulletCounter.GetComponent<PlayerBulletCounterHUD>();
            playerBulletCounterHUD.Initialize(_combatSystem.GetWeapon());
            SetupEvents();
            UpdateDeathScreenVisibility(false);
        }
        private void SetupEvents()
        {
            //event subscribe
            // EventManager.OnPlayerStateChanged += OnPlayerStateChanged;
            GameManager.OnGameStateChanged += OnGameStateChanged;
            EventManager.OnGameWin += OnGameWin;
            // _characterInput.OnLookInput += HandleLookInput;
            EventManager.OnMouseLookInput += HandleLookInput;

            if (GameManager.Instance != null)
            {
                OnGameStateChanged(GameManager.Instance.CurrentState);
            }


        }
        void OnDisable()
        {
            EventManager.OnGameWin -= OnGameWin;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            EventManager.OnMouseLookInput -= HandleLookInput;
        }


        private void HandleLookInput(Vector2 vector)
        {
            if (aimIndicator != null)
            {
                aimIndicator.transform.position = Mouse.current.position.ReadValue();
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
        }

        public RewardsListIcon GetRewardsListIcon()
        {
            if (rewardListIconData == null)
                Debug.LogError("Reward List Icon Data reference is missing in HUDManager.");



            return rewardListIconData;
        }


    }
}
