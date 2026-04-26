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

        private int _currentHealth = 0;
        private int _maxHealth = 0;

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

        void HandleHealthChanged(int newHealth, int maxHealth)
        {
            this._currentHealth = newHealth;
            this._maxHealth = maxHealth;

            this.gameObject.SetActive(true);
            UpdateHealthVisual();
        }
        void UpdateHealthVisual()
        {
            healthFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _frameStartWidth + (lifeUpgrades * _frameStepValue));

            _maxRightMask = fillBarRect.rect.width - _mask.padding.x - _mask.padding.z;
            _maskRect.
            SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialMaskWidth + (lifeUpgrades * _frameStepValue / 3.15f));

            float targetWidth = _currentHealth * _maxRightMask / _maxHealth;
            float newRightMask = _maxRightMask + _initialRightMask - targetWidth;
            var padding = _mask.padding;
            padding.z = newRightMask;
            _mask.padding = padding;

        }

        private void HandleGetNewReward(RewardData data)
        {
            foreach (var rewards in data.rewards)
            {
                if (rewards.type == SimpleRewardType.Health)
                {
                    lifeUpgrades += rewards.value;
                }
            }
            UpdateHealthVisual();

        }


    }
}
