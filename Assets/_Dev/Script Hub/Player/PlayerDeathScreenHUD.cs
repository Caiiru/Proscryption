using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class PlayerDeathScreenHUD : MonoBehaviour
    {
        public Image backgroundImage;
        public TMPro.TextMeshProUGUI deathMessageText;
        public Button respawnButton;

        void Start()
        {
            Initialize();
        }
        private void Initialize()
        {
            if (backgroundImage == null || deathMessageText == null || respawnButton == null)
            {
                Debug.LogError("PlayerDeathScreenHUD: One or more UI components are not assigned.");
                return;
            }

            // Set up the death screen UI
            backgroundImage.color = new Color(0, 0, 0, 0.75f); // Semi-transparent black

            respawnButton.onClick.AddListener(OnRespawnButtonClicked);
        }

        public void ShowDeathScreen()
        {
            Color startColor = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);
            Color endColor = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0.75f);
            backgroundImage.color = startColor;
            deathMessageText.alpha = 0;
            respawnButton.gameObject.transform.localScale = Vector3.zero;

            //Animation
            backgroundImage.DOColor(endColor, 1f).SetEase(Ease.OutSine).Play();
            deathMessageText.DOFade(1f, 1f).SetEase(Ease.OutSine).Play().OnComplete(() =>
            {
                respawnButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).Play();
            });
        }
        private void OnRespawnButtonClicked()
        {
            Debug.Log("Restart Game");
        }
    }
}
