using Unity.Netcode;
using UnityEngine;

public class ManaComponent : NetworkBehaviour
{
    [SerializeField] protected Character _character;
    [SerializeField] protected float _mana;
    [SerializeField] protected float _manaPerTick;

    protected float _curMana;
    private NetworkVariable<float> _netMana = new NetworkVariable<float>();

    public float MaxMana => _mana;
    public float CurMana => GameManager.instance.IsOnline() ? _netMana.Value : _curMana;

    public void Init()
    {
        _curMana = 0;

    }

    public void NetworkInit()
    {
        _netMana.Value = 0;
    }

    public void TickMana(float scoreIncrease)
    {
        if (GameManager.instance.IsOnline() && !NetworkManager.Singleton.IsServer) return;

        if (NetworkManager.Singleton.IsServer)
        {
            _netMana.Value += _manaPerTick * scoreIncrease;
            if (_netMana.Value > _mana) _netMana.Value = _mana;
        }
        else
        {
            _curMana += _manaPerTick * scoreIncrease;
            if (_curMana > _mana) _curMana = _mana;
        }

            
    }

    public void AddMana(float value)
    {
        if (GameManager.instance.IsOnline() && !NetworkManager.Singleton.IsServer) return;

        if (NetworkManager.Singleton.IsServer)
        {
            _netMana.Value += value;
            if (_netMana.Value > _mana) _netMana.Value = _mana;
        }
        else
        {
            _curMana += value;
            if (_curMana > _mana) _curMana = _mana;
        }
            
    }

    public bool UseMana(float manaCost)
    {
        if (GameManager.instance.IsOnline() && !NetworkManager.Singleton.IsServer) return false;

        if (manaCost > CurMana) return false;

        if (NetworkManager.Singleton.IsServer)
            _netMana.Value -= manaCost;
        else
            _curMana -= manaCost;

        return true;
    }
}
