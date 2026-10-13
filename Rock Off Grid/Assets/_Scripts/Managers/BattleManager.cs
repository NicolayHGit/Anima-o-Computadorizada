using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [SerializeField] private GridManager _gridManager;
    [SerializeField] private int _countDownTimer = 3;

    private BattleState _battleState;

    public BattleState State => _battleState;
    public int CountdownTimer => _countDownTimer;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void InstantiatePlayers()
    {
        switch (GameManager.instance.Mode)
        {
            case GameMode.SinglePlayer:
                CharacterManager.instance.InitializeSinglePlayer();
                break;
            case GameMode.MultiLocal:
                CharacterManager.instance.InitializeMultiLocalPlayers();
                break;
            case GameMode.MultiPlayer:
                if (!NetworkManager.Singleton.IsServer) return;
                if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;
                CharacterManager.instance.InitializeMultiPlayers();
                break;
        }
    }

    private void SpawnTilesAndCharacters()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            CharacterManager.instance.PlayerA.GenerateTilesClientRpc();
            CharacterManager.instance.PlayerB.GenerateTilesClientRpc();
        }

        if (CharacterManager.instance.PlayerA != null)
        {
            Vector2Int pos = new Vector2Int(0, 1);
            _gridManager.MoveCharacterToTile(CharacterManager.instance.PlayerA, pos);
            CanvasManager.instance.InitCharacterUI(CharacterManager.instance.PlayerA);

            if (GameManager.instance.IsOnline())
                CharacterManager.instance.PlayerA.CharacterSyncBattleClientRpc(pos);

        }

        if (CharacterManager.instance.PlayerB != null)
        {
            Vector2Int pos = new Vector2Int(5, 1);
            _gridManager.MoveCharacterToTile(CharacterManager.instance.PlayerB, pos);
            if (GameManager.instance.IsOnline())
                CharacterManager.instance.PlayerB.CharacterSyncBattleClientRpc(pos);

            CanvasManager.instance.InitCharacterUI(CharacterManager.instance.PlayerB);
        }
    }

    private IEnumerator PreBattleRoutine()
    {
        yield return new WaitForSeconds(BeatManager.instance.BeatDuration);

        BeatManager.instance.StartPreviewBeat();

        ChangeBattleState(BattleState.Countdown);

        yield return StartCoroutine(CountdownRoutine());

        StartBattle();
    }

    private IEnumerator CountdownRoutine()
    {
        int count = _countDownTimer;

        if (GameManager.instance.IsOnline())
        {
            float timerStart = BeatManager.instance.BeatDuration * (_countDownTimer + 1);
            BeatSyncNetwork.instance.ScheduleStartTimeServerRpc(timerStart);
        }

        while (count > 0)
        {
            CanvasManager.instance.ShowCountdown(count.ToString());
            yield return new WaitForSeconds(BeatManager.instance.BeatDuration);
            count--;
        }

        CanvasManager.instance.ShowCountdown("GO");
        yield return new WaitForSeconds(BeatManager.instance.BeatDuration);

        CanvasManager.instance.HideCountdown();
    }

    public IEnumerator CountdownRoutineClient(int count, float beatDuration)
    {
        Debug.Log($"CountdownRoutineClient: count={count}, beatDuration={beatDuration}");
        BeatManager.instance.StartPreviewBeat();

        while (count > 0)
        {
            CanvasManager.instance.ShowCountdown(count.ToString());
            yield return new WaitForSeconds(beatDuration);
            count--;
        }

        CanvasManager.instance.ShowCountdown("GO");
        yield return new WaitForSeconds(beatDuration);

        CanvasManager.instance.HideCountdown();
    }

    private void StartBattle()
    {
        ChangeBattleState(BattleState.Playing);

        GameManager.instance.StartBeat();

        CharacterManager.instance.EnableDisableCharactersControllers(true);
    }

    public void ChangeBattleState(BattleState newState)
    {
        _battleState = newState;
    }

    public void GenerateBattlefield()
    {
        InstantiatePlayers();
        SpawnTilesAndCharacters();

        ChangeBattleState(BattleState.Tunning);

        CharacterManager.instance.EnableDisableCharactersControllers(false);

        StartCoroutine(PreBattleRoutine());
    }

    public void FinishBattle()
    {
        StopAllCoroutines();
    }
}
