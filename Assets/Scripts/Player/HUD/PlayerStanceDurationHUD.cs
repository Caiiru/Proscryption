using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

namespace proscryption
{
    public class PlayerStanceDurationHUD : MonoBehaviour
    {
        public Image fillImage;
        void OnEnable()
        {
            PlayerEvents.OnPlayerStanceChanged += HandleStanceChanged;
            PlayerEvents.OnCurrentStanceDurationUpdated += HandleStanceDuration;
        }
        void OnDisable()
        {
            PlayerEvents.OnCurrentStanceDurationUpdated -= HandleStanceDuration;
            PlayerEvents.OnPlayerStanceChanged -= HandleStanceChanged;
        }
        void Start()
        {
            fillImage.fillAmount = 0;

        }
        private void HandleStanceDuration(float currentTime, float maxTime)
        {
            UpdateFillAmount(currentTime / maxTime, 0.1f);
            // fillImage.fillAmount = currentTime / maxTime;
        }
        private void UpdateFillAmount(float fill, float duration = 0.5f, Ease _ease = Ease.Linear)
        {
            fillImage.DOFillAmount(fill, duration).SetEase(_ease);
        }

        private void HandleStanceChanged(PlayerStance oldStance, PlayerStance newStance)
        {
            if (newStance == PlayerStance.Standard)
            {
                // do somethinfg
                fillImage.color = ColorManager.Instance.colorDatabase.UI_StandardColor;
                fillImage.fillAmount = 1;
                return;
            }
            if (newStance == PlayerStance.Blood)
                fillImage.color = ColorManager.Instance.colorDatabase.UI_BloodColor;
            if (newStance == PlayerStance.Light)
                fillImage.color = ColorManager.Instance.colorDatabase.UI_LightColor;
            fillImage.transform.gameObject.SetActive(true);
            UpdateFillAmount(1, 0.1f,Ease.OutCirc);
        }
    }
}
