using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    [SerializeField] private PlayerInputManager _playerInputManager;
    [SerializeField] private Character _playerPrefab;
    [SerializeField] private Character _enemyPrefab;


    private Character _playerA;
    private Character _playerB;

    public Character PlayerA => _playerA;
    public Character PlayerB => _playerB;

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

    public void SetPlayerUI()
    {

    }

    public void InitializeSinglePlayer()
    {
        _playerA = Instantiate(_playerPrefab);
        _playerA.Init(PlayerType.PLAYERA);
        var playerInputA = _playerA.GetComponent<PlayerInput>();
        if (playerInputA != null)
        {
            var devices = new List<InputDevice>();
            devices.Add(Keyboard.current);
            devices.Add(Mouse.current);
            devices.AddRange(Gamepad.all);

            playerInputA.SwitchCurrentControlScheme("All", devices.ToArray());
        }

        _playerB = Instantiate(_enemyPrefab);
        _playerB.Init(PlayerType.PLAYERB);
    }

    public void InitializeMultiLocalPlayers()
    {
        _playerA = Instantiate(_playerPrefab);
        _playerA.Init(PlayerType.PLAYERA);
        var playerInputA = _playerA.GetComponent<PlayerInput>();
        if (playerInputA != null)
        {
            playerInputA.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
        }

        _playerB = Instantiate(_playerPrefab);
        _playerB.Init(PlayerType.PLAYERB);
        var playerInputB = _playerB.GetComponent<PlayerInput>();
        if (playerInputB != null)
            playerInputB.SwitchCurrentControlScheme("Gamepad", Gamepad.all[1]);
    }

    public bool playersInitialized = false;

    public void InitializeMultiPlayers()
    {
        CleanCharacters();

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        if (clients.Count < 2) return;

        int index = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = Instantiate(_playerPrefab);
            var netObj = player.GetComponent<NetworkObject>();

            // spawn as player object (give ownership to that client)
            try
            {
                netObj.SpawnAsPlayerObject(client.ClientId, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CharMan] Exception on SpawnAsPlayerObject for client {client.ClientId}: {ex}");
            }

            var type = (index == 0) ? PlayerType.PLAYERA : PlayerType.PLAYERB;
            player.Init(type);
            player.InitClientRpc(type);

            if (type == PlayerType.PLAYERA)
                _playerA = player;
            else
                _playerB = player;

            player.EnableControllersClientRpc();

            index++;
        }
    }

    public void InitCharacter(Character character)
    {
        if(character.Type == PlayerType.PLAYERA)
            _playerA = character;
        else if(character.Type == PlayerType.PLAYERB)
            _playerB = character;
    }

    public void BattleEnd()
    {
        if(_playerA != null) _playerA.EndBattle();
        if(_playerB != null) _playerB.EndBattle();
    }

    public void CleanCharacters()
    {
        if (_playerA != null) _playerA.CleanCharacter();
        if (_playerB != null) _playerB.CleanCharacter();

        if(_playerA != null && _playerA.gameObject != null) Destroy(_playerA.gameObject);
        if(_playerB != null && _playerB.gameObject != null) Destroy(_playerB.gameObject);

        _playerA = null;
        _playerB = null;
    }

    public void EnableDisableCharactersControllers(bool enable)
    {
        if (GameManager.instance.IsOnline())
        {
            if(enable)
            {
                _playerA.EnableControllersClientRpc();
                _playerB.EnableControllersClientRpc();
            }
            else
            {
                _playerA.DisableControllersClientRpc();
                _playerB.DisableControllersClientRpc();
            }
                return;
        }

        _playerA.EnableDisableControllers(enable);
        _playerB.EnableDisableControllers(enable);
    }
}
