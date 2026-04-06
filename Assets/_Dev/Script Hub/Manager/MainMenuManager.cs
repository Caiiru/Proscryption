using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class MainMenuManager : MonoBehaviour
    {

        [Header("Main Menu")]
        public Button PlayButton;
        public Button ConfigButton;
        public Button QuitButton;

        void Start()
        {
            BindButtons();
        }
        void BindButtons()
        {
            if (PlayButton == null || ConfigButton == null || QuitButton == null)
            {
                Debug.LogWarning("Main Menu - Buttons not assigned in the inspector.");
                return;
            }

            PlayButton.onClick.AddListener(() => AppManager.Instance.ChangeAppState(AppState.Cutscene).Forget());
            ConfigButton.onClick.AddListener(() => AppManager.Instance.ChangeAppState(AppState.Config).Forget());
            QuitButton.onClick.AddListener(() => Application.Quit());
        }
    }
}
