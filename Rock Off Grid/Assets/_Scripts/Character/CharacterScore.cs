using Unity.Netcode;
using UnityEngine;

public class CharacterScore : NetworkBehaviour
{
    [SerializeField] private Character _character;
    [SerializeField] private ScoreConfig _scoreConfigSO;

    private int _curScore;
    private int _curRank;
    private int _perfectHits;
    private int _goodHits;
    private int _normalHits;
    private int _misses;
    private int _combo;
    private int _maxCombo;

    private NetworkVariable<int> _netScore = new NetworkVariable<int>();
    private NetworkVariable<int> _netRank = new NetworkVariable<int>();
    private NetworkVariable<int> _netPerfectHits = new();
    private NetworkVariable<int> _netGoodHits = new();
    private NetworkVariable<int> _netNormalHits = new();
    private NetworkVariable<int> _netMisses = new();
    private NetworkVariable<int> _netCombo = new();
    private NetworkVariable<int> _netMaxCombo = new();

    [SerializeField] private int _multiplierTracker;

    public int CurScore => GameManager.instance.IsOnline() ? _netScore.Value : _curScore;
    public int CurMultiplier => GameManager.instance.IsOnline() ? _netRank.Value : _curRank;
    public int Combo => GameManager.instance.IsOnline() ? _netCombo.Value : _combo;
    public int MaxCombo => GameManager.instance.IsOnline() ? _netMaxCombo.Value : _maxCombo;
    public int PerfectHits => GameManager.instance.IsOnline() ? _netPerfectHits.Value : _perfectHits;
    public int GoodHits => GameManager.instance.IsOnline() ? _netGoodHits.Value : _goodHits;
    public int NormalHits => GameManager.instance.IsOnline() ? _netNormalHits.Value : _normalHits;
    public int Misses => GameManager.instance.IsOnline() ? _netMisses.Value : _misses;

    public void Init()
    {
        _curScore = 0;
        _curRank = 1;
        _perfectHits = 0;
        _goodHits = 0;
        _normalHits = 0;
        _misses = 0;
        _combo = 0;
}

    public void NetworkInit()
    {
        _netScore.Value = 0;
        _netRank.Value = 1;
        _netMisses.Value = 0;
        _netPerfectHits.Value = 0;
        _netGoodHits.Value = 0;
        _netNormalHits.Value = 0;
        _netCombo.Value = 0;
    }

    private void UpdateScore()
    {
        _character.UpdateScore();
    }

    public void UpdateSongIntensity()
    {
        if (!GameManager.instance.IsOnline())
        {
            if (GameManager.instance.Mode == GameMode.SinglePlayer)
                MusicManager.instance.SetSinglePlayerIntensity(CurMultiplier);
            else
                MusicManager.instance.SetPlayerIntensity(_character.Type, CurMultiplier);
        }

        if (GameManager.instance.IsOnline())
            MusicManager.instance.SetPlayerIntensity(_character.Type, CurMultiplier);
    }

    private void RegisterHit(ref int local, ref NetworkVariable<int> net)
    {
        if (GameManager.instance.IsOnline())
            net.Value++;
        else
            local++;
    }

    private void UpdateCombo()
    {
        if (GameManager.instance.IsOnline())
        {
            _netCombo.Value++;

            if (_netCombo.Value > _netMaxCombo.Value)
                _netMaxCombo.Value = _netCombo.Value;
        }
        else
        {
            _combo++;

            if (_combo > _maxCombo)
                _maxCombo = _combo;
        }
    }

    private void RegisterMiss()
    {
        if (GameManager.instance.IsOnline())
            _netMisses.Value++;
        else
            _misses++;

        ResetCombo();
    }

    private void ResetCombo()
    {
        if (GameManager.instance.IsOnline())
            _netCombo.Value = 0;
        else
            _combo = 0;
    }

    public string GetScoreText()
    {
        int index = Mathf.Clamp(CurMultiplier - 1, 0, _scoreConfigSO.multiplierThresholds.Count - 1);
        return _scoreConfigSO.multiplierThresholds[index].rank;
    }

    public int GetThreshold()
    {
        return _multiplierTracker;
    }

    public void NoteHit()
    {
        int maxRank = _scoreConfigSO.multiplierThresholds.Count;

        if (CurMultiplier < maxRank)
        {
            _multiplierTracker += Mathf.Clamp(Combo / 5, 1, 3);

            if (_scoreConfigSO.multiplierThresholds[CurMultiplier - 1].multiplierThreshold <= _multiplierTracker)
            {
                _multiplierTracker = 0;
                if (GameManager.instance.IsOnline() && NetworkManager.Singleton.IsServer)
                    _netRank.Value = Mathf.Min(_netRank.Value + 1, maxRank);
                else
                    _curRank = Mathf.Min(_curRank + 1, maxRank);

                UpdateSongIntensity();
            }
        }
        UpdateCombo();
        UpdateScore();
    }

    public void PerformActionBeat(TimingAccuracyType accuracy)
    {
        if (GameManager.instance.IsOnline() && !NetworkManager.Singleton.IsServer) return;

        switch (accuracy)
        {
            case TimingAccuracyType.Normal:
                NormalHit();
                break;
            case TimingAccuracyType.Good:
                GoodHit();
                break;
            case TimingAccuracyType.Perfect:
                PerfectHit();
                break;
            case TimingAccuracyType.Miss:
                NoteMissed();
                break;
        }
    }

    public void NormalHit()
    {
        RegisterHit(ref _normalHits, ref _netNormalHits);

        if (GameManager.instance.IsOnline() && NetworkManager.Singleton.IsServer)
            _netScore.Value += _scoreConfigSO.scorePerNote * CurMultiplier;
        else
            _curScore += _scoreConfigSO.scorePerNote * CurMultiplier;
        
        NoteHit();
    }

    public void GoodHit()
    {
        RegisterHit(ref _goodHits, ref _netGoodHits);

        if (GameManager.instance.IsOnline() && NetworkManager.Singleton.IsServer)
            _netScore.Value += _scoreConfigSO.scorePerGoodNote * CurMultiplier;
        else
            _curScore += _scoreConfigSO.scorePerGoodNote * CurMultiplier;
        
        NoteHit();
    }

    public void PerfectHit()
    {
        RegisterHit(ref _perfectHits, ref _netPerfectHits);

        if (GameManager.instance.IsOnline() && NetworkManager.Singleton.IsServer)
            _netScore.Value += _scoreConfigSO.scorePerPerfectNote * CurMultiplier;
        else
            _curScore += _scoreConfigSO.scorePerPerfectNote * CurMultiplier;
        
        NoteHit();
    }

    public void NoteMissed()
    {
        RegisterMiss();

        if (CurMultiplier > 1)
        {

            _multiplierTracker = _multiplierTracker - (1 * CurMultiplier);

            if (_scoreConfigSO.multiplierThresholds[CurMultiplier - 1].multiplierThreshold >= _multiplierTracker)
            {
                _multiplierTracker = _scoreConfigSO.multiplierThresholds[CurMultiplier - 1].multiplierThreshold - 1;
                if (GameManager.instance.IsOnline() && NetworkManager.Singleton.IsServer)
                    _netRank.Value = (CurMultiplier == 1) ? 1 : CurMultiplier - 1;
                else
                    _curRank = (CurMultiplier == 1) ? 1 : CurMultiplier - 1;

                UpdateSongIntensity();
            }
        }
        UpdateScore();
    }

    public void AddDefenseBonus(TimingAccuracyType accuracy)
    {
        PerformActionBeat(accuracy);
    }
}
