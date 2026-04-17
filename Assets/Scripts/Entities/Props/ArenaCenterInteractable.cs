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

            SetupEvents();
        }
        void SetupEvents()
        {
            ArenaEvents.OnArenaWaveEnded += HandleWaveEnded;
        }
        void OnDestroy()
        {
            ArenaEvents.OnArenaWaveEnded -= HandleWaveEnded;
        }
        void HandleWaveEnded()
        {
            canInteract = true;
            _collider.radius = _baseRadius;
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

            ArenaEvents.BroadcastArenaStart();
            canInteract = false;
            _collider.radius = 0;
        }

        public bool CanInteract()
        {
            return canInteract;
        }
    }
}
