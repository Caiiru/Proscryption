using System;
using UnityEngine;

namespace proscryption
{
    public class EventTrigger : MonoBehaviour
    {

        public string eventName;
        public TriggerType type = TriggerType.Enter;
        [SerializeField] private bool hasTriggered = false;
        private void OnTriggerEnter(Collider other)
        {
            if (type != TriggerType.Enter) return;
            if (hasTriggered) return;
            if (other.CompareTag("Player"))
            {
                hasTriggered = true;
                TriggerEvent();
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (type != TriggerType.Exit) return;
            if (hasTriggered) return;
            if (other.CompareTag("Player"))
            {
                hasTriggered = true;
                TriggerEvent();
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (type != TriggerType.Stay) return;
            if (hasTriggered) return;
            if (other.CompareTag("Player"))
            {
                hasTriggered = true;
                TriggerEvent();
            }
        }

        private void TriggerEvent()
        {
            Debug.Log($"Event '{eventName}' triggered!");
            // Here you can add logic to handle the event, such as invoking a delegate or sending a message to another script.
            
        }

    }
    [Serializable]
    public enum TriggerType { Enter, Exit, Stay }
}
