using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private BeatManager _beatManager;
    [SerializeField] private CharacterManager _characterManager;
    [SerializeField] private LevelManager _levelManager;

    private GameState _gameState = GameState.None;
    private GameMode _gameMode;

    public GameState State => _gameState;
    public GameMode Mode => _gameMode;

    public static UnityEvent<GameState> OnGameSateChanged;


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
        ChangeGameState(GameState.Menu);
    }

    public bool IsOnline()
    {
        return (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient));
    }

    public void StartBeat()
    {
        BeatManager.instance.StopPreviewBeat();
        Time.timeScale = 1;
        if (IsOnline()) return;
        BeatManager.instance.StartBeat();
    }

    public void ChangeGameState(GameState state)
    {
        _gameState = state;

        switch (state)
        {
            case GameState.Menu:
                break;
            case GameState.Loading:
                LevelManager.instance.LoadScene("BeatRhythm");
                break;
            case GameState.Tunning:
                break;
            case GameState.Playing:
                break;
            case GameState.Endgame:
                EndBattle();
                break;
            default:
                break;
        }

        OnGameSateChanged?.Invoke(_gameState);
    }

    public void SetGameMode(GameMode mode)
    {
        _gameMode = mode;
        ChangeGameState(GameState.Loading);
    }

    public Character GetCharacter(PlayerType type)
    {
        if(type == PlayerType.PLAYERA)
            return CharacterManager.instance.PlayerA;
        else
            return _characterManager.PlayerB;
    }

    public void CharacterDied(PlayerType playerDied)
    {
        CanvasManager.instance.EndGame(playerDied, CharacterManager.instance.PlayerA.Score, CharacterManager.instance.PlayerB.Score);
        ChangeGameState(GameState.Endgame);
    }

    public void EndBattle()
    {
        BeatManager.instance.StopBeat();
        BattleManager.instance.ChangeBattleState(BattleState.Endgame);
        GridManager.instance.ClearAllTiles();
        CharacterManager.instance.BattleEnd();

        if(IsOnline() && NetworkManager.Singleton.IsServer)
            BeatSyncNetwork.instance.FinishBattleServerRpc();
    }

    private void ClearObjects()
    {
        if (BeatSyncNetwork.instance != null)
            Destroy(BeatSyncNetwork.instance.gameObject);

        if (NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);

        if(BeatManager.instance != null)
            Destroy(BeatManager.instance.gameObject);

        if(CharacterManager.instance != null)
            Destroy(CharacterManager.instance.gameObject);
    }

    public void ReturnFromBattle()
    {
        StopAllCoroutines();
        MusicManager.instance.StopMusic();
        BeatManager.instance.ResetManager();

        BattleManager.instance.FinishBattle();
        CharacterManager.instance.CleanCharacters();

        ClearObjects();

        LevelManager.instance.LoadScene("MainMenu");
    }
}
