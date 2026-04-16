using UnityEngine;

namespace proscryption
{
    public class PlayerInteractHUD : MonoBehaviour
    {
        private TMPro.TextMeshProUGUI _interactText;
        void Start()
        {
            _interactText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            PlayerEvents.OnPlayerEnterInteractRange += EnterInteractRangeHandler;
        }
        void OnDisable()
        {
            PlayerEvents.OnPlayerEnterInteractRange -= EnterInteractRangeHandler;

        }
        private void EnterInteractRangeHandler(string interactableName)
        { 
            _interactText.text = "Press E to interact with " + interactableName;
        }
    }
}
