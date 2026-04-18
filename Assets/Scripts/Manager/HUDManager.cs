using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace proscryption
{
    public class HUDManager : MonoBehaviour
    {

        [SerializeField] GameObject healthBar;
        [SerializeField] GameObject staminaBar;
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
                SetupEvents();
                Initialize();
            }

        }
        private void SetupEvents()
        {
            //event subscribe
            // EventManager.OnPlayerStateChanged += OnPlayerStateChanged;
            GameManager.OnGameStateChanged += OnGameStateChanged;
            EventManager.OnGameWin += OnGameWin;
            // _characterInput.OnLookInput += HandleLookInput;
            PlayerEvents.OnMouseLookInput += HandleLookInput;
            EventManager.OnEntityDied += HandleEntityDied;
            ArenaEvents.OnArenaWaveEnded += HandleWaveEnded;
            PlayerEvents.OnPlayerCloseRewardScreen += HandleRewardScreenBeenClosed;



            if (GameManager.Instance != null)
            {
                OnGameStateChanged(GameManager.Instance.CurrentState);
            }
        }


        void OnDisable()
        {
            EventManager.OnGameWin -= OnGameWin;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            PlayerEvents.OnMouseLookInput -= HandleLookInput;
            EventManager.OnEntityDied -= HandleEntityDied;
            ArenaEvents.OnArenaWaveEnded -= HandleWaveEnded;
            PlayerEvents.OnPlayerCloseRewardScreen -= HandleRewardScreenBeenClosed;
        }

        internal void Initialize()
        {
            #region Check GO
            if (healthBar == null)
            {
                Debug.LogWarning("Health Bar reference is missing in HUDManager.");
            }
            if (staminaBar == null)
            {
                Debug.LogWarning("Stamina Bar reference is missing in HUDManager.");
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
            #endregion

            _pauseManager = pauseScreen.GetComponent<PauseManager>();

            _playerRef = GameObject.FindWithTag("Player");
            _combatSystem = _playerRef.GetComponent<CombatSystem>();
            _characterInput = _playerRef.GetComponent<CharacterInput>();
            playerBulletCounterHUD = bulletCounter.GetComponent<PlayerBulletCounterHUD>();
            playerBulletCounterHUD.Initialize(_combatSystem.GetWeapon());
            SetupEvents();
            UpdateDeathScreenVisibility(false);
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


        private void HandleEntityDied(GameObject entity)
        {

            if (entity.CompareTag("Player"))
            {
                UpdateDeathScreenVisibility(true);
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
        }

        private void HandleWaveEnded()
        {
            healthBar.SetActive(false);
            staminaBar.SetActive(false);
            bulletCounter.SetActive(false);
            aimIndicator.SetActive(false);

        }

        private void HandleRewardScreenBeenClosed()
        {
            healthBar.SetActive(true);
            staminaBar.SetActive(true);
            bulletCounter.SetActive(true);
            aimIndicator.SetActive(true);
        }

        public RewardsListIcon GetRewardsListIcon()
        {
            if (rewardListIconData == null)
                Debug.LogError("Reward List Icon Data reference is missing in HUDManager.");



            return rewardListIconData;
        }


    }
}
