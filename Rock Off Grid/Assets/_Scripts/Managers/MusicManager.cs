using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    [SerializeField] private EventReference _mainMenu;
    [SerializeField] private EventReference _battleMusic;

    private EventInstance _musicInstance;

    private EVENT_CALLBACK _beatCallback;

    private GCHandle _handle;

    private int _currentBeat;

    private int _lastBeat = 0;
    private int _beatCount = 0;

    public int BeatCount => _beatCount;
    public int CurrentBeat => _currentBeat;

    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadMusicBank();
    }

    private void OnDisable()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _musicInstance.release();   

        _beatCallback = null;
    }

    private void LoadMusicBank()
    {
        RuntimeManager.LoadBank("Master");
        RuntimeManager.LoadBank("Music");

        RuntimeManager.WaitForAllSampleLoading();

        StartMusic();
    }

    public float GetMusicTime()
    {
        _musicInstance.getTimelinePosition(out int position);
        float musicTimeS = position / 1000f;
        return musicTimeS;
    }

    public int GetMusicTimeMS()
    {
        _musicInstance.getTimelinePosition(out int position);
        return position;
    }

    public void StartMusic()
    {
        _musicInstance = RuntimeManager.CreateInstance(_mainMenu);
        _musicInstance.start();
    }

    public void StartBattleMusic()
    {
        StopMusic();

        _musicInstance = RuntimeManager.CreateInstance(_battleMusic);
        _beatCallback = BeatCallback;

        _musicInstance.setCallback(_beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        _handle = GCHandle.Alloc(this);
        _musicInstance.setUserData(GCHandle.ToIntPtr(_handle));

        _musicInstance.start();
    }

    public void StopMusic()
    {
        if (_handle.IsAllocated)
            _handle.Free();

        _musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _musicInstance.release();
    }

    private void SetIntensityA(int intensity)
    {
        _musicInstance.setParameterByName("IntensityL", intensity);
    }

    private void SetIntensityB(int intensity)
    {
        _musicInstance.setParameterByName("IntensityR", intensity);
    }

    public void SetSinglePlayerIntensity(int rank)
    {
        int intensity = Mathf.Clamp(rank - 1, 0, 4);

        SetIntensityA(intensity);
        SetIntensityB(intensity);
    }

    public void SetPlayerIntensity(PlayerType type, int rank)
    {
        int intensity = Mathf.Clamp(rank - 1, 0, 4);

        if(type == PlayerType.PLAYERA)
            SetIntensityA(intensity);
        else
            SetIntensityB(intensity);
    }

    public void InitMultiPanMusic()
    {
        _musicInstance = RuntimeManager.CreateInstance(_battleMusic);

        _musicInstance.setVolume(0f); // Desativa o volume principal para evitar sobreposição

        _musicInstance.start();
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private static RESULT BeatCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        if(GameManager.instance.State != GameState.Playing) return RESULT.OK;

        if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            var parameter = (TIMELINE_BEAT_PROPERTIES)System.Runtime.InteropServices.Marshal.PtrToStructure(
                parameterPtr, typeof(TIMELINE_BEAT_PROPERTIES));

            FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

            instance.getUserData(out IntPtr userDataPtr);
            if (userDataPtr != IntPtr.Zero)
            {
                GCHandle handle = GCHandle.FromIntPtr(userDataPtr);
                MusicManager manager = (MusicManager)handle.Target;

                if (parameter.beat < manager._lastBeat)
                    manager._beatCount += (parameter.beat);
                else
                    manager._beatCount++;

                manager._lastBeat = parameter.beat;
                manager._currentBeat = manager._beatCount;
            }
        }

        return RESULT.OK;
    }
}