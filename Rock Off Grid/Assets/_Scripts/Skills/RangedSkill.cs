using UnityEngine;

public class RangedSkill : SkillComponent
{
    [SerializeField] protected Vector2 offset;

    public override void UseSkill(TimingAccuracyType accuracy)
    {
        if (!IsSkillEnable()) return;
        StartCooldown();

        var skillEffect = GetFromPool();
        skillEffect.Init(this, _character.Type, accuracy, _value);
    }
}
