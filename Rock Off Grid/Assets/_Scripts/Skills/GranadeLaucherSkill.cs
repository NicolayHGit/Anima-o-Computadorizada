using UnityEngine;

public class GranadeLaucherSkill : SkillComponent
{
    [SerializeField] protected Vector2Int _tagetTile;

    private void LauchGranade(TimingAccuracyType accuracy)
    {
        StartCooldown();

        int offset = (_character.Type == PlayerType.PLAYERA) ? 1 : -1;
        Vector2Int tagetTile = new Vector2Int(_tagetTile.x * offset, _tagetTile.y);

        var skillEffect = GetFromPool();
        skillEffect.Init(this, _character.Type, accuracy, _value, tagetTile);
    }

    public override void UseSkill(TimingAccuracyType accuracy)
    {
        if (!CanUse()) return;
        if (!_character.Mana.UseMana(_manaCost)) return;

        LauchGranade(accuracy);
    }

    public override void UseSkillClient(TimingAccuracyType accuracy)
    {
        LauchGranade(accuracy);
    }
}
