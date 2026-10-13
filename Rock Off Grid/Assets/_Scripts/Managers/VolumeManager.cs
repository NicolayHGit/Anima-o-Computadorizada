using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager instance;

    private VCA _musicVCA;
    private VCA _sfxVCA;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadVolume();
    }

    private void InitSetVolumes()
    {
        float musicValue = PlayerPrefs.GetFloat("MusicVolume");
        float sfxValue = PlayerPrefs.GetFloat("SFXVolume");

        SetMusicVolume(musicValue);
        SetSFXVolume(sfxValue);
    }

    public void LoadVolume()
    {
        _musicVCA = RuntimeManager.GetVCA("vca:/Music");
        _sfxVCA = RuntimeManager.GetVCA("vca:/SFX");


        if (PlayerPrefs.HasKey("MusicVolume"))
            InitSetVolumes();
        else
        {
            SetMusicVolume(1f);
            SetSFXVolume(1f);
        }
    }

    public void SetMusicVolume(float value)
    {
        float volume = value;

        _musicVCA.setVolume(volume);

        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float value)
    {
        float volume = value;

        _sfxVCA.setVolume(volume);

        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}
