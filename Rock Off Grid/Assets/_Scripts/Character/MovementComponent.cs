using UnityEngine;
using UnityEngine.InputSystem;

public class MovementComponent : MonoBehaviour
{
    [SerializeField] protected Character _character;

    protected Vector2Int moveDirection = Vector2Int.zero;

    protected virtual void Move()
    {
        if (_character == null || _character.CurTile == null) return;
        if (moveDirection == Vector2Int.zero) return;

        Vector2Int moveTileDestination = new Vector2Int((int)_character.CurTile.Position.x, (int)_character.CurTile.Position.y) + moveDirection;

        GridManager.instance.MoveCharacterToTile(_character, moveTileDestination);
    }

    public virtual void MoveCommand(Vector2Int direction) { }

    public virtual void SetMovementTowardsEnemy(Tile tile) { }

    public virtual void SetRandomMovement() { }
}
