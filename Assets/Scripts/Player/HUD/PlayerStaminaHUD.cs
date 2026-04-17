using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class PlayerStaminaHUD : MonoBehaviour
    {
        bool isPlaying = false;
        bool isFull = false;
        Slider _slider;
        void Awake()
        {
            _slider = GetComponent<Slider>();
            EventManager.OnPlayerStaminaChanged += HandleStaminaChanged;
            EventManager.OnGameLoaded += () => this.isPlaying = true;
        }

        void Start()
        {
            if (Initializer.Instance == null)
            {
                this.isPlaying = true;
            }

            this.gameObject.SetActive(false);
        }
        void HandleStaminaChanged(float currentStamina, float maxStamina)
        {
            if (!isPlaying) return;
            if (this.gameObject.activeSelf == false)
                this.gameObject.SetActive(true);
            _slider.value = (float)currentStamina / maxStamina;
            CheckIfISFull();

        }
        void CheckIfISFull()
        {
            if (_slider.value >= 0.99f)
            {
                isFull = true;
            }
            else
            {
                isFull = false;
            }
            if (isFull)
            {
                this.gameObject.SetActive(false);
            }
        }
        void OnDestroy()
        {
            EventManager.OnPlayerStaminaChanged -= HandleStaminaChanged;
        }

    }
}
