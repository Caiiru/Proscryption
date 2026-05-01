using UnityEngine;

namespace proscryption
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData/StanceData", order = 1)]
    public class PlayerStanceData : ScriptableObject
    {
        [Header("Health & Stamina")]
        public int maxHealth = 100;
        public int maxStamina = 100;
        public float staminaRegenPerSec = 1f;
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float rollForce = 3f;
        public float rollCooldown = 2f;
        [Header("Stamina Cost")]
        public int rollStaminaCost = 20;
        public int attackStaminaCost = 10;

        [Space]
        [Header("Stance Data")]
        [Tooltip("-1 to infinite")]
        public float stanceDuration = -1;
        [Tooltip("-1 to nothing - cooldown to get stance back")]
        public float stanceCooldown = -1;
 
    }
 
  
}
