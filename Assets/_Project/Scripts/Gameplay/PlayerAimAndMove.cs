using UnityEngine;
using UnityEngine.InputSystem;

// Ruch bez WSAD. Kierunek = pozycja kursora/palca w świecie.
// Wymagane: CharacterController na graczu, kamera top-down w scenie.
[RequireComponent(typeof(CharacterController))]
public class PlayerAimAndMove : MonoBehaviour
{
    [Header("References")]
    public Camera topDownCamera;                      // jeśli puste -> użyje Camera.main (Twoja top-down)
    public Transform fpsCamera;                       // opcjonalnie: dziecko gracza (np. HeadAnchor/CameraFPS)

    [Header("Movement")]
    public float moveSpeed = 5f;                      // stała prędkość poruszania
    public float rotationSpeedDeg = 540f;            // szybkość obracania (deg/s)
    public float gravityScale = 1f;                  // 1 = Physics.gravity

    [Header("Aim")]
    public float aimPlaneYOffset = 0f;               // jeśli gracz stoi na Y=0, zostaw 0
    public float deadZoneRadius = 0.1f;              // strefa martwa przy samym graczu
    public bool usePhysicsRaycast = false;           // true jeśli wolisz raycast po kolizjach (podłoga z colliderem)
    public LayerMask groundMask = ~0;                // maska dla raycastu (gdy usePhysicsRaycast = true)

    [Header("Mobile / Input")]
    public bool requirePointerToMove = false;        // gdy true, bez dotyku/kliku gracz nie idzie
    public bool preferTouchOverMouse = true;         // na urządzeniach dotykowych używa palca

    CharacterController _controller;
    Vector3 _velocity;
    Vector2 _lastPointerPos;                         // zapamiętany cel, gdy nie ma dotyku

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (!topDownCamera) topDownCamera = Camera.main;
        if (!_controller)
            _controller = gameObject.AddComponent<CharacterController>();
    }

    void Update()
    {
        // 1) Pobierz pozycję kursora/palca
        if (!TryGetPointerScreenPosition(out Vector2 screenPos))
        {
            if (requirePointerToMove)
            {
                ApplyGravityOnly();
                return;
            }
            // brak wskaźnika: użyj ostatniej znanej pozycji, albo zatrzymaj ruch
            screenPos = _lastPointerPos; // może być (0,0) na starcie – wtedy nie ruszymy
        }
        else
        {
            _lastPointerPos = screenPos;
        }

        // 2) Znajdź punkt w świecie pod wskaźnikiem (na wysokości gracza)
        if (!TryGetAimWorldPoint(screenPos, out Vector3 aimPointWorld))
        {
            ApplyGravityOnly();
            return;
        }

        // 3) Kierunek do punktu (bez składowej Y)
        Vector3 toAim = aimPointWorld - transform.position;
        toAim.y = 0f;
        float distSqr = toAim.sqrMagnitude;

        // Obracaj gracza w stronę kursora
        if (distSqr > 1e-6f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toAim.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeedDeg * Time.deltaTime);
        }

        // 4) Ruch do przodu – ZAWSZE (chyba że w martwej strefie)
        if (distSqr > deadZoneRadius * deadZoneRadius)
        {
            Vector3 forward = transform.forward * moveSpeed;
            _controller.Move(forward * Time.deltaTime);
        }

        // 5) Grawitacja
        ApplyGravityOnly();

        // 6) (Opcjonalnie) dopasuj yaw FPS kamery do gracza
        if (fpsCamera)
        {
            // Jeżeli fpsCamera jest dzieckiem gracza, yaw podąża sam. Ten kod utrzymuje stały pitch (np. lekko w dół).
            float pitch = fpsCamera.localEulerAngles.x; // zostaw bieżący, jeśli chcesz stały, ustaw np. -10
            fpsCamera.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);
        }
    }

    void ApplyGravityOnly()
    {
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
        _velocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    bool TryGetPointerScreenPosition(out Vector2 screenPos)
    {
        screenPos = default;

        // Dotyk priorytetowo (na mobile)
        if (preferTouchOverMouse && Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.isPressed)
            {
                screenPos = touch.position.ReadValue();
                return true;
            }
            return false;
        }

        // Mysz
        if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
            return true;
        }
        return false;
    }

    bool TryGetAimWorldPoint(Vector2 screenPos, out Vector3 worldPoint)
    {
        worldPoint = default;
        if (!topDownCamera) return false;

        Ray ray = topDownCamera.ScreenPointToRay(screenPos);

        if (usePhysicsRaycast)
        {
            if (Physics.Raycast(ray, out var hit, 5000f, groundMask))
            {
                worldPoint = hit.point;
                return true;
            }
            return false;
        }
        else
        {
            // Płaszczyzna na wysokości gracza + offset (domyślnie 0)
            float planeY = transform.position.y + aimPlaneYOffset;
            Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
            if (plane.Raycast(ray, out float enter))
            {
                worldPoint = ray.GetPoint(enter);
                return true;
            }
            return false;
        }
    }
}