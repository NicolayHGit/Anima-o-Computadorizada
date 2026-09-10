using System;
using UnityEngine;
using UnityEngine.Events;

public class ControlPoint : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    private bool interacting = false;

    //public static event Action OnPointUpdate;

    private void OnMouseDown()
    {
        interacting = true;
        Debug.Log("Mouse Down on Control Point: " + transform.name);
    }

    private void OnMouseUp()
    {
        interacting = false;
        Debug.Log("Mouse Up on Control Point: " + transform.name);
    }

    private void OnMouseDrag()
    {
        if (!interacting) return;
        Debug.Log("Dragging Control Point: " + transform.name);

        Vector2 screenPosition = Input.mousePosition;

        // 2. Convert the screen pixels to 2D world coordinates
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        // 3. For 2D, explicitly set Z to 0 so objects don't clip behind the camera

        // Example: Print it or move this object to the mouse position
        Debug.Log($"Mouse World Position: {worldPosition}");
        transform.position = worldPosition;
        _gameManager.UpdateCurve();
    }
}
