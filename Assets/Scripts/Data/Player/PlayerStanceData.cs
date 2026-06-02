using UnityEngine;

namespace proscryption
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData/StanceData", order = 1)]
    public class PlayerStanceData : ScriptableObject
    {
        [Header("Health & Stamina")] public int maxHealth = 100;
        public int maxStamina = 100;
        public float staminaRegenPerSec = 1f;
        [Header("Movement")] public float moveSpeed = 5f;
        public float runSpeed = 5f;
        public int attackStaminaCost = 10;

        [Space] [Header("Stance Data")] [Tooltip("-1 to infinite")]
        public float stanceDuration = -1;

        [Tooltip("-1 to nothing - cooldown to get stance back")]
        public float stanceCooldown = -1;

        [Space] [Header("Bullet")] public int minDamage;
        public int maxDamage;
        [Range(0, 100)] public int critChance;

        public float bulletSpeed;
        public float bulletDuration;
        public float bulletForce = 0;

        public GameObject bulletPrefab;

        [Header("EFFECTS")] public GameObject impactEffect;
    }
}