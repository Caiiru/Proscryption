using UnityEngine;

namespace proscryption
{
    public class RewardManager : MonoBehaviour
    {
        #region Singleton
        //Singleton
        public static RewardManager Instance { get; private set; }
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        #endregion

        public RewardDatabase rewardDatabase;

        public RewardData GetRandomReward()
        {
            int randomIndex = Random.Range(0, rewardDatabase.rewards.Length);
            return rewardDatabase.rewards[randomIndex];
        }

    }

    [System.Serializable]
    public struct RewardDatabase
    {

        public RewardData[] rewards;

    }
}
