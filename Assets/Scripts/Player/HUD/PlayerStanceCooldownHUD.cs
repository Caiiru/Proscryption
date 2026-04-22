using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class PlayerStanceCooldownHUD : MonoBehaviour
    {
        public Image fillImage;
        public PlayerStance fillStance;
        public PlayerStance _currentStance;
        void Start()
        {
            PlayerEvents.OnPlayerStanceChanged += HandleStanceChanged;
            if (fillStance == PlayerStance.Blood)
            {
                PlayerEvents.OnPlayerBloodCooldownUpdated += HandleCooldownUpdated;
            }
            if (fillStance == PlayerStance.Light)
            {
                PlayerEvents.OnPlayerLightCooldownUpdated += HandleCooldownUpdated;
            }

            fillImage = GetComponentInChildren<Image>();
        }
        void OnDestroy()
        {
            PlayerEvents.OnPlayerStanceChanged -= HandleStanceChanged;
            if (fillStance == PlayerStance.Blood)
            {
                PlayerEvents.OnPlayerBloodCooldownUpdated -= HandleCooldownUpdated;
            }
            if (fillStance == PlayerStance.Light)
            {
                PlayerEvents.OnPlayerLightCooldownUpdated -= HandleCooldownUpdated;
            }
        }

        private void HandleStanceChanged(PlayerStance oldStance, PlayerStance newStance)
        { 


            _currentStance = newStance;
        }


        private void HandleCooldownUpdated(float currentTime, float maxTime)
        {
            UpdateFillAmount((maxTime - currentTime) / maxTime, 0.1f);
        }
        private void UpdateFillAmount(float fill, float duration = 0.5f, Ease _ease = Ease.OutCirc)
        {
            fillImage.DOFillAmount(fill, duration).SetEase(_ease);

        }
    }
}
