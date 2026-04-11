using System;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace proscryption
{
    public class PlayerBulletCounterHUD : MonoBehaviour
    {
        public Transform bulletBackground;
        public GameObject bulletForeground;
        private BaseWeapon _weapon;

        public bool _updateIcons;
        float radius = 80f; // Adjust as needed
        float angleOffset = 30f;

        GameObject[] bulletIcons;



        public void Initialize(BaseWeapon weapon)
        {
            _weapon = weapon;
            bulletIcons = new GameObject[BaseWeapon.MAX_BULLETS];
            for (int i = 0; i < BaseWeapon.MAX_BULLETS; i++)
            {
                GameObject icon = Instantiate(bulletForeground, bulletBackground);
                bulletIcons[i] = icon;

                // move icons in circle based on maxbullets based on center of bulletbackground     
                float angle = i * (360f / BaseWeapon.MAX_BULLETS) + (angleOffset * 3);
                Vector3 offset = new Vector3(-Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * radius;
                icon.GetComponent<RectTransform>().anchoredPosition = offset;

                icon.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
            }

            SetupEvents();
        }
        private void SetupEvents()
        {
            // EventManager.OnPlayerAttack += HandleAttackPlayed;
            if (_weapon)
            {
                _weapon.OnShoot += HandleAttackPlayed;
            }
            PlayerEvents.OnPlayerReloadEnded += Reload;
        }


        void OnDisable()
        {
            // EventManager.OnPlayerAttack -= HandleAttackPlayed;
            if (_weapon)
                _weapon.OnShoot -= HandleAttackPlayed;
            PlayerEvents.OnPlayerReloadEnded -= Reload;

        }

        private void HandleAttackPlayed()
        {
            ShootBullet(_weapon.bulletIndex).Forget();


        }
        public UniTask ShootBullet(int b_index)
        {
            if (b_index >= 0 && b_index < bulletIcons.Length)
            {
                bulletIcons[b_index].GetComponent<UnityEngine.UI.Image>().color = Color.red; // Change color to indicate shot
            }
            UniTask.Delay(2000).Forget();
            bulletBackground.Rotate(Vector3.forward, angleOffset * 2); // Rotate the entire counter
            _updateIcons = false;
            return UniTask.CompletedTask;
        }
        private void Reload()
        {
            for (int i = 0; i < BaseWeapon.MAX_BULLETS; i++)
            {
                bulletIcons[i].GetComponent<UnityEngine.UI.Image>().color = Color.white; // Reset color
            }

        }

        void Update()
        {
            if (_updateIcons) return;

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                _updateIcons = true;
                HandleAttackPlayed();

            }
        }
    }
}
