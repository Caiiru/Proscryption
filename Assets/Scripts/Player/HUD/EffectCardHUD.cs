
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class EffectCardHUD : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI effectText;

        public void Setup(Sprite newIcon, string effectText)
        {
            if (icon == null)
            {
                Debug.LogError("Icon is null");
            }
            if (effectText == null)
            {
                Debug.LogError("Effect Text is null");
            }
            icon.sprite = newIcon;
            this.effectText.text = $"{effectText}";
        }
    }


}
