using System;
using System.Collections.Generic;
using System.Timers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace proscryption
{
    public class PlayerModel : MonoBehaviour
    {
        // ===== CONFIGURATION =====
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float maxStamina = 100;
        [SerializeField] private float staminaRegenPerSec = 10f;
        [SerializeField] private float moveSpeed = 6f;

        // ===== STATE DATA =====
        [SerializeField] private float _currentHealth;
        private float _currentStamina;
        [SerializeField] private PlayerState _currentState = PlayerState.Idle;
        [SerializeField] private PlayerStance _currentStance = PlayerStance.Standard;
        [SerializeField] private bool _isInvulnerable = false;
        private bool _canMove = true;

        //Reload
        private bool _canReload = true;
        public float currentReloadTimer = 0;
        public float reloadCooldown;

        //Roll
        [Header("Roll Settings")] [SerializeField]
        public bool isRolling = false;

        public float rollForce = 20f;

        [SerializeField] public float rollDuration = 0.5f;
        [SerializeField] public float rollCooldown = 1.5f;
        [SerializeField] public int ROLL_STAMINA_COST = 20;

        [Header("PlayerStances Data")] [SerializeField]
        private PlayerStanceData BaseStandardData;

        [SerializeField] PlayerBloodStanceData BaseBloodData;
        [SerializeField] PlayerFaithStanceData BaseLightData;

        [SerializeField] float _currentStanceTimer = 0;

        //Cooldown
        float LightCooldown;
        float BloodCooldown;
        [SerializeField] float _lightCooldownTimer = 0;
        [SerializeField] float _bloodCooldownTimer = 0;

        //Upgardes
        private int _healthUpgrade = 0;


        private PlayerStanceData _currentData;

        // My Rewards
        [SerializeField] private List<RewardData> collectedRewards;

        // ===== PUBLIC GETTERS (Read-only access to state) =====

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public float CurrentStamina => _currentStamina;
        public float MaxStamina => maxStamina;
        public PlayerState CurrentState => _currentState;
        public bool IsInvulnerable => _isInvulnerable;
        public bool IsAlive => _currentHealth > 0;
        public float MoveSpeed => moveSpeed;
        public bool CanMove => _canMove;

        // ===== REFERENCES =====
        private Rigidbody _rigidbody;
        private PlayerView _playerView;

        private CombatSystem _combatSystem;

        // ===== Debug =====
        [Space] [Header("DEBUG")] public bool killPlayer = false;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            // Clone Copy 
            BaseStandardData = Instantiate(BaseStandardData);
            BaseBloodData = Instantiate(BaseBloodData);
            BaseLightData = Instantiate(BaseLightData);

            // Initialize state
            _currentHealth = maxHealth;
            _currentStamina = maxStamina;

            _playerView = GetComponent<PlayerView>();
            _combatSystem = GetComponent<CombatSystem>();
        }

        void OnEnable()
        {
            // Listen to combat events that affect model
            EventManager.OnHitDetected += HandleHitDetected;
            PlayerEvents.OnPlayerGetReward += HandleGetNewReward;
            PlayerEvents.OnPlayerReloadEnded += HandleReloadEnded;
            PlayerEvents.OnPlayerHitLightShot += HandleLightShot;
        }


        void OnDisable()
        {
            PlayerEvents.OnPlayerReloadEnded -= HandleReloadEnded;
            EventManager.OnHitDetected -= HandleHitDetected;
            PlayerEvents.OnPlayerGetReward -= HandleGetNewReward;
            PlayerEvents.OnPlayerHitLightShot -= HandleLightShot;
        }


        void Start()
        {
            _currentData = BaseStandardData; // Start with standard stance data 
            SetupTimers();
            SetupCurrentStance();
        }

        void Update()
        {
            if (killPlayer)
            {
                killPlayer = false;
                HandleHitDetected(transform.position, maxHealth, gameObject);
            }

            UpdateCurrentState();
            HandleTimers();
        }

        void SetupTimers()
        {
            LightCooldown = BaseLightData.stanceCooldown;
            BloodCooldown = BaseBloodData.stanceCooldown;
        }

        void HandleTimers()
        {
            HandleCurrentStanceTimer();
            if (_bloodCooldownTimer > 0)
            {
                _bloodCooldownTimer -= Time.deltaTime;
                PlayerEvents.BroadcastPlayerBloodCooldownUpdated(_bloodCooldownTimer, BloodCooldown);
            }

            if (_lightCooldownTimer > 0)
            {
                _lightCooldownTimer -= Time.deltaTime;
                PlayerEvents.BroadcastPlayerLightCooldownUpdated(_lightCooldownTimer, LightCooldown);
            }
        }

        void UpdateCurrentState()
        {
            switch (_currentState)
            {
                case PlayerState.Reloading:
                    currentReloadTimer -= Time.deltaTime;
                    if (_combatSystem.GetWeapon().IsFull())
                    {
                        ChangeState(PlayerState.Idle);
                        _playerView.StopReloading();
                        return;
                    }

                    if (currentReloadTimer <= 0)
                    {
                        _playerView.InsertBulletVisual();
                        currentReloadTimer = reloadCooldown;
                    }

                    break;
            }
        }


        private void HandleCurrentStanceTimer()
        {
            if (_currentStanceTimer > 0)
            {
                PlayerEvents.BroadcastCurrentStanceDurationUpdated(_currentStanceTimer, _currentData.stanceDuration);
                _currentStanceTimer -= Time.deltaTime;
                return;
            }

            if (_currentStance != PlayerStance.Standard)
            {
                ChangeStance(PlayerStance.Standard);
            }
        }
        // ===== Stance Management ===== 

        public void ChangeStance(PlayerStance newStance)
        {
            //Only swap if cooldown has ended
            switch (newStance)
            {
                case PlayerStance.Blood:
                    if (_bloodCooldownTimer > 0) return;
                    break;
                case PlayerStance.Light:
                    if (_lightCooldownTimer > 0) return;
                    break;
            }

            PlayerStance _prevStance = _currentStance;

            if (_currentStance == newStance)
            {
                if (_currentStance == PlayerStance.Standard) return;


                _currentStance = PlayerStance.Standard;
            }

            else
            {
                this._currentStance = newStance;
            }

            PlayerEvents.BroadcastPlayerStanceChanged(_prevStance, _currentStance);
            HandleStanceChanged(_prevStance, _currentStance);
        }

        private void SetupCurrentStance()
        {
            float healthPercentage = CurrentHealth / MaxHealth;
            float staminaPercentage = _currentStamina / maxStamina;


            PlayerStanceData data = (PlayerStanceData)_currentData;
            maxHealth = data.maxHealth;
            moveSpeed = data.moveSpeed;
            maxStamina = data.maxStamina;
            rollForce = data.rollForce;
            ROLL_STAMINA_COST = data.rollStaminaCost;
            rollCooldown = data.rollCooldown;

            if (_currentStance != PlayerStance.Standard)
                _currentStanceTimer = data.stanceDuration;


            // staminaRegenPerSec = _currentData.staminaRegenPerSec;
            SetupHealth(healthPercentage);
            SetupStamina(staminaPercentage);
        }

        private void HandleStanceChanged(PlayerStance oldStance, PlayerStance newStance)
        {
            switch (oldStance)
            {
                case PlayerStance.Light:
                    _lightCooldownTimer = LightCooldown;

                    break;
                case PlayerStance.Blood:
                    _bloodCooldownTimer = BloodCooldown;
                    break;
                default:
                    break;
            }

            switch (newStance)
            {
                case PlayerStance.Light:
                    _currentData = BaseLightData;

                    break;
                case PlayerStance.Blood:
                    _currentData = (PlayerStanceData)BaseBloodData;
                    break;
                default:
                    _currentData = BaseStandardData;

                    break;
            }


            SetupCurrentStance();
        }


        // ===== STAMINA MANAGEMENT =====
        private void SetupStamina(float staminaPercentage)
        {
            _currentStamina = maxStamina * staminaPercentage;
            PlayerEvents.BroadcastPlayerStaminaChanged(_currentStamina, maxStamina);
        }

        /// <summary>
        /// Try to consume stamina for an action. Returns true if successful.
        /// </summary>
        public bool TryConsumeStamina(int amount)
        {
            if (_currentStamina >= amount)
            {
                _currentStamina -= amount;
                PlayerEvents.BroadcastPlayerStaminaChanged(_currentStamina, maxStamina);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Regenerate stamina (called every frame by update system)
        /// </summary>
        public void RegenerateStamina(float deltaTime)
        {
            if (_currentState == PlayerState.Dead) return;
            if (_currentStamina == maxStamina) return;

            float regenAmount = (float)(staminaRegenPerSec * deltaTime);

            _currentStamina = Mathf.Min(_currentStamina + regenAmount, maxStamina);


            // Only broadcast if actually changed
            if (regenAmount > 0)
                PlayerEvents.BroadcastPlayerStaminaChanged(_currentStamina, maxStamina);
        }

        // ===== STATE MANAGEMENT =====

        /// <summary>
        /// Change player state. Broadcasts event if state actually changes.
        /// </summary>
        public void ChangeState(PlayerState newState)
        {
            if (_currentState == newState) return;

            PlayerState prev = _currentState;
            _currentState = newState;

            PlayerEvents.BroadcastPlayerStateChanged(prev, newState);

            if (newState == PlayerState.Reloading)
            {
                currentReloadTimer = reloadCooldown;
            }

            if (newState == PlayerState.Menu)
            {
                AppManager.Instance.SetCursorVisibility(true);
            }
            else
            {
                AppManager.Instance.SetCursorVisibility(false);
            }
        }


        private void HandleReloadEnded()
        {
            this.ChangeState(PlayerState.Idle);
        }

        // ===== REWARD MANAGMENT =====
        public void HandleGetNewReward(RewardData reward)
        {
            collectedRewards.Add(reward);

            foreach (var minireward in reward.rewards)
            {
                AddSimpleReward(minireward.type, minireward.value);
            }
        }

        private void AddSimpleReward(SimpleRewardType type, float _value)
        {
            switch (type)
            {
                case SimpleRewardType.Health:
                    maxHealth += (int)_value;
                    PlayerEvents.BroadcastPlayerHealthChanged(_currentHealth, maxHealth);
                    Heal((int)_value);
                    break;
                case SimpleRewardType.Stamina:
                    maxStamina += (int)_value;
                    break;
                case SimpleRewardType.MoveSpeed:
                    moveSpeed += _value;
                    break;
                case SimpleRewardType.ReloadTime:
                    reloadCooldown += _value;
                    break;
                case SimpleRewardType.StandardDamage:
                    this.BaseStandardData.minDamage += (int)_value;
                    this.BaseStandardData.maxDamage += (int)_value;
                    break;
                case SimpleRewardType.BloodDamage:
                    this.BaseBloodData.minDamage += (int)_value;
                    this.BaseBloodData.maxDamage += (int)_value;
                    break;
                case SimpleRewardType.LightDamage:
                    this.BaseLightData.minDamage += (int)_value;
                    this.BaseLightData.maxDamage += (int)_value;
                    break;
                case SimpleRewardType.BloodDuration:
                    this.BaseBloodData.stanceDuration += (int)_value;
                    break;
                case SimpleRewardType.LightDuration:
                    this.BaseLightData.stanceDuration += (int)_value;
                    break;
                case SimpleRewardType.Cure:
                    this.BaseLightData.healAmount += (int)_value;
                    break;
            }
        }


        // ===== DAMAGE/HEALTH MANAGEMENT =====

        /// <summary>
        /// Handle incoming damage. Called via EventManager when hit is detected.
        /// </summary>
        void SetupHealth(float percentage)
        {
            _currentHealth = (MaxHealth * percentage);
            PlayerEvents.BroadcastPlayerHealthChanged(_currentHealth, maxHealth);
        }

        private void HandleHitDetected(Vector3 hitPos, float damage, GameObject target)
        {
            if (target != gameObject) return;
            if (!IsAlive) return;
            if (_isInvulnerable)
            {
                return;
            }

            TakeDamage(damage);
        }

        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            PlayerEvents.BroadcastPlayerHealthChanged(_currentHealth, maxHealth);

            if (_currentHealth <= 0)
            {
                ChangeState(PlayerState.Dead);
                EventManager.BroadcastEntityDied(gameObject);
            }
        }

        private void HandleLightShot()
        {
            Heal(BaseLightData.healAmount);
        }

        /// <summary>
        /// Heal the player
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive) return;

            _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
            PlayerEvents.BroadcastPlayerHealthChanged(_currentHealth, maxHealth);
        }

        // ===== INVULNERABILITY =====

        /// <summary>
        /// Set invulnerability state (e.g., during roll or after being hit)
        /// </summary>
        public void SetInvulnerable(bool value, float duration = 0f)
        {
            _isInvulnerable = value;
            if (value && duration > 0)
            {
                CancelInvoke(nameof(EndInvulnerability));
                Invoke(nameof(EndInvulnerability), duration);
            }
        }

        private void EndInvulnerability()
        {
            _isInvulnerable = false;
        }

        public void SetCantMove()
        {
            this._canMove = false;
        }

        public void SetCanMove()
        {
            this._canMove = true;
        }

        public async UniTask SetEndRoll()
        {
            //Animation call 

            await UniTask.WaitForSeconds(0.1f);

            this.isRolling = false;
            ChangeState(PlayerState.Idle);
        }

        // ===== QUERY METHODS (Controllers ask "can I do this?") =====

        public bool GetCanMove()
        {
            return _currentState == PlayerState.Idle ||
                   _currentState == PlayerState.Moving;
        }

        public bool CanAttack()
        {
            if (!IsAlive) return false;
            if (_currentState != PlayerState.Idle || _currentState == PlayerState.Moving)
                return false;
            if (isRolling) return false;

            return true;
        }

        public bool CanRoll()
        {
            return (_currentState == PlayerState.Idle ||
                    _currentState == PlayerState.Moving) &&
                   _currentStamina >= ROLL_STAMINA_COST &&
                   IsAlive;
        }

        public bool CanRotate()
        {
            return _currentState != PlayerState.Rolling &&
                   _currentState != PlayerState.Attacking &&
                   _currentState != PlayerState.Menu &&
                   IsAlive;
        }

        public bool CanReload()
        {
            return (_currentState == PlayerState.Idle ||
                    _currentState == PlayerState.Moving) &&
                   _canReload &&
                   IsAlive;
        }

        public PlayerStanceData GetCurrentData()
        {
            return _currentData;
        }
    }

    public enum PlayerStance
    {
        Standard,
        Blood,
        Light
    }
}