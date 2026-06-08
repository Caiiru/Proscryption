using System;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class PlayerStaminaHUD : MonoBehaviour
    {
        public RectTransform staminaFrame;
        public RectTransform fillBarRect;
        public RectTransform maskFormatRect;
        public RectMask2D mask;
        public RectTransform maskRect;

        //Mask 

        [SerializeField] private float _currentStamina = 0;
        [SerializeField] private float _maxStamina = 0;

        private float _frameStartWidth = 1920;
        private float _frameStepValue = 1.5f;

        [SerializeField] private float _maskPaddingEmpty;
        [SerializeField] private float _maskPaddingFull;


        void OnEnable()
        {
            _frameStartWidth = staminaFrame.rect.width;
            PlayerEvents.OnPlayerStaminaChanged += HandleStaminaChanged;
            PlayerEvents.OnPlayerGetReward += HandleGetNewReward;
        }

        void OnDisable()
        {
            PlayerEvents.OnPlayerStaminaChanged -= HandleStaminaChanged;
            PlayerEvents.OnPlayerGetReward -= HandleGetNewReward;
        }

        private void Start()
        {
            // _maskPaddingFull = mask.padding.z;
            _maskPaddingFull = mask.padding.z;
            // _maskPaddingEmpty = maskRect.rect.width;
            _maskPaddingEmpty = 184f;
        }

        private void HandleStaminaChanged(float newStamina, float maxStamina)
        {
            _currentStamina = newStamina;
            _maxStamina = maxStamina;
            UpdateStaminaVisual();
        }

        private void UpdateStaminaVisual()
        {
            float staminaPercentage = _currentStamina / _maxStamina;
            float targetPaddingZ = Mathf.Lerp(_maskPaddingEmpty, _maskPaddingFull, staminaPercentage);
            // float targetWidth = _currentStamina * _maxRightMask / _maxStamina;
            // float newRightMask = _maxRightMask + _initialRightMask - targetWidth;
            var padding = mask.padding;
            padding.z = targetPaddingZ;
            mask.padding = padding;
        }

        private void HandleGetNewReward(RewardData data)
        {
            // foreach (var rewards in data.rewards)
            // {
            //     if (rewards.type == SimpleRewardType.Stamina)
            //     {
            //         staminaUpgrades += rewards.value;
            //     }
            // }
            //
            // UpdateStaminaVisual();
        }
    }
}