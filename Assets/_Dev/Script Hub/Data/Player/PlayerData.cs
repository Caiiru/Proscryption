using UnityEngine;

namespace proscryption
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
    public class PlayerData : ScriptableObject
    {
        public int maxHealth = 100;
        public int maxStamina = 100;
        public float moveSpeed = 5f;
        public float rollForce = 3f;
        public float rollCooldown = 2f;
        public int rollStaminaCost = 20;
    }
}
