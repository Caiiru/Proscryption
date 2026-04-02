using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class PlayerHealthHUD : MonoBehaviour
    {
        Slider _slider;
        bool isFull = false;
        void Awake()
        {
            _slider = GetComponent<Slider>();
            EventManager.OnPlayerHealthChanged += HandleHealthChanged;
        }
        void HandleHealthChanged(int newHealth, int maxHealth)
        {
            if (this.gameObject.activeSelf == false)
                this.gameObject.SetActive(true);


            _slider.value = (float)newHealth / maxHealth;
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
            EventManager.OnPlayerHealthChanged -= HandleHealthChanged;
        }

    }
}
