using UnityEngine;

namespace proscryption
{
    [CreateAssetMenu(fileName = "BloodData", menuName = "ScriptableObjects/PlayerData/BloodStanceData", order = 1)]
    public class PlayerBloodStanceData : PlayerStanceData
    {
        [Space]
        [Header("Blood Settings")]
        public int bloodDamage = 5; 
    }
}
