using UnityEngine;

/// <summary>
/// PlayerModel - MVC Model Layer
/// Centralizes all player state data (health, stamina, position, state)
/// Controllers ask "CanI do this?" before acting
/// Model broadcasts state changes via EventManager
/// </summary>
public class PlayerModel : MonoBehaviour
{
    // ===== CONFIGURATION =====
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxStamina = 100;
    [SerializeField] private float staminaRegenPerSec = 10f;
    [SerializeField] private float moveSpeed = 6f;
    
    // ===== STATE DATA =====
    private int _currentHealth;
    private int _currentStamina;
    private PlayerState _currentState = PlayerState.Idle;
    private bool _isInvulnerable = false;
    
    // ===== REFERENCES =====
    private Rigidbody _rigidbody;
    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        // Initialize state
        _currentHealth = maxHealth;
        _currentStamina = maxStamina;
    }
    
    void OnEnable()
    {
        // Listen to combat events that affect model
        EventManager.OnHitDetected += HandleHitDetected;
    }
    
    void OnDisable()
    {
        EventManager.OnHitDetected -= HandleHitDetected;
    }
    
    // ===== PUBLIC GETTERS (Read-only access to state) =====
    
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentStamina => _currentStamina;
    public int MaxStamina => maxStamina;
    public PlayerState CurrentState => _currentState;
    public bool IsInvulnerable => _isInvulnerable;
    public bool IsAlive => _currentHealth > 0;
    public float MoveSpeed => moveSpeed;
    
    // ===== STAMINA MANAGEMENT =====
    
    /// <summary>
    /// Try to consume stamina for an action. Returns true if successful.
    /// </summary>
    public bool TryConsumeStamina(int amount)
    {
        if (_currentStamina >= amount)
        {
            _currentStamina -= amount;
            EventManager.BroadcastPlayerStaminaChanged(_currentStamina, maxStamina);
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
        
        int regenAmount = (int)(staminaRegenPerSec * deltaTime);
        _currentStamina = Mathf.Min(_currentStamina + regenAmount, maxStamina);
        
        // Only broadcast if actually changed
        if (regenAmount > 0)
            EventManager.BroadcastPlayerStaminaChanged(_currentStamina, maxStamina);
    }
    
    // ===== STATE MANAGEMENT =====
    
    /// <summary>
    /// Change player state. Broadcasts event if state actually changes.
    /// </summary>
    public void SetState(PlayerState newState)
    {
        if (_currentState == newState) return;
        
        PlayerState prev = _currentState;
        _currentState = newState;
        
        EventManager.BroadcastPlayerStateChanged(prev, newState);
    }
    
    // ===== DAMAGE/HEALTH MANAGEMENT =====
    
    /// <summary>
    /// Handle incoming damage. Called via EventManager when hit is detected.
    /// </summary>
    private void HandleHitDetected(Vector3 hitPos, int damage, GameObject target)
    {
        if (target != gameObject) return;
        if (!IsAlive) return;
        if (_isInvulnerable) return;
        
        _currentHealth -= damage;
        EventManager.BroadcastPlayerHealthChanged(_currentHealth, maxHealth);
        
        if (_currentHealth <= 0)
        {
            SetState(PlayerState.Dead);
            EventManager.BroadcastEntityDied(gameObject);
        }
    }
    
    /// <summary>
    /// Heal the player
    /// </summary>
    public void Heal(int amount)
    {
        if (!IsAlive) return;
        
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        EventManager.BroadcastPlayerHealthChanged(_currentHealth, maxHealth);
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
    
    // ===== QUERY METHODS (Controllers ask "can I do this?") =====
    
    public bool CanMove()
    {
        return _currentState == PlayerState.Idle || 
               _currentState == PlayerState.Moving;
    }
    
    public bool CanAttack()
    {
        return (_currentState == PlayerState.Idle || 
                _currentState == PlayerState.Moving) &&
               IsAlive;
    }
    
    public bool CanRoll()
    {
        return (_currentState == PlayerState.Idle || 
                _currentState == PlayerState.Moving) && 
               _currentStamina >= 0 &&
               IsAlive;
    }
}
