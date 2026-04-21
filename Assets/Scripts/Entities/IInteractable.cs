using UnityEngine;

namespace proscryption
{
    public interface IInteractable
    {
        public void Interact();
        public string GetInteractName();
        public bool CanInteract();
    }
}
