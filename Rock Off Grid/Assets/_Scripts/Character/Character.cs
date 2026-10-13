using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TextCore.Text;

public class Character : NetworkBehaviour
{
    [SerializeField] protected MovementComponent _movementComponent;
    [SerializeField] protected HealthComponent _healthComponent;
    [SerializeField] protected ManaComponent _manaComponent;
    [SerializeField] protected CharacterScore _characterScore;

    [SerializeField] protected SkillComponent _defendSkillComponent;
    [SerializeField] protected SkillComponent _meleeSkillComponent;
    [SerializeField] protected SkillComponent _rangedSkillComponent;
    [SerializeField] protected SkillComponent _specialSkillComponent;

    [SerializeField] protected PlayerType _type;
    [SerializeField] protected Tile _curTile;

    protected CharacterUIController _characterUIController;

    protected bool _isInvincible;

    public HealthComponent Health => _healthComponent;
    public ManaComponent Mana => _manaComponent;
    public CharacterScore Score => _characterScore;
    public PlayerType Type => _type;
    public Tile CurTile => _curTile;
    public CharacterUIController CharacterUI => _characterUIController;

    public virtual void Init(PlayerType type)
    {
        _type = type;
        _isInvincible = false;

        _healthComponent.Init();
        _manaComponent.Init();
        _characterScore.Init();
        _defendSkillComponent.StartSkillOnCooldown();
        _specialSkillComponent.StartSkillOnCooldown();
        BeatManager.instance.RegisterInInterval(1, OnBeat);
    }

    protected virtual void OnDisable()
    {
        BeatManager.instance.UnRegisterInInterval(1, OnBeat);
    }

    protected virtual void OnBeat()
    {
        BeatTick();
    }

    protected virtual void EnableControllers() { }

    protected virtual void DisableControllers() { }

    protected virtual void BeatTick()
    {
        _manaComponent.TickMana(_characterScore.CurMultiplier);
        _characterUIController.UpdateMana();
    }

    protected virtual void UseDefendSkill(TimingAccuracyType accuracy)
    {
        _defendSkillComponent.UseSkill(accuracy);
    }
    protected virtual void UseDefendSkillClient(TimingAccuracyType accuracy)
    {
        _defendSkillComponent.UseSkillClient(accuracy);
    }
    protected virtual void UseMeleeSkill(TimingAccuracyType accuracy)
    {
        _meleeSkillComponent.UseSkill(accuracy);
    }
    protected virtual void UseRangeSkill(TimingAccuracyType accuracy)
    {
        _rangedSkillComponent.UseSkill(accuracy);
    }
    protected virtual void UseSpecialSkill(TimingAccuracyType accuracy)
    {
        _specialSkillComponent.UseSkill(accuracy);
    }

    protected virtual void UseSpecialSkillClient(TimingAccuracyType accuracy)
    {
        _specialSkillComponent.UseSkillClient(accuracy);
    }

    public void SetInvincible(bool value)
    {
        _isInvincible = value;
    }

    public virtual void SetUIPanel(CharacterUIController ui)
    {
        _characterUIController = ui;
        if (!IsServer) return;
        _defendSkillComponent.RegisterActionCooldown(UpdateShieldIndicator);
        _specialSkillComponent.RegisterActionCooldown(UpdateSpecialIndicator);
    }

    public virtual void MoveToTile(Tile tile)
    {
        _curTile = tile;
        transform.position = _curTile.transform.position;
    }


    public virtual void Hit(float value)
    {
        if (_isInvincible)
        {
            DefendSkill defense = (DefendSkill)_defendSkillComponent;
            if (defense == null) return;
            defense.ParryPerfect();
            return;
        }

        _healthComponent.Hit(value);
        UpdateHealth();

        if (_healthComponent.CurHealth <= 0)
            CharacterDied();
    }

    public void OnActionFeedback(TimingAccuracyType accuracy)
    {
        _characterUIController.ShowHitFeedback(accuracy);
    }

    public void UpdateHealth()
    {
        _characterUIController.UpdateHealth();
        if (GameManager.instance.IsOnline() && IsServer) UpdateHealthClientRpc(_healthComponent.CurHealth);
    }

    public void UpdateHealth(float value)
    {
        _characterUIController.UpdateHealth(value);
    }

