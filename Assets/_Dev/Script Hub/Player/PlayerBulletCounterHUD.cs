
using Cysharp.Threading.Tasks; 
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

        PlayerStance _currentStance = PlayerStance.Standard;
        GameObject[] bulletIcons;

        [Header("Colors")]
        public Color StandardColor = Color.gray;
        public Color BloodColor = Color.darkRed;
        public Color LightColor = Color.yellow;
        public Color EmptyColor = Color.black;
        private Color _currentStanceColor = Color.gray;



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
            PlayerEvents.OnPlayerStanceChanged += HandleStanceChanged;
        }


        void OnDisable()
        {
            // EventManager.OnPlayerAttack -= HandleAttackPlayed;
            if (_weapon)
                _weapon.OnShoot -= HandleAttackPlayed;
            PlayerEvents.OnPlayerReloadEnded -= Reload;
            PlayerEvents.OnPlayerStanceChanged -= HandleStanceChanged;

        }

        private void HandleAttackPlayed()
        {
            ShootBullet(_weapon.bulletIndex).Forget();


        }
        public UniTask ShootBullet(int b_index)
        {
            if (b_index >= 0 && b_index < bulletIcons.Length)
            {
                bulletIcons[b_index].GetComponent<UnityEngine.UI.Image>().color = EmptyColor;
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
                bulletIcons[i].GetComponent<UnityEngine.UI.Image>().color = _currentStanceColor; // Reset color
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
        private void HandleStanceChanged(PlayerStance oldStance, PlayerStance newStance)
        {
            Debug.Log($"Stance changed from {oldStance} to {newStance}");
            _currentStance = newStance;
            Color bulletsColor = Color.black;
            switch (newStance)
            {
                case PlayerStance.Standard:
                    // bulletBackground.GetComponent<UnityEngine.UI.Image>().color = ColorUtility.TryParseHtmlString("#3C3C3C", out Color standardColor) ? standardColor : Color.gray;
                    bulletsColor = StandardColor;
                    break;
                case PlayerStance.Blood:
                    // bulletBackground.GetComponent<UnityEngine.UI.Image>().color = Color.red;
                    bulletsColor = BloodColor;
                    break;
                case PlayerStance.Light:
                    // bulletBackground.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
                    bulletsColor = LightColor;
                    break;
            }

            for (int i = 0; i < BaseWeapon.MAX_BULLETS; i++)
            {
                if (_weapon.bullets[i] == 0) continue;

                bulletIcons[i].GetComponent<UnityEngine.UI.Image>().color = bulletsColor;
            }
            _currentStanceColor = bulletsColor;
        }
    }
}
