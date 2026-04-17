using System;
using TMPro;
using UnityEngine;

namespace proscryption
{
    public class PlayerInteractHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _interactText;
        void Start()
        {
            if (_interactText == null)
            {
                Debug.LogError("PlayerInteractHUD: No TextMeshProUGUI component found in children.");
                return;
            }

            _interactText.gameObject.SetActive(false);
            PlayerEvents.OnPlayerEnterInteractRange += EnterInteractRangeHandler;
            PlayerEvents.OnPlayerLeaveInteractRange += LeaveInteractRangeHandler;
        }

        void OnDisable()
        {
            PlayerEvents.OnPlayerEnterInteractRange -= EnterInteractRangeHandler;

        }

        private void LeaveInteractRangeHandler()
        {
            if (_interactText)
                _interactText.gameObject.SetActive(false);
        }
        private void EnterInteractRangeHandler(string interactableName)
        {
            _interactText.gameObject.SetActive(true);
            _interactText.text = "Press E to interact with " + interactableName;
        }
    }
}
