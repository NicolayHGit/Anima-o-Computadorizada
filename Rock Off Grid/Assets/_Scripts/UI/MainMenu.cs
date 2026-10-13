using NUnit.Framework.Constraints;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    [SerializeField] private GameObject _painelStart;
    [SerializeField] private LobbyMenu _painelServer;
    [SerializeField] private GameObject _panelControlersWarning;
    [SerializeField] private GameObject _settingPanel;
    [SerializeField] private GameObject _waitingPanel;
    [SerializeField] private Button _quitBtn;
    [SerializeField] private TMP_InputField _ipInput;
    [SerializeField] private TMP_InputField _codeInput;
    [SerializeField] private TextMeshProUGUI _codeText;

    private void Awake()
    {
        instance = this;
        _quitBtn.onClick.AddListener(QuitApplication);
    }

    private void OnDisable()
    {
        _quitBtn.onClick.RemoveListener(QuitApplication);
    }

    private void QuitApplication()
    {
        Application.Quit();
    }

    public void SetPanelServer(bool value)
    {
        _painelStart.SetActive(!value);
        if(value) _painelServer.EnablePanel();
    }

    public void StartSinglePlayer()
    {
        GameManager.instance.SetGameMode(GameMode.SinglePlayer);
    }

    public void StartMultiPlayer()
    {
        if(Gamepad.all.Count < 2)
        {
            _panelControlersWarning.SetActive(true);
            return;
        }

        GameManager.instance.SetGameMode(GameMode.MultiLocal);
    }

    public void UpdateJoinCode(string code)
    {
        _codeText.text = "Join Code: " + code;
    }

    public void StartHost(string code)
    {
        //UpdateJoinCode(code);
        _painelServer.gameObject.SetActive(false);
        _waitingPanel.gameObject.SetActive(true);
    }

    public async void StartClient()
    {
        string lobbyCode = _codeInput.text;
        if(lobbyCode == null || lobbyCode == "")
            await LobbyManager.instance.QuickJoinLobby();
        else
        {
            Debug.Log($"Code {lobbyCode}");
            await LobbyManager.instance.JoinByCode(lobbyCode);
        }
    }

}
