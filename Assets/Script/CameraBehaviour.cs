using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private float orbitSpeed = 50.0f;
    [SerializeField] private Vector3 orbitCenter = new Vector3(0, 800, 0);

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 300.0f;

    [Header("Keybindings")]
    [SerializeField] private KeyCode frontKey = KeyCode.W;
    [SerializeField] private KeyCode backKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;

    private void FixedUpdate()
    {
        float delta = orbitSpeed * Time.fixedDeltaTime;

        // Orbit horizontally around Y-axis (left/right)
        if (Input.GetKey(leftKey))
            transform.RotateAround(orbitCenter, Vector3.up, delta);

        if (Input.GetKey(rightKey))
            transform.RotateAround(orbitCenter, Vector3.up, -delta);

        // Orbit vertically around local X-axis (up/down)
        if (Input.GetKey(frontKey))
            transform.RotateAround(orbitCenter, transform.right, delta);

        if (Input.GetKey(backKey))
            transform.RotateAround(orbitCenter, transform.right, -delta);
    }

    private void LateUpdate()
    {
        // Zoom in/out along current forward direction
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.forward * scroll * zoomSpeed * Time.deltaTime, Space.Self);
    }

    // Debug visual for orbit center
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(orbitCenter, 0.2f);
    }
}
