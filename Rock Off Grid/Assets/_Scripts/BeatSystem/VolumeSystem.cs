using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSystem : MonoBehaviour
{
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    private void Start()
    {
        LoadVolume();
    }

    private void InitSetVolumes()
    {
        _musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        _sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }

    public void LoadVolume()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
            InitSetVolumes();
    }

    public void SetMusicVolume()
    {
        float volume = _musicVolumeSlider.value;

        VolumeManager.instance.SetMusicVolume(volume);
    }

    public void SetSFXVolume()
    {
        float volume = _sfxVolumeSlider.value;

        VolumeManager.instance.SetSFXVolume(volume);
    }

    

}
