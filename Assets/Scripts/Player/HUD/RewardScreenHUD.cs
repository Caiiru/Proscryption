using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace proscryption
{
    public class RewardScreenHUD : MonoBehaviour
    {
        public GameObject rewardCardPrefab;
        public Transform rewardCardParent;
        private Transform _container;
        private HorizontalLayoutGroup _layout;
        [Header("Cards")]
        private const int CARDS_COUNT = 3;
        public Transform[] _currentCards;
        [SerializeField] RewardCardHUD selectedCard;
        [SerializeField] Vector3 selectedCardStartPosition;

        public void Start()
        {
            _container = transform.GetChild(0);
            _container.gameObject.SetActive(false);
            _currentCards = new Transform[CARDS_COUNT];
            _layout = rewardCardParent.GetComponent<HorizontalLayoutGroup>();
            SetupEvents();


        }
        void Update()
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                ArenaEvents.BroadcastArenaWaveEnded();
            }
        }
        private void SetupEvents()
        {
            ArenaEvents.OnArenaWaveEnded += HandleWaveEnded;
            PlayerEvents.OnPlayerCloseRewardScreen += HandleCloseRewardScreen;

        }
        private void OnDestroy()
        {
            ArenaEvents.OnArenaWaveEnded -= HandleWaveEnded;
            PlayerEvents.OnPlayerCloseRewardScreen -= HandleCloseRewardScreen;
        }

        private UniTask ShowRewardScreen()
        {
            _container.localScale = Vector3.zero;
            _container.gameObject.SetActive(true);

            _container.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutElastic);

            UniTask.Delay(500);

            return UniTask.CompletedTask;
        }
        private async void HandleWaveEnded()
        {
            // Debug.Log("Showing Reward Screen");
            _currentCards = new Transform[CARDS_COUNT];
            await ShowRewardScreen();

            for (int i = 0; i < CARDS_COUNT; i++)
            {
                RewardData rewardData = RewardManager.Instance.GetRandomReward();
                ShowReward(rewardData, i);
            }
        }
        private void HandleCloseRewardScreen()
        {
            _container.gameObject.SetActive(false);

            _layout.enabled = true;
            for (int i = 0; i < rewardCardParent.childCount; i++)
            {
                Destroy(rewardCardParent.GetChild(i).gameObject);
            }
        }


        public void ShowReward(RewardData rewardData, int index)
        {
            var rewardCard = Instantiate(rewardCardPrefab, rewardCardParent);
            rewardCard.GetComponent<RewardCardHUD>().Setup(rewardData, this);
            _currentCards[index] = rewardCard.transform;
        }

        public async void SelectCard(RewardCardHUD rewardCardHUD)
        {
            foreach (Transform card in _currentCards)
            {
                if (card != rewardCardHUD.transform)
                    card.gameObject.SetActive(false);
            }
            selectedCard = rewardCardHUD;
            _layout.enabled = false;
            RectTransform rectT = GetComponent<RectTransform>();

            Vector3 endValue = new Vector3(rectT.rect.width / 2, rectT.rect.height / 2, 0);
            Transform cardTransform = rewardCardHUD.transform;
            selectedCardStartPosition = cardTransform.position;
            cardTransform.DOMove(endValue, 0.5f).onComplete = async () =>
            {
                await rewardCardHUD.SelectAnimation();
            };



        }
        public async void CloseSelectedCard()
        {
            Transform cardTransform = selectedCard.transform;
            cardTransform.DOMove(selectedCardStartPosition, 0.5f).onComplete = async () =>
            {
                _layout.enabled = true;
                foreach (Transform card in _currentCards)
                {
                    if (card != selectedCard.transform)
                        card.gameObject.SetActive(true);
                }
                selectedCard = null;
                selectedCardStartPosition = Vector3.zero;


            };
        }
    }
}
