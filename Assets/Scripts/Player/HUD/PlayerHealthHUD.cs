using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace proscryption
{
    public class PlayerHealthHUD : MonoBehaviour
    {
        public RectTransform healthFrame;
        public RectTransform fillBarRect;
        public RectMask2D _mask;
        public RectTransform _maskRect;


        //Mask
        public float _maxRightMask;
        private float _initialRightMask;
        public float _maskLimit = 200;
        private float _currentHealth = 0;
        private float _maxHealth = 0;

        private float _frameStartWidth = 1920;
        private float _frameStepValue = 2;
        [SerializeField] private float _initialMaskWidth;
        [SerializeField] private float lifeUpgrades = 0; //1 hp -> +4 size

        void OnEnable()
        {
            _initialMaskWidth = _maskRect.rect.width;
            _maxRightMask = fillBarRect.rect.width - _mask.padding.x - _mask.padding.z;
            _initialRightMask = _mask.padding.z;
            PlayerEvents.OnPlayerHealthChanged += HandleHealthChanged;
            PlayerEvents.OnPlayerGetReward += HandleGetNewReward;
        }

        void OnDisable()
        {
            PlayerEvents.OnPlayerGetReward -= HandleGetNewReward;
            PlayerEvents.OnPlayerHealthChanged -= HandleHealthChanged;
        }

        void HandleHealthChanged(float newHealth, float maxHealth)
        {
            this._currentHealth = newHealth;
            this._maxHealth = maxHealth;

            this.gameObject.SetActive(true);
            UpdateHealthVisual();
        }

        void UpdateHealthVisual()
        {
            float targetWidth = _currentHealth * _maxRightMask / _maxHealth;
            float newRightMask = _maxRightMask + _initialRightMask - targetWidth;
            var padding = _mask.padding;
            padding.z = newRightMask;
            _mask.padding = padding;
        }

        void UpgradeHealth()
        {
            healthFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                _frameStartWidth + (lifeUpgrades * _frameStepValue));

            _maxRightMask = fillBarRect.rect.width - _mask.padding.x - _mask.padding.z;
            _maskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                _initialMaskWidth + (lifeUpgrades * _frameStepValue / 3.15f));

            UpdateHealthVisual();
        }

        private void HandleGetNewReward(RewardData data)
        {
            bool hasHealthBeenUpgraded = false;
            foreach (var rewards in data.rewards)
            {
                if (rewards.type == SimpleRewardType.Health)
                {
                    hasHealthBeenUpgraded = true;
                    lifeUpgrades += rewards.value;
                }
            }

            if (!hasHealthBeenUpgraded) return;
            UpgradeHealth();
        }
    }
}