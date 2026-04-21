using System;
using System.Linq;
using UnityEngine;

namespace proscryption
{
    public class GraphicsManager : MonoBehaviour
    {
        //Resolution
        public TMPro.TMP_Dropdown resolutionDropdown;
        private Resolution[] _resolutions;

        public TMPro.TMP_Dropdown fullscreenModeDropdown;
        FullScreenMode[] _fullscreenModes = (FullScreenMode[])System.Enum.GetValues(typeof(FullScreenMode));

        void Start()
        {
            //Resolution
            _resolutions = Screen.resolutions.Reverse().ToArray(); // Get available resolutions and reverse to show highest first
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(_resolutions.Select(r => $"{r.width} x {r.height}").Distinct().ToList());
            resolutionDropdown.value = _resolutions.ToList().FindIndex(r => r.width == Screen.width && r.height == Screen.height);
            

            resolutionDropdown.onValueChanged.AddListener(SetResolution);

            //Fullscreen Mode
            fullscreenModeDropdown.ClearOptions();
            fullscreenModeDropdown.AddOptions(_fullscreenModes.Select(f => f.ToString()).ToList());
            fullscreenModeDropdown.value = Screen.fullScreenMode switch
            {
                FullScreenMode.ExclusiveFullScreen => 0,
                FullScreenMode.FullScreenWindow => 1, 
                FullScreenMode.Windowed => 2,
                _ => 0
            };
            fullscreenModeDropdown.onValueChanged.AddListener(SetFullscreenMode);

            PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
            PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        }

        private void SetFullscreenMode(int arg0)
        {
            FullScreenMode mode = _fullscreenModes[arg0];
            Screen.fullScreenMode = mode;
            Debug.Log("Fullscreen mode set to: " + mode);
            PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        }

        public void SetResolution(int index)
        {
            Resolution res = _resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
            Debug.Log("Resolution set to: " + res.width + "x" + res.height + " @ " + res.refreshRateRatio + "Hz");
        }
    }
}
