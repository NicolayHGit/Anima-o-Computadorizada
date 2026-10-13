using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] MainMenu _mainMenu;

    [SerializeField] private LobbyRoom _lobbyRoomPrefab;
    [SerializeField] private RectTransform _lobbyRoomContainer;

    [SerializeField] private Button _quickJoinBtn;
    [SerializeField] private Button _joinCodeBtn;
    [SerializeField] private Button _createRoomBtn;

    [SerializeField] private TMP_InputField _joinCodeInput;
    [SerializeField] private TMP_InputField _createRoomInput;

    public async void RefreshRooms()
    {
        foreach (Transform child in _lobbyRoomContainer)
            Destroy(child.gameObject);

        List<Lobby> lobbies = await LobbyManager.instance.GetLobbies();

        if (lobbies == null) return;

        foreach (Lobby lobbyData in lobbies)
        {
            var room = Instantiate(_lobbyRoomPrefab, _lobbyRoomContainer);
            room.Init(lobbyData);
        }
    }

    public void UpdateJoinCode(string code)
    {
        _mainMenu.UpdateJoinCode(code);
    }

    public async void StartHost()
    {
        string lobbyName = _createRoomInput.text;
        if (lobbyName == "") lobbyName = null;
        Lobby looby = await LobbyManager.instance.CreateLobby(lobbyName);
        _mainMenu.StartHost(looby.Id);
    }

    public async void JoinCode()
    {
        string lobbyCode = _joinCodeInput.text;

        await LobbyManager.instance.JoinByCode(lobbyCode);
    }

    public async void QuickJoin()
    {
        await LobbyManager.instance.QuickJoinLobby();
    }

    public void EnablePanel()
    {
        gameObject.SetActive(true);
        RefreshRooms();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
        _mainMenu.SetPanelServer(false);
    }
}
