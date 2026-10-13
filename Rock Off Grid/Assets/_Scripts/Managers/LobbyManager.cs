using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

    [SerializeField] private float _heartBeatTimer = 15;
    [SerializeField] private UnityServicesManager _services;

    private Lobby _currentLobby;

    private int _clientsLoaded = 0;
    private int _totalClients = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        /*if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadEventCompleted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }*/
    }

    private void OnDestroy()
    {
        try 
        {
            StopAllCoroutines();
        } 
        catch (System.Exception e)
        {
            Debug.LogError($"Erro desconectar lobby: {e}");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Apenas o Host decide quando iniciar
        if (!NetworkManager.Singleton.IsServer) return;

        _clientsLoaded = 1;
        _totalClients = NetworkManager.Singleton.ConnectedClients.Count;

        // Checa se já temos 2 jogadores
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadEventCompleted;

            StartBattle();
        }
    }

    private void OnLoadEventCompleted(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // unsubscribe do evento para não disparar várias vezes
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadEventCompleted;

        _clientsLoaded++;

        // Agora que TODOS os players carregaram, chame a lógica de spawn no servidor
        if (_clientsLoaded >= _totalClients)
        {
            // Chamamos GameManager/CharacterManager para spawnar via réseau
            if(BattleManager.instance != null)
                BattleManager.instance.GenerateBattlefield(); // ajuste para chamar método que faz spawn online corretamente
        }
    }

    private void SetHostRelayAllocationData(Allocation a)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(
            a.RelayServer.IpV4,
            (ushort)a.RelayServer.Port,
            a.AllocationIdBytes,
            a.Key,
            a.ConnectionData);
    }

    private void SetClientRelayAllocationData(JoinAllocation a)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(
            a.RelayServer.IpV4,
            (ushort)a.RelayServer.Port,
            a.AllocationIdBytes,
            a.Key,
            a.ConnectionData,
            a.HostConnectionData);
    }

    private static IEnumerator HeartBeatLobbyCoroutine(string lobbyId, float delaySeconds)
    {
        var wait = new WaitForSeconds(delaySeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return wait;
        }
    }

    private async Task JoinRelay(string joinCode)
    {
        try
        {
            NetworkManager.Singleton.Shutdown();
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            SetClientRelayAllocationData(allocation);

            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao conectar via Relay: {e}");
        }
    }

    private void StartBattle()
    {
        GameManager.instance.SetGameMode(GameMode.MultiPlayer);
    }

    public void ConnectServerToWaitingClient()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public async Task<Lobby> CreateLobby(string lobbyName = "lobby", int maxPlayers = 2)
    {
        await _services.InitServices();

        try
        {
            // Cria relay allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Cria lobby com joinCode salvo
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            StartCoroutine(HeartBeatLobbyCoroutine(lobby.Id, _heartBeatTimer));

            SetHostRelayAllocationData(allocation);

            // Inicia como host
            if (NetworkManager.Singleton.StartHost())
                ConnectServerToWaitingClient();
            else
                Debug.LogError("Falha ao iniciar Host!");


            MainMenu.instance.UpdateJoinCode(joinCode);

            _currentLobby = lobby;

            return lobby;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao criar lobby/relay: {e}");
            return null;
        }
    }

    public async Task JoinByCode(string lobbyCode)
    {
        await _services.InitServices();

        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            string relayJoinCode = lobby.Data["joinCode"].Value;

            await JoinRelay(relayJoinCode);
            _currentLobby = lobby;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao entrar no lobby: {e}");
        }
    }

    public async Task JoinLobbyById(string lobbyId)
    {
        await _services.InitServices();

        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            string relayJoinCode = lobby.Data["joinCode"].Value;

            await JoinRelay(relayJoinCode);

            _currentLobby = lobby;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao entrar no lobby: {e}");
        }
    }

    public async Task QuickJoinLobby()
    {
        await _services.InitServices();

        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            if (_currentLobby != null && _currentLobby.Id == _currentLobby.Id)
            {
                Debug.Log("Ignorando próprio lobby");
                return;
            }

            string joinCode = lobby.Data["joinCode"].Value;
            await JoinRelay(joinCode);

            _currentLobby = lobby;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao entrar no lobby: {e}");
        }
    }

    public async Task<List<Lobby>> GetLobbies()
    {
        await _services.InitServices();

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(
                        QueryFilter.FieldOptions.AvailableSlots,
                        "0",
                        QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(
                        false,
                        QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);

            return response.Results;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao buscar lobbies: {e}");
            return null;
        }
    }

    public void FinishBattle()
    {
            if (NetworkManager.Singleton != null)
            {
                Debug.Log("Finalizando lobby e desconectando clientes...");
                StopAllCoroutines();
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadEventCompleted;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                if (NetworkManager.Singleton.IsServer)
                    LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                else
                    LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, NetworkManager.Singleton.LocalClientId.ToString());

                NetworkManager.Singleton.Shutdown();
            }

            _currentLobby = null;
    }
}
