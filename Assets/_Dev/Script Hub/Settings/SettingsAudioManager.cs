using UnityEngine;
using UnityEngine.UI;

namespace proscryption
{
    public class SettingsAudioManager : MonoBehaviour
    {
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;

        void Start()
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        }
        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }
        public void SetMusicVolume(float volume)
        {
            // Implement music volume control logic here
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }
        public void SetSFXVolume(float volume)
        {
            // Implement SFX volume control logic here
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }
    }
}
