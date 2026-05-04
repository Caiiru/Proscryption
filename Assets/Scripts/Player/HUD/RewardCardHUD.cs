using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class RewardCardHUD : MonoBehaviour
    {
        public Transform closedTransform;
        public Transform openTransform;

        [Header("Icons")] public float iconSize = 128;
        public Image openIcon;
        public Image closedIcon;

        [Header("Opened")] public TextMeshProUGUI titleText;
        public TextMeshProUGUI rewardDescriptionText;
        public TextMeshProUGUI loreDescriptionText;
        public Transform effectTransform;

        public Button selectButton;
        public Button openButton;
        public Button backButton;

        private Animator _animator;
        private RewardData _currentRewardData;

        [SerializeField] private Vector3 startPosition;

        [Header("State")] public bool isOpened;

        [Space] public GameObject miniRewardPrefab;

        RewardScreenHUD _rewardScreen;

        public void Setup(RewardData rewardData, RewardScreenHUD rewardScreen)
        {
            _rewardScreen = rewardScreen;
            _animator = GetComponent<Animator>();
            isOpened = false;

            _currentRewardData = rewardData;
            Sprite _icon = RewardsListIconUtility.GetIconForSimpleRewardType(HUDManager.Instance.GetRewardsListIcon(),
                _currentRewardData.rewards[0].type);
            // closedIcon.sprite = _currentRewardData.rewardIcon;
            closedIcon.sprite = _icon;
            closedIcon.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize);
            closedIcon.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize);

            openIcon.sprite = _icon;
            openIcon.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize);
            openIcon.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize);

            // openIcon.sprite = _currentRewardData.rewardIcon;
            titleText.text = _currentRewardData.rewardName;
            loreDescriptionText.text = _currentRewardData.loreDescription;


            rewardDescriptionText.text = _currentRewardData.rewardDescription;
            string desc = _currentRewardData.rewardDescription;
            int valueStart = -1;
            int valueEnd = -1;
            for (int i = 0; i < desc.Length; i++)
            {
                var c = desc[i];
                if (c.ToString() == "{")
                {
                    valueStart = i;
                }
                else if (c.ToString() == "}")
                {
                    valueEnd = i;
                }
            }

            string finalString = "";
            bool hasValue = false;
            for (int i = 0; i < desc.Length; i++)
            {
                if (i == valueStart || i == valueEnd)
                {
                    continue;
                }
                if (i > valueStart && i < valueEnd)
                {
                    if (!hasValue)
                    {
                        hasValue = true;
                        finalString += rewardData.rewards[0].value;
                    }
                }
                else
                {
                    finalString += desc[i];
                }
            }

            rewardDescriptionText.text = finalString;


            // Clear previous effects
            // foreach (Transform child in effectTransform)
            // {
            //     Destroy(child.gameObject);
            // }

            // Create Mini Rewards - Effects
            // for (int i = 0; i < _currentRewardData.rewards.Length; i++)
            // {
            //     var miniRewardGO = Instantiate(miniRewardPrefab, effectTransform);
            //
            //     Sprite _image =
            //         RewardsListIconUtility.GetIconForSimpleRewardType(HUDManager.Instance.GetRewardsListIcon(),
            //             _currentRewardData.rewards[i].type);
            //
            //     miniRewardGO.GetComponent<EffectCardHUD>()
            //         .Setup(_image, _currentRewardData.rewards[i].value.ToString());
            // }


            closedTransform.gameObject.SetActive(true);
            openTransform.gameObject.SetActive(false);

            startPosition = Vector3.zero;

            SetupEvents();
        }

        private void SetupEvents()
        {
            openButton.onClick.AddListener(OnOpenClick);
            selectButton.onClick.AddListener(OnChooseClick);
            // backButton.onClick.AddListener(OnCloseClick);
        }

        private void OnChooseClick()
        {
            PlayerEvents.BroadcastPlayerGetReward(_currentRewardData);
            PlayerEvents.BroadcastPlayerCloseRewardScreen();
        }

        private void OnOpenClick()
        {
            _rewardScreen.SelectBook(this);
        }

        public async UniTask SelectAnimation()
        {
            if (isOpened)
            {
                return;
            }

            _animator.SetTrigger("Open");
            await UniTask.Yield();
            isOpened = true;
            float animDuration = _animator.GetCurrentAnimatorClipInfo(0).Length;
            await UniTask.Delay(Mathf.FloorToInt(animDuration * 1000));
        }

        private void OnCloseClick()
        {
            CloseAnimation().Forget();
        }

        public async UniTask CloseAnimation()
        {
            _animator.SetTrigger("Back");
            isOpened = false;
            await UniTask.Delay(500);
            _rewardScreen.CloseSelectedCard();
        }

        public Vector3 GetStartPosition()
        {
            return startPosition;
        }

        public void SetStartPosition(Vector3 newStartPosition)
        {
            if (this.startPosition == Vector3.zero)
                this.startPosition = newStartPosition;
        }
    }
}