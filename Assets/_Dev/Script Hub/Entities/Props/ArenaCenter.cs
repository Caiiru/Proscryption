using UnityEngine;

namespace proscryption
{
    public class ArenaCenter : MonoBehaviour, IInteractable
    {
        public void Interact()
        {
            EventManager.BroadcastArenaStart();
        }
    }
}
