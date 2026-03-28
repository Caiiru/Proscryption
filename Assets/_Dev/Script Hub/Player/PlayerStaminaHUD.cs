using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class PlayerStaminaHUD : MonoBehaviour
    {
        Slider _slider;
        void Awake()
        {
            _slider = GetComponent<Slider>();
            EventManager.OnPlayerStaminaChanged += HandleStaminaChanged;
        }
        void HandleStaminaChanged(float currentStamina, float  maxStamina)
        {
            
            _slider.value = (float)currentStamina / maxStamina;
        }
        void OnDestroy()
        {
            EventManager.OnPlayerStaminaChanged -= HandleStaminaChanged;
        }

    }
}
