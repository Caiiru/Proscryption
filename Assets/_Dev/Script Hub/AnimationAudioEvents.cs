using FMOD.Studio;
using UnityEngine;
using FMODUnity;
using JetBrains.Annotations;

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

        Debug.LogWarning($"No sound found for event: {eventName} from {transform.gameObject.name}");
    }

    [CanBeNull]
    public EventInstance? PlaySoundAndSaveReference(string eventName)
    {
        foreach (var e in events)
        {
            if (e.name == eventName)
            {
                EventInstance i = RuntimeManager.CreateInstance(e.fmodEvent);
                i.start();
                // Debug.Log($"Sound played: {eventName}");
                return i;
            }
        }

        return null;
    }
}