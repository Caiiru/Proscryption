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
        public float _minRightMask;
        [SerializeField]private float _maxRightMask;
        [SerializeField]private float _initialRightMask;

        [SerializeField]private float _currentStamina = 0;
        [SerializeField]private float _maxStamina = 0;

        private float _frameStartWidth = 1920;
        private float _frameStepValue = 1.5f;
        [SerializeField] private float _initialMaskWidth;
        [SerializeField] private float staminaUpgrades = 0; //1 -> +2 size

        void OnEnable()
        {

            _minRightMask = fillBarRect.rect.width - mask.padding.x - mask.padding.z - maskRect.rect.width; 
            _initialMaskWidth = maskRect.rect.width;
            _maxRightMask = fillBarRect.rect.width - mask.padding.x - mask.padding.z;
            _initialRightMask = mask.padding.z;
            _frameStartWidth = staminaFrame.rect.width;
            PlayerEvents.OnPlayerStaminaChanged += HandleStaminaChanged;
            PlayerEvents.OnPlayerGetReward += HandleGetNewReward;

        }
        void OnDisable()
        {

            PlayerEvents.OnPlayerStaminaChanged -= HandleStaminaChanged;
            PlayerEvents.OnPlayerGetReward -= HandleGetNewReward;
        }

        private void HandleStaminaChanged(float newStamina, float maxStamina)
        {
            _currentStamina = newStamina;
            _maxStamina = maxStamina;
            UpdateStaminaVisual();
        }
        private void UpdateStaminaVisual()
        {
            staminaFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _frameStartWidth + (staminaUpgrades * _frameStepValue));
            _maxRightMask = fillBarRect.rect.width - mask.padding.x - mask.padding.z;
            maskRect.
            SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialMaskWidth + (staminaUpgrades * _frameStepValue / 3.15f));
            maskFormatRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialMaskWidth + (staminaUpgrades * _frameStepValue ));

            float targetWidth = _currentStamina * _maxRightMask / _maxStamina;
            float newRightMask = _maxRightMask + _initialRightMask - targetWidth;
            var padding = mask.padding;
            padding.z = newRightMask ;
            mask.padding = padding;

        }
        private void HandleGetNewReward(RewardData data)
        {
            foreach (var rewards in data.rewards)
            {
                if (rewards.type == SimpleRewardType.Stamina)
                {
                    staminaUpgrades += rewards.value;
                }
            }
            UpdateStaminaVisual();
        }
    }
}
