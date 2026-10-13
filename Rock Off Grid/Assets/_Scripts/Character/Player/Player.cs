using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class Player : Character
{
    [SerializeField] private PlayerInput _playerInput;

    private InputAction _move;
    private InputAction _defend;
    private InputAction _meleeAttack;
    private InputAction _rangedAttack;
    private InputAction _specialAttack;



    private void OnEnable()
    {
        if (GameManager.instance.IsOnline() && !IsOwner) return;
        InitControllers();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) NetWorkInit();

        if (!IsOwner)
        {
            _playerInput.enabled = false;
            return;
        }
        InitControllers();
    }

    private void NetWorkInit()
    {
        if (!NetworkManager.IsServer) return;
        _healthComponent.NetworkInit();
        _manaComponent.NetworkInit();
        _characterScore.NetworkInit();
    }

    protected override void OnDisable() 
    {
        base.OnDisable();
    }

    private void InitControllers()
    {
        var actions = _playerInput.actions;
        _move = actions.FindAction("Move", true);
        _defend = actions.FindAction("Defend", true);
        _meleeAttack = actions.FindAction("MeleeAttack", true);
        _rangedAttack = actions.FindAction("RangedAttack", true);
        _specialAttack = actions.FindAction("SpecialAttack", true);

        if (_move != null) _move.performed += Movement;
        if (_defend != null) _defend.performed += DefendCommand;
        if (_meleeAttack != null) _meleeAttack.performed += MeleeCommand;
        if (_rangedAttack != null) _rangedAttack.performed += RangedCommand;
        if (_specialAttack != null) _specialAttack.performed += SpecialCommand;

        DisableControllers();
    }

    protected override void EnableControllers()
    {
        if (GameManager.instance.IsOnline() && !IsOwner) return;
        if (_move != null) _move.Enable();
        if (_defend != null) _defend.Enable();
        if (_meleeAttack != null) _meleeAttack.Enable();
        if (_rangedAttack != null) _rangedAttack.Enable();
        if (_specialAttack != null) _specialAttack.Enable();
    }

    protected override void DisableControllers()
    {
        if (GameManager.instance.IsOnline() && !IsOwner) return;
        if (_move != null) _move.Disable();
        if (_defend != null) _defend.Disable();
        if (_meleeAttack != null) _meleeAttack.Disable();
        if (_rangedAttack != null) _rangedAttack.Disable();
        if (_specialAttack != null) _specialAttack.Disable();
    }

    private void ActionBeatTimming()
    {
        TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracy();
        _characterScore.PerformActionBeat(accuracy);
    }

    private void Movement(InputAction.CallbackContext context)
    {
        if(GameManager.instance.IsOnline())
            if (!IsSpawned || (!IsOwner)) return;
        Vector2 moveDirection = context.action.ReadValue<Vector2>();
        Vector2Int moveDirectionInt = new Vector2Int((int)moveDirection.x, (int)moveDirection.y);

        if (GameManager.instance.IsOnline() && IsOwner)
        {
            double clientTime = NetworkManager.Singleton.LocalTime.Time;
            MoveCommandServerRpc(moveDirectionInt, clientTime);
        }
        else
        {
            TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracy();

            _movementComponent.MoveCommand(moveDirectionInt);
            _characterScore.PerformActionBeat(accuracy);
            OnActionFeedback(accuracy);
        }

    }

    private void DefendCommand(InputAction.CallbackContext context)
    {
        if (GameManager.instance.IsOnline())
            if (!IsSpawned || (!IsOwner)) return;
        if (GameManager.instance.IsOnline() && IsOwner)
        {
            double clientTime = NetworkManager.Singleton.LocalTime.Time;
            DefendCommandServerRpc(clientTime);
        }
        else
        {
            TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracy();

            UseDefendSkill(accuracy);
            _characterScore.PerformActionBeat(accuracy);
            OnActionFeedback(accuracy);
        }
    }

    private void MeleeCommand(InputAction.CallbackContext context)
    {
        if (GameManager.instance.IsOnline())
            if (!IsSpawned || (!IsOwner)) return;
        if (GameManager.instance.IsOnline() && IsOwner)
        {
            double clientTime = NetworkManager.Singleton.LocalTime.Time;
            MeleeCommandServerRpc(clientTime);
        }
        else
        {
            TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracy();

            UseMeleeSkill(accuracy);
            _characterScore.PerformActionBeat(accuracy);
            OnActionFeedback(accuracy);
        }

    }

    private void RangedCommand(InputAction.CallbackContext context)
    {
        if (GameManager.instance.IsOnline())
            if (!IsSpawned || (!IsOwner)) return;
        if (GameManager.instance.IsOnline() && IsOwner)
        {
            double clientTime = NetworkManager.Singleton.LocalTime.Time;
            RangedCommandServerRpc(clientTime);
        }
        else
        {
            TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracy();

            UseRangeSkill(accuracy);
            _characterScore.PerformActionBeat(accuracy);
            OnActionFeedback(accuracy);
        }
        
    }

    private void SpecialCommand(InputAction.CallbackContext context)
    {
        if (GameManager.instance.IsOnline())
            if (!IsSpawned || (!IsOwner)) return;
        if (GameManager.instance.IsOnline() && IsOwner)
        {
            double clientTime = NetworkManager.Singleton.LocalTime.Time;
            SpecialCommandServerRpc(clientTime);
        }
        else
        {
            TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracy();

            UseSpecialSkill(accuracy);
            _characterScore.PerformActionBeat(accuracy);
            OnActionFeedback(accuracy);
        }
    }

    public override void CleanCharacter()
    {
        if (_move != null)
        {
            _move.performed -= Movement;
            _move.Disable();
            _move = null;
        }
        if (_defend != null)
        {
            _defend.performed -= DefendCommand;
            _defend.Disable();
            _defend = null;
        }
        if (_meleeAttack != null)
        {
            _meleeAttack.performed -= MeleeCommand;
            _meleeAttack.Disable();
            _meleeAttack = null;
        }
        if (_rangedAttack != null)
        {
            _rangedAttack.performed -= RangedCommand;
            _rangedAttack.Disable();
            _rangedAttack = null;
        }
        if (_specialAttack != null)
        {
            _specialAttack.performed -= SpecialCommand;
            _specialAttack.Disable();
            _specialAttack = null;
        }
    }

    [ServerRpc]
    private void MoveCommandServerRpc(Vector2Int moveDir, double clientTimer)
    {
        if (!IsServer) return;
        // Executa lógica só no server
        TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracyServer(clientTimer);

        _movementComponent.MoveCommand(moveDir);
        _characterScore.PerformActionBeat(accuracy);
        OnActionFeedback(accuracy);

        // Notifica todos os clientes para replicar o movimento
        MoveCommandClientRpc(moveDir, accuracy);
    }

    [ServerRpc]
    private void DefendCommandServerRpc(double clientTimer)
    {
        if (!IsServer) return;
        if (!_defendSkillComponent.CanUse()) return;

        TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracyServer(clientTimer);

        UseDefendSkill(accuracy);
        _characterScore.PerformActionBeat(accuracy);
        OnActionFeedback(accuracy);

        DefendCommandClientRpc(accuracy);
    }

    [ServerRpc]
    private void MeleeCommandServerRpc(double clientTimer)
    {
        if (!IsServer) return;
        if (!_meleeSkillComponent.CanUse()) return;

        TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracyServer(clientTimer);

        UseMeleeSkill(accuracy);
        _characterScore.PerformActionBeat(accuracy);
        OnActionFeedback(accuracy);

        MeleeCommandClientRpc(accuracy);
    }

    [ServerRpc]
    private void RangedCommandServerRpc(double clientTimer)
    {
        if (!IsServer) return;
        if (!_rangedSkillComponent.CanUse()) return;

        TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracyServer(clientTimer);

        UseRangeSkill(accuracy);
        _characterScore.PerformActionBeat(accuracy);
        OnActionFeedback(accuracy);

        RangedCommandClientRpc(accuracy);
    }

    [ServerRpc]
    private void SpecialCommandServerRpc(double clientTimer)
    {
        if (!IsServer) return;
        if (!_specialSkillComponent.CanUse()) return;

        TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracyServer(clientTimer);

        UseSpecialSkill(accuracy);
        _characterScore.PerformActionBeat(accuracy);
        OnActionFeedback(accuracy);

        SpecialCommandClientRpc(accuracy);
    }

    [ClientRpc]
    public override void EnableControllersClientRpc()
    {
        if (!IsOwner) return;
        EnableControllers();
    }

    [ClientRpc]
    public override void DisableControllersClientRpc()
    {
        if (!IsOwner) return;
        DisableControllers();
    }

    

    [ClientRpc]
    private void MoveCommandClientRpc(Vector2Int moveDir, TimingAccuracyType accuracy)
    {
        if (IsServer) return;
        _movementComponent.MoveCommand(moveDir);
        OnActionFeedback(accuracy);
    }

    [ClientRpc]
    private void DefendCommandClientRpc(TimingAccuracyType accuracy)
    {
        if (IsServer) return;
        UseDefendSkillClient(accuracy);
        OnActionFeedback(accuracy);
    }

    [ClientRpc]
    private void MeleeCommandClientRpc(TimingAccuracyType accuracy)
    {
        if (IsServer) return;
        UseMeleeSkill(accuracy);
        OnActionFeedback(accuracy);
    }

    [ClientRpc]
    private void RangedCommandClientRpc(TimingAccuracyType accuracy)
    {
        if (IsServer) return;
        UseRangeSkill(accuracy);
        OnActionFeedback(accuracy);
    }

    [ClientRpc]
    private void SpecialCommandClientRpc(TimingAccuracyType accuracy)
    {
        if (IsServer) return;
        UseSpecialSkillClient(accuracy);
        OnActionFeedback(accuracy);
    }
}
