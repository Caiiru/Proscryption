
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class RewardCardHUD : MonoBehaviour
    {

        public Image icon;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public Transform effectTransform;
        [SerializeField] private RewardData _currentRewardData;

        [Space]
        public GameObject miniRewardPrefab;

        public void Setup(RewardData rewardData)
        {
            _currentRewardData = rewardData;

            icon.sprite = _currentRewardData.rewardIcon;
            titleText.text = _currentRewardData.rewardName;
            descriptionText.text = _currentRewardData.rewardDescription;

            // Clear previous effects
            foreach (Transform child in effectTransform)
            {
                Destroy(child.gameObject);
            }

            // Create Mini Rewards - Effects
            for (int i = 0; i < _currentRewardData.rewards.Length; i++)
            {
                var miniRewardGO = Instantiate(miniRewardPrefab, effectTransform);
                
                Sprite _image = RewardsListIconUtility.GetIconForSimpleRewardType(HUDManager.Instance.GetRewardsListIcon(), _currentRewardData.rewards[i].type);

                miniRewardGO.GetComponent<EffectCardHUD>().Setup(_image, _currentRewardData.rewards[i].value.ToString());
            }


        }



    }
}
