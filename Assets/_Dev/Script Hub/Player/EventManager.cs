using System;
using UnityEngine;

/// <summary>
/// Global event bus for decoupled communication between systems.
/// All gameplay events are broadcast here.
/// Usage: Subscribe in OnEnable(), unsubscribe in OnDisable()
/// </summary>
public static class EventManager
{
    // ===== INPUT EVENTS =====
    /// Broadcast when player moves (Vector2 movement direction)
    public static event Action<Vector2> OnPlayerMoveInput;

    /// Broadcast when player attempts to attack
    public static event Action OnPlayerAttackInput;

    /// Broadcast when player attempts to roll
    public static event Action OnPlayerRollInput;

    // ===== COMBAT EVENTS =====
    /// Broadcast when player attacks (int damage value)
    public static event Action<int> OnPlayerAttack;

    //Brotadcast when player try to parry
    public static event Action OnPlayerParryInput;

    /// Broadcast when hit detection occurs (hitPosition, damage, target)
    public static event Action<Vector3, int, GameObject> OnHitDetected;
    

    /// Broadcast when any entity takes damage (damage, damageSource)
    public static event Action<int, GameObject> OnEntityDamaged;

    /// Broadcast when any entity dies (deadEntity)
    public static event Action<GameObject> OnEntityDied;

    // ===== STATE EVENTS =====
    /// Broadcast when player state changes (previousState, newState)
    public static event Action<PlayerState, PlayerState> OnPlayerStateChanged;

    /// Broadcast when player health changes (currentHealth, maxHealth)
    public static event Action<int, int> OnPlayerHealthChanged;

    /// Broadcast when player stamina changes (currentStamina, maxStamina)
    public static event Action<float, float> OnPlayerStaminaChanged;

    // ===== BROADCAST METHODS (Controllers call these) =====

    public static void BroadcastPlayerMoveInput(Vector2 movement)
    {
        OnPlayerMoveInput?.Invoke(movement);
    }

    public static void BroadcastPlayerAttackInput()
    {
        OnPlayerAttackInput?.Invoke();
    }

    public static void BroadcastPlayerParryInput()
    {
        OnPlayerParryInput?.Invoke();
    }

    public static void BroadcastPlayerRollInput()
    {
        OnPlayerRollInput?.Invoke();
    }

    public static void BroadcastPlayerAttack(int damage)
    {
        OnPlayerAttack?.Invoke(damage);
        Debug.Log($"[Event] Player attacked for {damage} damage", null);
    }
    
    public static void BroadcastHitDetected(Vector3 hitPos, int damage, GameObject target)
    {
        OnHitDetected?.Invoke(hitPos, damage, target);
        Debug.Log($"[Event] Hit detected on {target.name} for {damage} damage at {hitPos}", target);
    }

    public static void BroadcastEntityDamaged(int damage, GameObject damageSource)
    {
        OnEntityDamaged?.Invoke(damage, damageSource);
        Debug.Log($"[Event] Entity took {damage} damage from {damageSource.name}", damageSource);
    }

    public static void BroadcastEntityDied(GameObject deadEntity)
    {
        OnEntityDied?.Invoke(deadEntity);
        Debug.Log($"[Event] {deadEntity.name} died", deadEntity);
    }

    public static void BroadcastPlayerStateChanged(PlayerState prev, PlayerState next)
    {
        OnPlayerStateChanged?.Invoke(prev, next);
        // Debug.Log($"[Event] Player state: {prev} → {next}", null);
    }

    public static void BroadcastPlayerHealthChanged(int newHealth, int maxHealth)
    {
        OnPlayerHealthChanged?.Invoke(newHealth, maxHealth);
    }

    public static void BroadcastPlayerStaminaChanged(float newStamina, float maxStamina)
    {
        OnPlayerStaminaChanged?.Invoke(newStamina, maxStamina);
    }
}

/// <summary>
/// Player state machine for behavior and animation control
/// </summary>
public enum PlayerState
{
    Idle,
    Moving,
    Attacking,
    Rolling,
    Stunned,
    Parrying,
    Dead
}
