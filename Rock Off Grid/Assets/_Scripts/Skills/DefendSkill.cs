using System.Collections;
using UnityEngine;

public class DefendSkill : SkillComponent
{
    [SerializeField] protected float _bonusDuration = 0.25f;      // bônus por Perfect
    [SerializeField] protected float _manaOnPerfect = 10f;

    protected bool _perfectExecution = false;
    protected Coroutine _activeDefense;
    protected SkillObject _activeShield;

    public bool PerfectExecution => _perfectExecution;

    private void ActivateShield(TimingAccuracyType accuracy)
    {
        StartCooldown();


        float immuneDuration = _value;
        if (accuracy == TimingAccuracyType.Perfect)
        {
            immuneDuration += _bonusDuration;
            _perfectExecution = true;
        }

        if (_activeDefense != null)
            StopCoroutine(_activeDefense);

        _activeDefense = StartCoroutine(DoDefense(immuneDuration, accuracy));
    }

    public override void UseSkill(TimingAccuracyType accuracy)
    {
        if (!CanUse()) return;
        if (!_character.Mana.UseMana(_manaCost)) return;

        ActivateShield(accuracy);
    }

    public override void UseSkillClient(TimingAccuracyType accuracy)
    {
        ActivateShield(accuracy);
    }

    private IEnumerator DoDefense(float duration, TimingAccuracyType accuracy)
    {
        _character.SetInvincible(true);

        _activeShield = GetFromPool();
        _activeShield.transform.position = _character.transform.position;
        _activeShield.transform.SetParent(_character.transform);

        yield return new WaitForSeconds(duration);

        EndShield();
    }

    private void EndShield()
    {
        if (_activeDefense != null)
            StopCoroutine(_activeDefense);

        _perfectExecution = false;
        _character.SetInvincible(false);

        if (_activeShield != null) ReturnToPool(_activeShield);

        _activeDefense = null;
    }

    public void ParryPerfect()
    {
        _character.Mana.AddMana(_manaOnPerfect);
        _character.Score.AddDefenseBonus(TimingAccuracyType.Perfect);
        _character.Health.PerfectParry();
        _character.CharacterUI.UpdateHealth();
        EndShield();
    }
}
