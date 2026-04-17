using UnityEngine;
using UnityEngine.InputSystem;

namespace proscryption
{
    public class CutsceneManager : MonoBehaviour
    {
        void Update()
        {
            if (Keyboard.current.anyKey.isPressed)
            {
                AppManager.Instance.ChangeAppState(AppState.Playing);
            }
        }
        public void EndAnimation()
        {
            AppManager.Instance.ChangeAppState(AppState.Playing);
        }
    }
}
