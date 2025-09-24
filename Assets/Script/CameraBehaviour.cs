using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // New Input System

public class CameraBehaviour : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private float orbitSpeed = 50.0f;
    [SerializeField] private Vector3 orbitCenter = new Vector3(0, 800, 0);

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 30000.0f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 300f; 

    private Vector2 lastMousePosition;
    private bool isPanning = false;

    private void FixedUpdate()
    {
        float delta = orbitSpeed * Time.fixedDeltaTime;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Orbit horizontally 
        if (keyboard.aKey.isPressed)
            transform.RotateAround(orbitCenter, Vector3.up, delta);

        if (keyboard.dKey.isPressed)
            transform.RotateAround(orbitCenter, Vector3.up, -delta);

        // Orbit vertically 
        if (keyboard.wKey.isPressed)
            transform.RotateAround(orbitCenter, transform.right, delta);

        if (keyboard.sKey.isPressed)
            transform.RotateAround(orbitCenter, transform.right, -delta);
    }

    private void LateUpdate()
    {

        var mouse = Mouse.current;
        if (mouse == null) return;

        // zoom code:
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 dir = (transform.position - orbitCenter).normalized; // vector from planet to camera
            transform.position += dir * scroll * zoomSpeed * Time.deltaTime;
        }

        // panning code
        if (mouse.leftButton.isPressed)
        {
            // Prevent panning if pointer is over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                isPanning = false;
                return;
            }

            Vector2 currentPos = mouse.position.ReadValue();

            if (!isPanning)
            {
                isPanning = true;
                lastMousePosition = currentPos;
            }
            else
            {
                Vector2 delta = currentPos - lastMousePosition;
                lastMousePosition = currentPos;

                Vector3 panMovement = (-transform.right * delta.x - transform.up * delta.y) * panSpeed * Time.deltaTime;
                transform.position += panMovement;
            }
        }
        else
        {
            isPanning = false;
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(orbitCenter, 0.2f);

    }
}
