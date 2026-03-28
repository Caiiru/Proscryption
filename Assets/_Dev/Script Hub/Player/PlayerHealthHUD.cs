using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class PlayerHealthHUD : MonoBehaviour
    {
        Slider _slider;
        void Awake()
        {
            _slider = GetComponent<Slider>();
            EventManager.OnPlayerHealthChanged += HandleHealthChanged;
        }
        void HandleHealthChanged(int newHealth, int maxHealth)
        {
            
            _slider.value = (float)newHealth / maxHealth;
        }
        void OnDestroy()
        {
            EventManager.OnPlayerHealthChanged -= HandleHealthChanged;
        }

    }
}
