using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class BeatManager : MonoBehaviour
{
    public static BeatManager instance;

    [SerializeField] private float _bpm;
    [SerializeField] private float _beatThreshold = 0.1f;
    [SerializeField] private List<Intervals> _intervals;

    private bool _isPlaying = false;

    private bool _previewMode = false;
    private float _previewTimer = 0f;

    private float _beat;
    private int _lastProcessedBeat = -1;
    private float _songtime => MusicManager.instance.GetMusicTime();
    private float _netSongtime;

    private double _networkStartTime;

    public float BeatDuration => _beat;
    public float SongTime => GameManager.instance.IsOnline() ? _netSongtime : _songtime;
    public float NetSongTime => _netSongtime;

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
        Init();
    }

    private void Update()
    {
        if (GameManager.instance.IsOnline())
            _netSongtime = (float)(NetworkManager.Singleton.ServerTime.Time - _networkStartTime);

        if (_previewMode)
        {
            _previewTimer += Time.deltaTime;
            foreach (Intervals interval in _intervals)
            {
                float sampleTime = _previewTimer / interval.GetIntervalLength(_bpm);
                interval.CheckForNewInterval(sampleTime);
            }
            return;
        }

        if (!_isPlaying) return;

        int currentBeat = MusicManager.instance.CurrentBeat;

        if (currentBeat == _lastProcessedBeat) return;

        foreach (var interval in _intervals)
        {
            if ((currentBeat % interval.Steps) == 0)
                interval.Invoke();
        }

        _lastProcessedBeat = currentBeat;
    }

    private void Init()
    {
        _beat = 60f / _bpm;

        foreach (var interval in _intervals)
            interval.Reset();
    }

    public void StartPreviewBeat()
    {
        _previewTimer = 0f;
        _previewMode = true;
        _isPlaying = false;
    }

    public void StopPreviewBeat()
    {
        _previewMode = false;
    }

    public void StartBeat()
    {
        _isPlaying = true;

        foreach (var interval in _intervals)
            interval.Reset();

        MusicManager.instance.StartBattleMusic();
        Init();
    }

    public void StopBeat()
    {
        _isPlaying = false;
        MusicManager.instance.StopMusic();
    }

    public void ResetManager()
    {
        StopAllCoroutines();
        _isPlaying = false;
        _previewMode = false;

        _networkStartTime = 0;
        _netSongtime = 0;
    }

    public IEnumerator ScheduleStart(double networkStartTime)
    {
        _networkStartTime = networkStartTime;
        Debug.Log($"Agendando início da música para o tempo de rede: {_networkStartTime} - {NetworkManager.Singleton.LocalTime.Time}");
        // espera até o tempo da rede chegar
        while (NetworkManager.Singleton.LocalTime.Time < _networkStartTime)
            yield return null;

        // toca a música exatamente no tempo de rede combinado
        StartBeat();
    }

    public int GetCurrentBeat()
    {
        if(GameManager.instance.IsOnline())
            return Mathf.FloorToInt(_netSongtime / BeatDuration);
        else
            return Mathf.FloorToInt(_songtime / BeatDuration);
    }

    public float GetNextBeatTime(int beatsAhead = 1)
    {
        int currentBeat = GetCurrentBeat();
        int targetBeat = currentBeat + beatsAhead;
        return targetBeat * BeatDuration;
    }


    public TimingAccuracyType GetTimingAccuracy()
    {
        float songTime = SongTime;

        float beatFloat = songTime / BeatDuration;
        float nearestBeat = Mathf.Round(beatFloat);
        float distance = Mathf.Abs(beatFloat - nearestBeat) * BeatDuration;

        // Aqui você define as janelas de acerto
        if (distance < 0.05f) return TimingAccuracyType.Perfect; // 50ms
        if (distance < 0.1f) return TimingAccuracyType.Good;     // 100ms
        if (distance < 0.15f) return TimingAccuracyType.Normal;  // 150ms
        return TimingAccuracyType.Miss;
    }

    public TimingAccuracyType GetTimingAccuracyServer(double clientTime)
    {
        double serverTime = NetworkManager.Singleton.ServerTime.Time;

        // calcula latência
        double latency = serverTime - clientTime;
        latency = Mathf.Clamp((float)latency, 0, 0.3f);

        // corrige o tempo (estimativa do momento real do input)
        double correctedTime = serverTime - latency * 0.5;

        float nearestBeat = Mathf.Round((float)correctedTime / BeatDuration) * BeatDuration;
        float distance = Mathf.Abs((float)correctedTime - nearestBeat);

        if (distance < 0.05f) return TimingAccuracyType.Perfect;
        if (distance < 0.1f) return TimingAccuracyType.Good;
        if (distance < 0.15f) return TimingAccuracyType.Normal;
        return TimingAccuracyType.Miss;
    }

    public bool IsOnBeat()
    {
        float songPosition = _songtime;
        float beatTime = BeatDuration;

        float remainder = songPosition % beatTime;

        return remainder <= _beatThreshold || remainder >= beatTime - _beatThreshold;
    }

    public void RegisterInInterval(float step, UnityAction action)
    {
        foreach (var interval in _intervals) 
        {
            if(interval.Steps == step)
            {
                interval.RegisterAction(action);
                return;
            }
        }
    }

    public void UnRegisterInInterval(float step, UnityAction action)
    {
        foreach (var interval in _intervals)
        {
            if (interval.Steps == step)
            {
                interval.UnRegisterAction(action);
                return;
            }
        }
    }
}


[System.Serializable]
public class Intervals
{
    [SerializeField] private int _steps;
    [SerializeField] private UnityEvent _trigger = new UnityEvent();

    private int _lastInterval;

    public int Steps => _steps;

    public float GetIntervalLength(float bpm)
    {
        return 60f / (bpm * _steps);
    }

    public void CheckForNewInterval(float interval)
    {
        int intervalRound = Mathf.FloorToInt(interval);

        if (intervalRound < _lastInterval) _lastInterval = 0;

        while (_lastInterval < intervalRound)
        {
            _lastInterval++;
            _trigger?.Invoke();
        }
    }

    public void Invoke()
    {
        _trigger?.Invoke();
    }

    public void Reset()
    {
        _lastInterval = 0;
    }

    public void RegisterAction(UnityAction action)
    {
        _trigger.AddListener(action);
    }

    public void UnRegisterAction(UnityAction action)
    {
        _trigger.RemoveListener(action);
    }
}