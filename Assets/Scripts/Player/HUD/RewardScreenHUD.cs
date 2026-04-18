using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace proscryption
{
    public class RewardScreenHUD : MonoBehaviour
    {
        public GameObject rewardCardPrefab;
        public Transform rewardCardParent;
        private Transform _container;

        public void Start()
        {
            _container = transform.GetChild(0);
            _container.gameObject.SetActive(false);
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
            Debug.Log("Showing Reward Screen");
            await ShowRewardScreen();
            RewardData rewardData = RewardManager.Instance.GetRandomReward();
            ShowReward(rewardData);
        }
        private void HandleCloseRewardScreen()
        {
            _container.gameObject.SetActive(false);
            
            
            for (int i = 0; i < rewardCardParent.childCount; i++)
            {
                Destroy(rewardCardParent.GetChild(i).gameObject);
            }
        }


        public void ShowReward(RewardData rewardData)
        {
            var rewardCard = Instantiate(rewardCardPrefab, rewardCardParent);
            rewardCard.GetComponent<RewardCardHUD>().Setup(rewardData);
        }
    }
}
