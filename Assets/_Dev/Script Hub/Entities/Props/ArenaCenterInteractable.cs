using UnityEngine;

namespace proscryption
{
    public class ArenaCenterInteractable : MonoBehaviour, IInteractable
    {
        public string InteractableName = "Ritual";
        public bool canInteract = true;

        SphereCollider _collider;

        float _baseRadius = 0f;

        void Start()
        {
            this.transform.name = InteractableName;
            _collider = this.GetComponent<SphereCollider>();
            _baseRadius = _collider.radius;
        }
        public string GetInteractName()
        {
            return InteractableName;
        }

        public void Interact()
        {
            if (!canInteract)
            {
                return;
            }

            EventManager.BroadcastArenaStart();
            canInteract = false;
            _collider.radius = 0;
        }

        public bool CanInteract()
        {
            return canInteract;
        }
    }
}
