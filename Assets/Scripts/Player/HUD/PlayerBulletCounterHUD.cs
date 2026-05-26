using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace proscryption
{
    public class PlayerBulletCounterHUD : MonoBehaviour
    {
        public Transform bulletBackground;
        public Transform bulletContainerTransform;
        public Transform StandardStanceTransform;
        public Transform BloodStanceTransform;
        public Transform LightStanceTransform;

        public GameObject bulletForeground;
        private BaseWeapon _weapon;

        public float angleOffset = 30f;
        public float rotateAnimationDuration = 0.5f;

        PlayerStance _currentStance = PlayerStance.Standard;
        GameObject[] bulletIcons;


        public void Initialize(BaseWeapon weapon)
        {
            _weapon = weapon;
            bulletIcons = new GameObject[BaseWeapon.MAX_BULLETS];
            for (int i = 0; i < BaseWeapon.MAX_BULLETS; i++)
            {
                bulletIcons[i] = bulletContainerTransform.GetChild(i).gameObject;
                TextMeshProUGUI bulletNumber = bulletIcons[i].GetComponentInChildren<TextMeshProUGUI>();
                // bulletNumber.text = i.ToString();
            }

            StandardStanceTransform.gameObject.SetActive(true);
            BloodStanceTransform.gameObject.SetActive(false);
            LightStanceTransform.gameObject.SetActive(false);

            SetupEvents();
        }

        private async void SetupEvents()
        {
            // EventManager.OnPlayerAttack += HandleAttackPlayed;
            if (_weapon)
            {
                _weapon.OnShootAction += HandleAttackPlayed;
                _weapon.OnReloadBulletAction += HandleReloadBulletAction;
            }

            PlayerEvents.OnPlayerReloadEnded += Reload;
            PlayerEvents.OnPlayerStanceChanged += HandleStanceChanged;
            PlayerEvents.OnBulletChanged += HandleBulletChanged;
        }


        void OnDestroy()
        {
            // EventManager.OnPlayerAttack -= HandleAttackPlayed;
            if (_weapon)
            {
                _weapon.OnReloadBulletAction -= HandleReloadBulletAction;
                _weapon.OnShootAction -= HandleAttackPlayed;
            }

            PlayerEvents.OnPlayerReloadEnded -= Reload;
            PlayerEvents.OnPlayerStanceChanged -= HandleStanceChanged;
            PlayerEvents.OnBulletChanged -= HandleBulletChanged;
        }

        private void HandleAttackPlayed(int index)
        {
            ShootBullet(index).Forget();
        }

        UniTask ShootBullet(int index)
        {
            if (index >= 0 && index < bulletIcons.Length)
            {
                // bulletIcons[b_index].GetComponent<UnityEngine.UI.Image>().color = EmptyColor;
                bulletIcons[index].transform.gameObject.SetActive(false);
            }

            // RotateClockwise(b_index);
            // RotateAntiClockwise(index);
            // UniTask.Delay(2000).Forget();
            return UniTask.CompletedTask;
        }

        private void HandleBulletChanged(int index)
        {
            RotateAntiClockwise(index);
        }


        private void RotateAntiClockwise(int index)
        {
            bulletBackground.DORotate(new Vector3(0, 0, angleOffset * (index) * 2), rotateAnimationDuration,
                RotateMode.Fast); // Rotate the entire counter  
        }

        private void RotateClockwise(int index)
        {
            bulletBackground.DORotate(new Vector3(0, 0, -angleOffset * (index - 1) * 2), rotateAnimationDuration,
                RotateMode.Fast); // Rotate the entire counter 
        }

        private void RotateToIndex(int index)
        {
            Debug.Log(index);
            bulletBackground.DORotate(new Vector3(0, 0, angleOffset * (index) * 2), rotateAnimationDuration,
                RotateMode.FastBeyond360);
        }

        private void Reload()
        {
            // for (int i = 0; i < BaseWeapon.MAX_BULLETS; i++)
            // {
            //     bulletIcons[i].transform.gameObject.SetActive(true);
            // }
            //
            // Debug.Log("Reload");
        }

        private void HandleReloadBulletAction(int index, bool isRotating)
        {
            HandleReloadOneBullet(index, isRotating).Forget();
        }


        private async UniTask HandleReloadOneBullet(int index, bool rotate)
        {
            // Debug.Log("Reload one bullet");
            int i = 0;
            if (rotate)
            {
                RotateToIndex(index);
                await UniTask.WaitForSeconds(rotateAnimationDuration);
            }


            bulletIcons[index].transform.gameObject.SetActive(true);
        }

        private void HandleStanceChanged(PlayerStance oldStance, PlayerStance newStance)
        {
            // Debug.Log($"Stance changed from {oldStance} to {newStance}");
            _currentStance = newStance;
            Color bulletsColor = Color.black;
            StandardStanceTransform.gameObject.SetActive(false);
            BloodStanceTransform.gameObject.SetActive(false);
            LightStanceTransform.gameObject.SetActive(false);
            // bulletBackground.gam eObject.SetActive(false);
            switch (newStance)
            {
                case PlayerStance.Standard:
                    // bulletBackground.GetComponent<UnityEngine.UI.Image>().color = ColorUtility.TryParseHtmlString("#3C3C3C", out Color standardColor) ? standardColor : Color.gray;
                    bulletBackground.gameObject.SetActive(true);
                    StandardStanceTransform.gameObject.SetActive(true);
                    // bulletsColor = StandardColor;
                    break;
                case PlayerStance.Blood:
                    // bulletBackground.GetComponent<UnityEngine.UI.Image>().color = Color.red;
                    BloodStanceTransform.gameObject.SetActive(true);
                    // bulletsColor = BloodColor;
                    break;
                case PlayerStance.Light:
                    // bulletBackground.GetComponent<UnityEngine.UI.Image>().color = Color.yellow;
                    LightStanceTransform.gameObject.SetActive(true);
                    // bulletsColor = LightColor;
                    break;
            }

            for (int i = 0; i < BaseWeapon.MAX_BULLETS; i++)
            {
                if (_weapon.bullets[i] == 0) continue;

                // bulletIcons[i].GetComponent<UnityEngine.UI.Image>().color = bulletsColor;
            }
        }
    }
}