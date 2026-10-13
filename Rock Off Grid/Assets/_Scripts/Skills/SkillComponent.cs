using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SkillComponent : MonoBehaviour
{
    [SerializeField] protected Character _character;
    [SerializeField] protected SkillObject _skillEffect;
    [SerializeField] protected float _value;
    [SerializeField] protected float _cooldown;
    [SerializeField] protected int _poolSize = 1;
    [SerializeField] protected float _manaCost = 0;

    protected float _curCooldownTimer;

    protected Queue<SkillObject> _pool = new Queue<SkillObject>();

    protected UnityEvent<bool> SkillCooldown;

    public Character Character => _character;
    public float Value => _value;
    public float CurentCooldown => _curCooldownTimer;

    protected void Awake()
    {
        if (SkillCooldown == null) SkillCooldown = new UnityEvent<bool>();
    }

    protected void Start()
    {
        InstantiatePool();
        _curCooldownTimer = 0;
    }

    private void OnDisable()
    {
        ClearPool();
        StopAllCoroutines();
    }

    protected void Update()
    {
        if (CurentCooldown.IsUnityNull() || CurentCooldown <= 0) return;
        _curCooldownTimer -= Time.deltaTime;
        if (CurentCooldown <= 0) SkillOnCooldown(false);
    }

    protected virtual void InstantiatePool()
    {
        _pool = new();
        for (int i = 0; i < _poolSize; i++)
        {
            SkillObject obj = Instantiate(_skillEffect, transform);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    protected virtual void StartCooldown()
    {
        _curCooldownTimer = _cooldown;
        SkillOnCooldown(true);
    }

    protected SkillObject GetFromPool()
    {
        SkillObject obj = _pool.Dequeue();
        obj.transform.SetParent(null);
        obj.gameObject.SetActive(true);
        return obj;
    }

    protected void ClearPool()
    {
        foreach (var obj in _pool)
        {
            if (obj != null)
            {
                obj.EndCicleObject();
                Destroy(obj.gameObject);
            }
        }

        _pool.Clear();
        _pool = null;
    }

    protected virtual void SkillOnCooldown(bool value)
    {
        SkillCooldown?.Invoke(value);
    }

    public void RegisterActionCooldown(UnityAction<bool> action)
    {
        SkillCooldown.AddListener(action);
    }

    public void UnRegisterActionCooldown(UnityAction<bool> action)
    {
        SkillCooldown.RemoveListener(action);
    }

    public virtual bool IsSkillEnable()
    {
        return CurentCooldown <= 0;
    }

    public virtual bool CanUse()
    {
        return IsSkillEnable() && (_character.Mana.CurMana > _manaCost);
    }

    public virtual bool IsNetSkill()
    {
        return (GameManager.instance.IsOnline() && NetworkManager.Singleton.IsServer);
    }

    public void StartSkillOnCooldown()
    {
        StartCooldown();
    }

    public void ReturnToPool(SkillObject obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        _pool.Enqueue(obj);
    }

    public virtual void UseSkill(TimingAccuracyType accuracy) { }

    public virtual void UseSkillClient(TimingAccuracyType accuracy) { }
}
