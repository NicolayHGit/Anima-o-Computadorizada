using UnityEngine;

public class SlashSkill : SkillObject
{
    public override void Init(SkillComponent skill, PlayerType type, TimingAccuracyType accuracy, float value = 0)
    {
        base.Init(skill, type, accuracy, value);
        PlayAudioSkill();

        var tile = GridManager.instance.GetTileAtPosition(_skillComponent.Character.CurTile.Position + _direction);
        _curTile = tile;
        transform.position = _curTile.transform.position;
        StartCoroutine(DurationSkill());
    }
}
