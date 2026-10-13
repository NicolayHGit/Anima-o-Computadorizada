using TMPro;
using UnityEngine;

public class EndBattleUI : MonoBehaviour
{
    [SerializeField] private CharacterUIEndgame _characterUIA;
    [SerializeField] private CharacterUIEndgame _characterUIB;
    [SerializeField] private TextMeshProUGUI _endGameText;

    public void SetupEndgamePanel(PlayerType playerWin, CharacterScore csA, CharacterScore csB)
    {
        _characterUIA.Score(csA);
        _characterUIB.Score(csB);

        _endGameText.text = (playerWin == PlayerType.PLAYERB) ? "Player 1 Wins" : "Player 2 Wins";
    }
}
