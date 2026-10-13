using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager instance;


    [SerializeField] private CharacterUIController _characterUiControllerA;
    [SerializeField] private CharacterUIController _characterUiControllerB;

    [SerializeField] private TextMeshProUGUI _countdownText;

    [SerializeField] private Button _returnBtn;

    [SerializeField] private EndBattleUI _endGamePanel;
    

    private void Awake()
    {
        instance = this;
        _returnBtn.onClick.AddListener(ReturnClick);
    }

    private void OnDisable()
    {
        _returnBtn.onClick.RemoveListener(ReturnClick);
    }

    private void ReturnClick()
    {
        GameManager.instance.ReturnFromBattle();
    }

    public void InitCharacterUI(Character character)
    {
        if(character.Type == PlayerType.PLAYERA)
            _characterUiControllerA.Init(character);
        else
            _characterUiControllerB.Init(character);
    }

    public void EndGame(PlayerType playerWin, CharacterScore csA, CharacterScore csB)
    {
        _endGamePanel.SetupEndgamePanel(playerWin, csA, csB);
        _endGamePanel.gameObject.SetActive(true);
    }

    public void ShowCountdown(string text)
    {
        _countdownText.gameObject.SetActive(true);
        _countdownText.text = text;
    }

    public void HideCountdown()
    {
        _countdownText.gameObject.SetActive(false);
    }
}