    public void UpdateDamageHealth(float value)
    {
        _characterUIController.UpdateDamageHealth(value);
        if (GameManager.instance.IsOnline() && IsServer) UpdateDamageHealthClientRpc(value);
    }

    public virtual void UpdateScore()
    {
        _characterUIController.UpdateScore();
        if (GameManager.instance.IsOnline() && IsServer) UpdateScoreClientRpc(_characterScore.CurMultiplier, _characterScore.CurScore, _characterScore.Combo);
    }

    public virtual void UpdateScore(float multi, int score, int combo)
    {
        _characterUIController.UpdateScore(multi, score, combo);
        _characterScore.UpdateSongIntensity();
    }

    public void UpdateShieldIndicator(bool value)
    {
        _characterUIController.UpdateShieldIndicator(value);
        if (GameManager.instance.IsOnline() && IsServer) UpdateShieldIndicatorClientRpc(value);
    }

    public void UpdateSpecialIndicator(bool value)
    {
        _characterUIController.UpdateSpecialIndicator(value);
        if (GameManager.instance.IsOnline() && IsServer) UpdateSpecialIndicatorClientRpc(value);
    }

    public virtual void CharacterDied()
    {
        _healthComponent.HealthDamageStop();
        GameManager.instance.CharacterDied(_type);

        if (GameManager.instance.IsOnline() && IsServer) CharacterDiedClientRpc();
    }

    public void ClearCharacterRef()
    {
        _curTile = null;
    }

    public void EndBattle()
    {
        BeatManager.instance.UnRegisterInInterval(1, OnBeat);
        DisableControllers();
        ClearCharacterRef();
    }

    public void EnableDisableControllers(bool enable)
    {
        if (enable)
            EnableControllers();
        else
            DisableControllers();
    }

    public void EnableDisableControllersServer()
    {
        if (!GameManager.instance.IsOnline() || !IsOwner) return;

        EnableControllersClientRpc();
    }

    public virtual void CleanCharacter()  {}

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.instance.IsOnline() && !IsServer) return;

        if (collision.TryGetComponent<SkillObject>(out var skill))
        {
            if (skill.Type == _type) return;
            Hit(skill.Value);
            skill.ObjectCollided();
            if (GameManager.instance.IsOnline() && IsServer) skill.ObjectCollidedClientRpc();
        }
    }

    [ServerRpc]
    public virtual void MoveToTileServerRpc(Vector2Int pos)
    {
        GridManager.instance.MoveCharacterToTile(this, pos);
    }

    [ServerRpc]
    public virtual void EnableControllersServerRpc()
    {
        EnableControllersClientRpc();
    }

    [ServerRpc]
    public virtual void DisableControllersServerRpc()
    {
        DisableControllersClientRpc();
    }

    [ClientRpc]
    public virtual void EnableControllersClientRpc() { }

    [ClientRpc]
    public virtual void DisableControllersClientRpc() { }

    [ClientRpc]
    public virtual void InitClientRpc(PlayerType type)
    {
        Init(type);
        CharacterManager.instance.InitCharacter(this);
    }

    [ClientRpc]
    public virtual void CharacterSyncBattleClientRpc(Vector2Int pos)
    {
        GridManager.instance.MoveCharacterToTile(this, pos);
        CanvasManager.instance.InitCharacterUI(this);
    }

    [ClientRpc]
    public virtual void GenerateTilesClientRpc()
    {
        GridManager.instance.GenerateGrid();
    }


    /*Update Clients UI Information*/
    [ClientRpc]
    public void UpdateHealthClientRpc(float value)
    {
        if (IsServer) return;
        UpdateHealth(value);
    }

    [ClientRpc]
    public void UpdateDamageHealthClientRpc(float value)
    {
        if (IsServer) return;
        UpdateDamageHealth(value);
    }

    [ClientRpc]
    public void UpdateScoreClientRpc(float multi, int score, int combo)
    {
        if (IsServer) return;
        UpdateScore(multi, score, combo);
    }

    [ClientRpc]
    public void UpdateShieldIndicatorClientRpc(bool value)
    {
        if (IsServer) return;
        UpdateShieldIndicator(value);
    }

    [ClientRpc]
    public void UpdateSpecialIndicatorClientRpc(bool value)
    {
        if (IsServer) return;
        UpdateSpecialIndicator(value);
    }

    [ClientRpc]
    public void CharacterDiedClientRpc()
    {
        if(IsServer) return;
        CharacterDied();
    }
}
