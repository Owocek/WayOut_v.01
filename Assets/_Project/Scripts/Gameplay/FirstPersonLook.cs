using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")] public Transform playerBody;
    [Header("Look settings")]
    public float sensitivityX = 1.0f;
    public float sensitivityY = 1.0f;
    public bool invertY = false;
    public float minPitch = -85f;
    public float maxPitch = 85f;
    public bool lockCursor = true;

    float _pitch;

    void OnEnable()
    {
        if (lockCursor) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        if (!playerBody) playerBody = transform.parent;
    }

    void OnDisable()
    {
        if (lockCursor) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }

    void Update()
    {
        var mouse = Mouse.current; if (mouse == null) return;
        Vector2 delta = mouse.delta.ReadValue();
        float dx = delta.x * sensitivityX;
        float dy = delta.y * sensitivityY * (invertY ? 1f : -1f);

        if (playerBody) playerBody.Rotate(Vector3.up, dx, Space.Self); // yaw na ciele gracza
        _pitch = Mathf.Clamp(_pitch + dy, minPitch, maxPitch);         // pitch na kamerze

        transform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
    }
}