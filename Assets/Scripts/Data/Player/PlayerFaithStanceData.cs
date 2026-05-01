using UnityEngine;

namespace proscryption
{
    [CreateAssetMenu(fileName = "PlayerFaithStanceData", menuName = "ScriptableObjects/PlayerData/PlayerFaithStanceData")]
    public class PlayerFaithStanceData : PlayerStanceData
    {
        [Header("Faith Settings")] public float healAmount = 5;
    }
}
