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
        private float _maxRightMask;
        private float _initialRightMask;

        private float _currentHealth = 0;
        private float _maxHealth = 0;

        private float _frameStartWidth = 1920;
        private float _frameStepValue = 2;


        private float _maskPaddingEmpty;
        private float _maskPaddingFull;

        void OnEnable()
        {
            // _initialMaskWidth = _maskRect.rect.width;
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

        private void Start()
        {
            _maskPaddingFull = _mask.padding.z;
            _maskPaddingEmpty = 298f;
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

            float healthPercentage = _currentHealth / _maxHealth;
            float targetPaddingZ = Mathf.Lerp(_maskPaddingEmpty,_maskPaddingFull, healthPercentage);
            
            
            
            
            var padding = _mask.padding;
            padding.z = targetPaddingZ;
            _mask.padding = padding;
        }

        void UpgradeHealth()
        {
            // healthFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _frameStartWidth + (lifeUpgrades * _frameStepValue));
            //
            // _maxRightMask = fillBarRect.rect.width - _mask.padding.x - _mask.padding.z;
            // _maskRect.
            // SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialMaskWidth + (lifeUpgrades * _frameStepValue / 3.15f));

            UpdateHealthVisual();
        }

        private void HandleGetNewReward(RewardData data)
        {
            // foreach (var rewards in data.rewards)
            // {
            //     if (rewards.type == SimpleRewardType.Health)
            //     {
            //         lifeUpgrades += rewards.value;
            //     }
            // }
            // UpgradeHealth();
        }
    }
}