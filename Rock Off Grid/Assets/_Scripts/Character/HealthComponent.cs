using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    [SerializeField] protected Character _character;
    [SerializeField] protected float _health;
    [SerializeField] protected float _timerHealthDamaged;

    private float _curHealth;
    private float _curDamageHealth;

    private NetworkVariable<float> _netHealth = new NetworkVariable<float>();
    private NetworkVariable<float> _netDamageHealth = new NetworkVariable<float>();

    public float MaxHealth => _health;
    public float CurHealth => GameManager.instance.IsOnline() ? _netHealth.Value : _curHealth;
    public float CurDamageHealth => GameManager.instance.IsOnline() ? _netDamageHealth.Value : _curDamageHealth;
    public void Init()
    {
        _curHealth = _health;
    }

    public void NetworkInit()
    {
        _netHealth.Value = _health;
        _netDamageHealth.Value = _health;
    }

    public virtual void Hit(float value)
    {
        if (GameManager.instance.IsOnline())
        {
            if (!NetworkManager.Singleton.IsServer) return;
            
            _netHealth.Value -= value;
            if (_netHealth.Value <= 0)
                _netHealth.Value = 0;
            else
                StartCoroutine(UpdateHealthDamage(_netHealth.Value));
        }
        else
        {
            _curHealth -= value;
            if (_curHealth <= 0)
                _curHealth = 0;
            else
                StartCoroutine(UpdateHealthDamage(_curHealth));
        }
    }

    public IEnumerator UpdateHealthDamage(float value)
    {
        float healthToUpdate = value;
        yield return new WaitForSeconds(_timerHealthDamaged);

        if (CurHealth > 0)
        {
            if (GameManager.instance.IsOnline() && NetworkManager.Singleton.IsServer)
            {
                _netDamageHealth.Value = healthToUpdate;
                _character.UpdateDamageHealth(_netDamageHealth.Value);
                _character.UpdateDamageHealthClientRpc(_netDamageHealth.Value);
            }
            else
            {
                _curDamageHealth = healthToUpdate;
                _character.UpdateDamageHealth(_curDamageHealth);
            }
        }
    }

    public void PerfectParry()
    {
        if (GameManager.instance.IsOnline() && !NetworkManager.Singleton.IsServer) return;

        HealthDamageStop();
        if (GameManager.instance.IsOnline() && NetworkManager.Singleton.IsServer)
            _netHealth.Value = _netDamageHealth.Value;
        else
            _curHealth = _curDamageHealth;
    }

    public void HealthDamageStop()
    {
        StopAllCoroutines();
    }
}
