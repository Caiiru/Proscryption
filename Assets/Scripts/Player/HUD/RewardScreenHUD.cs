using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace proscryption
{
    public class RewardScreenHUD : MonoBehaviour
    {
        public GameObject standardBookPrefab;
        public GameObject bloodBookPrefab;
        public GameObject lightBookPrefab;
        public Transform rewardCardParent;
        private Transform _container;
        private HorizontalLayoutGroup _layout;
        [Header("Cards")] private const int CARDS_COUNT = 3;
        public Transform[] _currentCards;
        [SerializeField] RewardCardHUD selectedCard;
        [SerializeField] Vector3 selectedCardStartPosition;
        [SerializeField] private float selectMoveOffset = 100;
        [SerializeField] private float selectAnimationDuration = 1.5f;

        private bool _canSelect = false;

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

            _container.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);

            UniTask.Delay(500);
            _canSelect = true;
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
            GameObject bookStance = null;
            switch (rewardData.rewardStance)
            {
                case PlayerStance.Blood:
                    bookStance = bloodBookPrefab;
                    break;
                case PlayerStance.Light:
                    bookStance = lightBookPrefab;
                    break;
                case PlayerStance.Standard:
                    bookStance = standardBookPrefab;
                    break;
            }

            var rewardCard = Instantiate(bookStance, rewardCardParent);
            rewardCard.GetComponent<RewardCardHUD>().Setup(rewardData, this);
            _currentCards[index] = rewardCard.transform;
        }

        public async UniTask SelectCard(RewardCardHUD rewardCardHUD)
        {
            if (!_canSelect) return;

            if (selectedCard != null)
            {
                await selectedCard.GetComponent<RewardCardHUD>().CloseAnimation();
                // await CloseSelectedCard();
            }

            _canSelect = false;

            foreach (Transform card in _currentCards)
            {
                RewardCardHUD r = card.GetComponent<RewardCardHUD>();
                r.SetStartPosition(card.transform.position);
                if (card == rewardCardHUD.transform) continue;

                card.DOScale(0.75f, selectAnimationDuration).SetEase(Ease.OutSine);
                Vector3 moveXEndValue = r.GetStartPosition() - rewardCardHUD.transform.position;
                moveXEndValue = moveXEndValue.normalized;
                card.DOMoveX(r.GetStartPosition().x + moveXEndValue.x * selectMoveOffset,
                        selectAnimationDuration / 2)
                    .SetEase(Ease.OutSine);
            }

            selectedCard = rewardCardHUD;
            _layout.enabled = false;
            RectTransform rectT = GetComponent<RectTransform>();

            Transform cardTransform = rewardCardHUD.transform;
            selectedCardStartPosition = cardTransform.position;
            cardTransform.DOScale(1.05f, selectAnimationDuration).SetEase(Ease.OutSine);
            await rewardCardHUD.SelectAnimation();

            await UniTask.Delay(100);
            _canSelect = true;
        }

        public async UniTask CloseSelectedCard()
        {
            foreach (Transform card in _currentCards)
            {
                card.DOScale(1, selectAnimationDuration / 2).SetEase(Ease.OutSine);
                card.DOMoveX(card.GetComponent<RewardCardHUD>().GetStartPosition().x, selectAnimationDuration / 2)
                    .SetEase(Ease.OutSine);
            }

            _canSelect = true;

            await UniTask.CompletedTask;
        }
    }
}