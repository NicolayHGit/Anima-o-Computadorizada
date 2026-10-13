using UnityEngine;
using Unity.Netcode;

public class BeatSyncNetwork : NetworkBehaviour
{
    public static BeatSyncNetwork instance;

    private void Awake()
    {
        if (instance == null) 
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    [ServerRpc]
    public void ScheduleStartTimeServerRpc(float timer)
    {
        if (!IsServer) return;
        double startTime = NetworkManager.Singleton.ServerTime.Time + timer;
        ScheduleStartBeatClientRpc(startTime);
        StartCoroutine(BeatManager.instance.ScheduleStart(startTime));
        SyncCountdownTimerClientRpc(BattleManager.instance.CountdownTimer, BeatManager.instance.BeatDuration);
    }

    [ServerRpc]
    public void FinishBattleServerRpc()
    {
        if (!IsServer) return;
        FinishBattleClientRpc();
        LobbyManager.instance.FinishBattle();
    }

    /*[ClientRpc]
    public void SyncBeatClientRpc(float hostTime)
    {
        if (IsServer) return;
        BeatManager.instance.SyncBeatClient(hostTime);
    }*/

    [ClientRpc]
    public void ScheduleStartBeatClientRpc(double networkStartTime)
    {
        if (IsServer) return;
        Debug.Log("Recebendo tempo de início da música: " + networkStartTime);
        StartCoroutine(BeatManager.instance.ScheduleStart(networkStartTime));
    }

    [ClientRpc]
    public void SyncCountdownTimerClientRpc(int count, float beatDuration)
    {
        if (IsServer) return;
        StartCoroutine(BattleManager.instance.CountdownRoutineClient(count, beatDuration));
    }

    [ClientRpc]
    public void FinishBattleClientRpc()
    {
        if (IsServer) return;
        LobbyManager.instance.FinishBattle();
    }
}
