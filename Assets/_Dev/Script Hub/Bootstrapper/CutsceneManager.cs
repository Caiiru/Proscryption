using UnityEngine;

namespace proscryption
{
    public class CutsceneManager : MonoBehaviour
    {
        public void EndAnimation()
        {
            AppManager.Instance.ChangeAppState(AppState.Playing);
        }
    }
}
