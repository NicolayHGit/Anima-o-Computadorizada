using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MovementComponent
{    
    public override void MoveCommand(Vector2Int direction)
    {
        TimingAccuracyType accuracy = BeatManager.instance.GetTimingAccuracy();
        moveDirection = direction;
        Move();
    }
}
