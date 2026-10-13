using TMPro;
using UnityEngine;

public class CharacterUIEndgame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _perfectScoreText;
    [SerializeField] private TextMeshProUGUI _goodScoreText;
    [SerializeField] private TextMeshProUGUI _normalScoreText;
    [SerializeField] private TextMeshProUGUI _missText;
    [SerializeField] private TextMeshProUGUI _maxComboText;

    private CharacterScore _characterScore;

    public void Score(CharacterScore cs)
    {
        _characterScore = cs;
        _scoreText.text = "Score: " + _characterScore.CurScore.ToString();
        _perfectScoreText.text = "Perfect: " + _characterScore.PerfectHits.ToString();
        _goodScoreText.text = "Good: " + _characterScore.GoodHits.ToString();
        _normalScoreText.text = "Normal: " + _characterScore.NormalHits.ToString();
        _missText.text = "Miss: " + _characterScore.Misses.ToString();
        _maxComboText.text = "Max Combo: " + _characterScore.MaxCombo.ToString();
    }
}
