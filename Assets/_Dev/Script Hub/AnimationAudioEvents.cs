using UnityEngine;
using FMODUnity;

public class AnimationAudioEvents : MonoBehaviour
{
    [System.Serializable]
    public class NamedEvent
    {
        public string name; // Ex: "sfx_playerFootStep"
        public EventReference fmodEvent;
    }

    [SerializeField] private NamedEvent[] events;

    public void PlaySound(string eventName)
    {
        foreach (var e in events)
        {
            if (e.name == eventName)
            {
                RuntimeManager.PlayOneShot(e.fmodEvent, transform.position);
                // Debug.Log($"Sound played: {eventName}");
                return;
            }
        }

        Debug.LogWarning($"No sound found for event: {eventName}");
    }
}