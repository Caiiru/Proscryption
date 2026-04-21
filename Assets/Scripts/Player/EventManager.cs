using System;
using proscryption;
using UnityEngine;

namespace proscryption
{
    public static class EventManager
    {
        /// Broadcast when hit detection occurs (hitPosition, damage, target)
        public static event Action<Vector3, int, GameObject> OnHitDetected;


        /// Broadcast when any entity takes damage (damage, damageSource)
        public static event Action<int, GameObject> OnEntityDamaged;

        /// Broadcast when any entity dies (deadEntity)
        public static event Action<GameObject> OnEntityDied;

       

        // ===== GAME EVENTS =====
        public static event Action OnGameLoaded;
        public static event Action OnGamePauseInput;
        public static event Action OnGameWin;

        // ===== ARENA EVENTS =====




        public static void BroadcastHitDetected(Vector3 hitPos, int damage, GameObject target)
        {
            OnHitDetected?.Invoke(hitPos, damage, target);
            // Debug.Log($"[Event] Hit detected on {target.name} for {damage} damage at {hitPos}", target);
        }

        public static void BroadcastEntityDamaged(int damage, GameObject damageSource)
        {
            OnEntityDamaged?.Invoke(damage, damageSource);
            // Debug.Log($"[Event] Entity took {damage} damage from {damageSource.name}", damageSource);
        }

        public static void BroadcastEntityDied(GameObject deadEntity)
        {
            OnEntityDied?.Invoke(deadEntity);
            // Debug.Log($"[Event] {deadEntity.name} died", deadEntity);
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
 


        public static void BroadcastGameWin()
        {
            Debug.Log("Broadcast... GAME WIN ");
            OnGameWin?.Invoke();
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
    Reloading,
    Dead
}