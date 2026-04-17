using UnityEngine;

namespace proscryption
{
    [CreateAssetMenu(fileName = "New Reward", menuName = "Rewards/Reward")]
    public class RewardData : ScriptableObject
    {
        public string rewardName;
        [SpritePreview]
        public Sprite rewardIcon;
        public string rewardDescription;
        public SimpleReward[] rewards;
    }
    [System.Serializable]
    public struct SimpleReward
    {
        public SimpleRewardType type;
        public float value;
    }
    public enum SimpleRewardType
    {
        Health,
        Stamina,
        MoveSpeed,
        RollForce,
        RollCooldown,
        RollStaminaCost
    }
}
