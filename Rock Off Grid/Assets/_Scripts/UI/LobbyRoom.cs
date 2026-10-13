using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoom : MonoBehaviour
{
    [SerializeField] private Button _roomBtn;
    [SerializeField] private TextMeshProUGUI _roomName;

    private Lobby _lobby;

    public void Init(Lobby lobby)
    {
        _lobby = lobby;
        _roomName.text = lobby.Name;

        _roomBtn.onClick.AddListener(RoomSelected);
    }

    public async void RoomSelected()
    {
        await LobbyManager.instance.JoinLobbyById(_lobby.Id);
    }
}
