using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class MeleeSkill : SkillComponent
{
    public override void UseSkill(TimingAccuracyType accuracy)
    {
        if (!IsSkillEnable()) return;
        StartCooldown();

        var skillEffect = GetFromPool();
        skillEffect.Init(this, _character.Type, accuracy, 0);

        int playerMultiplier = _character.Score.CurMultiplier;
        float damage = DamageCalculator.CalculateDamage(_value, playerMultiplier, accuracy);

        if(GameManager.instance.IsOnline() && !NetworkManager.Singleton.IsServer) return;
        skillEffect.CurTile.HitTargetInTile(damage);
    }
}
