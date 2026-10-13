using FMODUnity;
using FMOD.Studio;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SkillObject : MonoBehaviour
{
    [SerializeField] protected float _duration;
    [SerializeField] private EventReference _skillSFX;

    protected SkillComponent _skillComponent;
    protected PlayerType _type;

    protected float _value;
    protected float _multiplierDamage;

    protected Tile _curTile;
    protected Vector2Int _direction;


    public float Value => _value;
    public float MultiplierDamage => _multiplierDamage;
    public PlayerType Type => _type;
    public Tile CurTile => _curTile;

    protected virtual IEnumerator DurationSkill()
    {
        yield return new WaitForSeconds(_duration);
        Recycle();
    }

    protected virtual void PlayAudioSkill()
    {
        var instance = RuntimeManager.CreateInstance(_skillSFX);

        float randomPitch = Random.Range(0.9f, 1.1f);
        instance.setParameterByName("Pitch", randomPitch);

        instance.start();
        instance.release();
    }

    protected virtual void Recycle()
    {
        if (_skillComponent != null)
        {
            StopAllCoroutines();
            _skillComponent.ReturnToPool(this);
        }
        else
            Destroy(gameObject);
    }

    public virtual void Init(SkillComponent skill, PlayerType type, TimingAccuracyType accuracy, float value = 0)
    {
        _skillComponent = skill;
        _type = type;
        _value = value;

        int playerMultiplier = skill.Character.Score.CurMultiplier;
        _multiplierDamage = DamageCalculator.CalculateDamage(value, playerMultiplier, accuracy);

        _direction = (_type == PlayerType.PLAYERA) ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
    }

    public virtual void Init(SkillComponent skill, PlayerType type, TimingAccuracyType accuracy, float value = 0, Vector2Int? offset = null)
    {
        Init(skill, type, accuracy, value);
    }

    public virtual void ObjectCollided()
    {
        Recycle();
    }

    public void EndCicleObject()
    {
        StopAllCoroutines();
    }

    [ClientRpc]
    public virtual void ObjectCollidedClientRpc()
    {
        if (NetworkManager.Singleton.IsServer) return;
        Recycle();
    }
}
