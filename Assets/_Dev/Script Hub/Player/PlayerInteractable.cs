using UnityEngine;

namespace proscryption
{
    public class PlayerInteractable : MonoBehaviour
    {
        public int interactRadius = 3;
        public const string INTERACTABLE_TAG = "Interactable";
        private bool _canInteract = false;

        private GameObject _currentInteractable;


        [Header("Debug")]
        public bool ShowDebug = false;

        void Start()
        {
            PlayerEvents.OnPlayerCastInteract += Interact;
        }
        void OnDisable()
        {

            PlayerEvents.OnPlayerCastInteract -= Interact;
        }
        public void Interact()
        {
            Debug.Log("Player attempted to interact!");
            _currentInteractable?.GetComponent<IInteractable>()?.Interact();
        }

        void OnTriggerEnter(Collider other)
        {
            if (_currentInteractable) return;
            if (other.CompareTag(INTERACTABLE_TAG))
            {
                _currentInteractable = other.gameObject;
                Debug.Log("Player entered interactable radius of " + other.name);
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(INTERACTABLE_TAG) && other.gameObject == _currentInteractable)
            {
                Debug.Log("Player exited interactable radius of " + other.name);
                _currentInteractable = null;
            }
        }


        public void OnDrawGizmosSelected()
        {
            if (!ShowDebug) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
