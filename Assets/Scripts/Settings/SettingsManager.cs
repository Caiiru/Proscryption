using UnityEngine;

namespace proscryption
{
    public class SettingsManager : MonoBehaviour
    {
        public GameObject[] screens;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void OpenSettingsScreen()
        {
            HideScreens();
            foreach (GameObject screen in screens)
            {
                screen.SetActive(true);
                break;
            }
        }
        public void HideScreens()
        {
            foreach (GameObject screen in screens)
            {
                screen.SetActive(false);
            }

        }
    }
}
