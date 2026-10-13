using UnityEngine;

public class EnemyMovement : MovementComponent
{
    public override void SetMovementTowardsEnemy(Tile tile)
    {
        Vector2Int dir = tile.Position - _character.CurTile.Position;

        Vector2Int moveDir;

        // 🔹 Se muito perto, chance de dar um "dash lateral" ou recuar
        if (dir.magnitude <= 1f)
        {
            if (Random.value < 0.5f)
                moveDir = new Vector2Int(0, Random.value < 0.5f ? 1 : -1); // esquiva vertical
            else
                moveDir = new Vector2Int(-(int)Mathf.Sign(dir.x), 0); // recuo
        }
        else
        {
            // 🔹 Se longe, move normal
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                moveDir = new Vector2Int((int)Mathf.Sign(dir.x), 0);
            else
                moveDir = new Vector2Int(0, (int)Mathf.Sign(dir.y));
        }

        MoveCommand(moveDir);
    }

    public override void SetRandomMovement()
    {
        Vector2Int dir = new();
        //bool possibleMovement = false;
        int value = Random.Range(-1, 2);
        if(Random.Range(0f,1f) < 0.5)
            dir = new Vector2Int(value, 0);
        else
            dir = new Vector2Int(0, value);
        /*
        while (!possibleMovement)
        {
            dir = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));

            Tile tile = GridManager.instance.GetTileAtPosition(_character.CurTile.Position + dir, _character.Type);
            if (tile != null) possibleMovement = true;
        }
        */

        MoveCommand(dir);
    }

    public override void MoveCommand(Vector2Int direction)
    {
        moveDirection = direction;
        Move();
    }

}
