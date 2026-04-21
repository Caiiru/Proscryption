using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace proscryption
{
    public class PlayerHealthHUD : MonoBehaviour
    {
        bool isFull = false;

        public RectTransform healthFrame;
        public RectTransform fillBarRect;
        public RectMask2D _mask;

        //Mask
        private float _maxRightMask;
        private float _initialRightMask;


        private float _startWidth = 200;
        private float _frameStartWidth = 1920;
        private float _frameStepValue = 2;
        private float _healthStepValue = 100;
        private float lifeUpgrades = 0; //1 hp -> +4 size

        PlayerModel _playerModel;
        void Start()
        {
            _playerModel = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerModel>();

            _maxRightMask = fillBarRect.rect.width - _mask.padding.x - _mask.padding.z;
            _initialRightMask = _mask.padding.z;
        }
        void OnEnable()
        {
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
            this.gameObject.SetActive(true);
            healthFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1920+(lifeUpgrades*_frameStepValue));
            float targetWidth = newHealth * _maxRightMask / maxHealth;
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
                    lifeUpgrades = rewards.value;
                }
            }
        }


    }
}
