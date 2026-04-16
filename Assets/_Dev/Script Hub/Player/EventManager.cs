using System;
using proscryption;
using UnityEngine;

namespace proscryption
{
    public static class EventManager
    {
        [Obsolete("Use PlayerEvents instead of EventManager for better organization. This class will be removed in future updates.")]
        public static event Action<Vector2> OnPlayerMoveInput;

        /// Broadcast when player attempts to attack
        /// 
        [Obsolete("Use PlayerEvents instead of EventManager for better organization. This class will be removed in future updates.")]
        public static event Action OnPlayerAttackInput;

        /// Broadcast when player attempts to roll
        [Obsolete("Use PlayerEvents instead of EventManager for better organization. This class will be removed in future updates.")]
        public static event Action OnPlayerRollInput;

        // ===== COMBAT EVENTS =====
        /// Broadcast when player attacks (int damage value)

        [Obsolete("Use PlayerEvents instead of EventManager for better organization. This class will be removed in future updates.")]
        public static event Action OnPlayerAttack;

        //Brotadcast when player try to parry
        [Obsolete("Use PlayerEvents instead of EventManager for better organization. This class will be removed in future updates.")]
        public static event Action OnPlayerParryInput;

        /// Broadcast when hit detection occurs (hitPosition, damage, target)
        public static event Action<Vector3, int, GameObject> OnHitDetected;


        /// Broadcast when any entity takes damage (damage, damageSource)
        public static event Action<int, GameObject> OnEntityDamaged;

        /// Broadcast when any entity dies (deadEntity)
        public static event Action<GameObject> OnEntityDied;

        // ===== STATE EVENTS =====
        /// Broadcast when player state changes (previousState, newState)
        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static event Action<PlayerState, PlayerState> OnPlayerStateChanged;
        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static event Action<int, int> OnPlayerHealthChanged;
        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static event Action<float, float> OnPlayerStaminaChanged;
        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static event Action<Vector2> OnMouseLookInput;

        // ===== GAME EVENTS =====
        public static event Action OnGameLoaded;
        public static event Action OnGamePauseInput;
        public static event Action OnArenaStart;
        public static event Action OnGameWin;

        // ===== ARENA EVENTS =====
        public static event Action OnWaveStart;


        // ===== BROADCAST METHODS (Controllers call these) =====

        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static void BroadcastPlayerMoveInput(Vector2 movement)
        {
            OnPlayerMoveInput?.Invoke(movement);
        }

        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static void BroadcastPlayerAttackInput()
        {
            OnPlayerAttackInput?.Invoke();
        }

        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static void BroadcastPlayerParryInput()
        {
            OnPlayerParryInput?.Invoke();
        }

        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static void BroadcastPlayerRollInput()
        {
            OnPlayerRollInput?.Invoke();
        }

        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static void BroadcastPlayerAttack()
        {
            OnPlayerAttack?.Invoke();
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

        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static void BroadcastPlayerStateChanged(PlayerState prev, PlayerState next)
        {
            OnPlayerStateChanged?.Invoke(prev, next);
            // Debug.Log($"[Event] Player state: {prev} → {next}", null);
        }

        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static void BroadcastPlayerHealthChanged(int newHealth, int maxHealth)
        {
            OnPlayerHealthChanged?.Invoke(newHealth, maxHealth);
        }
        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static void BroadcastPlayerStaminaChanged(float newStamina, float maxStamina)
        {
            OnPlayerStaminaChanged?.Invoke(newStamina, maxStamina);
        }
        public static void BroadcastPauseInput()
        {
            Debug.Log("Broadcasting pause input from EventManager.");
            OnGamePauseInput?.Invoke();
        }

        public static void BroadcastGameLoaded()
        {
            OnGameLoaded?.Invoke();
        }

        [Obsolete("Use PlayerEvents instead of EventManager ")]
        public static void BroadcastMouseLookInput(Vector2 mousePosition)
        {
            OnMouseLookInput?.Invoke(mousePosition);
        }


        public static void TriggerEvent(string eventName)
        {
            BroadcastByName(eventName);
        }
        public static void BroadcastGameWin()
        {
            Debug.Log("Broadcast... GAME WIN ");
            OnGameWin?.Invoke();
        }
        private static void BroadcastByName(string eventName)
        {
            switch (eventName)
            {
                case "OnPlayerAttackInput":
                    OnPlayerAttackInput?.Invoke();
                    break;
                case "OnPlayerRollInput":
                    OnPlayerRollInput?.Invoke();
                    break;
                case "OnPlayerParryInput":
                    OnPlayerParryInput?.Invoke();
                    break;
                case "OnGameLoaded":
                    OnGameLoaded?.Invoke();
                    break;
                case "OnGamePauseInput":
                    OnGamePauseInput?.Invoke();
                    break;
                case "ArenaStart":
                    OnArenaStart?.Invoke();
                    break;
                default:
                    Debug.LogWarning($"[EventManager] Event name '{eventName}' not recognized in BroadcastByName.");
                    break;
            }

        }
        // Arena Broadcast =========

        public static void BroadcastWaveStart()
        {
            OnWaveStart?.Invoke();
        }
        public static void BroadcastArenaStart()
        {
            OnArenaStart?.Invoke();
        }
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