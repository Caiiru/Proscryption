using System;
using UnityEngine;

namespace proscryption
{
    public static class PlayerEvents
    {
        public static event Action<Vector2> OnPlayerMoveInput;


        public static event Action OnPlayerAttack;

        //Inputs

        public static event Action OnPlayerAttackInput;
        public static event Action OnPlayerRollInput;
        public static event Action OnPlayerReloadInput;
        public static event Action OnPlayerReloadEnded;
        public static event Action OnPlayerAimInput;
        public static event Action<Vector2> OnMouseLookInput;

        //Changes
        public static event Action<PlayerState, PlayerState> OnPlayerStateChanged;

        public static event Action<PlayerStance, PlayerStance> OnPlayerStanceChanged;

        public static event Action<int, int> OnPlayerHealthChanged;

        public static event Action<float, float> OnPlayerStaminaChanged;


        //============================ BROADCAST METHODS ============================
        #region Inputs Broadcasts

        /// <summary>
        /// Broadcasts the player move input event.
        /// </summary>
        /// <param name="movement">The movement vector.</param>
        public static void BroadcastPlayerMoveInput(Vector2 movement)
        {
            OnPlayerMoveInput?.Invoke(movement);
        }

        public static void BroadcastPlayerAttackInput()
        {
            OnPlayerAttackInput?.Invoke();
        }

        public static void BroadcastPlayerAimInput()
        {
            OnPlayerAimInput?.Invoke();
        }

        public static void BroadcastPlayerRollInput()
        {
            OnPlayerRollInput?.Invoke();
        }
        public static void BroadcastPlayerReloadInput()
        {
            OnPlayerReloadInput?.Invoke();
        }
        public static void BroadcastPlayerReloadEnded()
        {
            OnPlayerReloadEnded?.Invoke();
        }
        public static void BroadcastMouseLookInput(Vector2 mousePosition)
        {
            OnMouseLookInput?.Invoke(mousePosition);
        }

        #endregion

        public static void BroadcastPlayerAttack()
        {
            OnPlayerAttack?.Invoke();
        }
        #region State Changes Broadcasts
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
        public static void BroadcastPlayerStanceChanged(PlayerStance prev, PlayerStance next)
        {
            OnPlayerStanceChanged?.Invoke(prev, next); 
        }
        #endregion
    }
}