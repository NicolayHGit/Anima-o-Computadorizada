using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterUIController : MonoBehaviour
{
    [SerializeField] private PlayerType _playerType;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _healthDamageSlider;
    [SerializeField] private Slider _manaSlider;
    [SerializeField] private TextMeshProUGUI _multiplierText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private TextMeshProUGUI _hitFeedbackText;
    [SerializeField] private Image _shieldIndicator;
    [SerializeField] private Image _specialIndicator;

    private Character _character;

    public void Init(Character character)
    {
        _character = character;
        _character.SetUIPanel(this);
        UpdateHealth();
        UpdateMana();
        UpdateScore();
    }

    public void ShowHitFeedback(TimingAccuracyType accuracy)
    {
        _hitFeedbackText.gameObject.SetActive(true);

        switch (accuracy)
        {
            case TimingAccuracyType.Perfect:
                _hitFeedbackText.text = "PERFECT!";
                _hitFeedbackText.color = Color.yellow;
                break;

            case TimingAccuracyType.Good:
                _hitFeedbackText.text = "GOOD";
                _hitFeedbackText.color = Color.green;
                break;

            case TimingAccuracyType.Normal:
                _hitFeedbackText.text = "OK";
                _hitFeedbackText.color = Color.white;
                break;

            case TimingAccuracyType.Miss:
                _hitFeedbackText.text = "MISS";
                _hitFeedbackText.color = Color.red;
                break;
        }

        CancelInvoke(nameof(HideFeedback));
        Invoke(nameof(HideFeedback), 0.3f);
    }

    private void HideFeedback()
    {
        _hitFeedbackText.gameObject.SetActive(false);
    }

    public void UpdateHealth()
    {
        float value = _character.Health.CurHealth / _character.Health.MaxHealth;
        _healthSlider.value = value;
    }

    public void UpdateHealth(float health)
    {
        float value = health / _character.Health.MaxHealth;
        _healthSlider.value = value;
    }

    public void UpdateDamageHealth(float value)
    {
        float damageHealth = value / _character.Health.MaxHealth;
        _healthDamageSlider.value = damageHealth;
        
    }

    public void UpdateMana()
    {
        float value = _character.Mana.CurMana / _character.Mana.MaxMana;
        _manaSlider.value = value;
    }

    public void UpdateScore()
    {
        _scoreText.text = _character.Score.CurScore.ToString();
        _comboText.text = $"{_character.Score.Combo}x";
        _multiplierText.text = $"{_character.Score.GetScoreText()}";
    }

    public void UpdateScore(float multiplier, int score, int combo)
    {
        _scoreText.text = score.ToString();
        _comboText.text = $"{combo}x";
        _multiplierText.text = $"{_character.Score.GetScoreText()}";
    }

    public void UpdateShieldIndicator(bool value)
    {
        _shieldIndicator.gameObject.SetActive(!value);
    }

    public void UpdateSpecialIndicator(bool value)
    {
        _specialIndicator.gameObject.SetActive(!value);
    }
}
