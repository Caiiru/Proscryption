using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace proscryption
{
    public class CutsceneManager : MonoBehaviour
    {
        public VideoPlayer cutsceneVideo; 

        void Awake()
        {
            cutsceneVideo.loopPointReached += OnVideoEnd;
        }
        void OnVideoEnd(VideoPlayer vp)
        {
            Debug.Log("Video Was Ended");
            AppManager.Instance.ChangeAppState(AppState.Playing);

        }
        void Update()
        {
            if (Keyboard.current.anyKey.isPressed)
            {
                Debug.Log("Try to skip");
                AppManager.Instance.ChangeAppState(AppState.Playing);
            } 
        }
        public void EndAnimation()
        {
            AppManager.Instance.ChangeAppState(AppState.Playing);
        }
    }
}
