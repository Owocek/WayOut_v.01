using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravityScale = 1f; // 1 = Physics.gravity, możesz zwiększyć/zmniejszyć

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity; // tylko składowa Y faktycznie używana (grawitacja)

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!controller) controller = gameObject.AddComponent<CharacterController>();
        // sensowne wartości startowe
        controller.height = Mathf.Max(controller.height, 1.8f);
        controller.center = new Vector3(0, controller.height * 0.5f, 0);
        controller.radius = Mathf.Max(controller.radius, 0.3f);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        if (!controller.enabled) return;

        // Ruch na płaszczyźnie XZ w lokalnym układzie gracza (yaw)
        Vector3 moveDir = (transform.right * moveInput.x + transform.forward * moveInput.y);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // Poziomy ruch
        Vector3 horizontalMove = moveDir * moveSpeed;
        controller.Move(horizontalMove * Time.deltaTime);

        // Grawitacja (używamy CharacterController.isGrounded)
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f; // „trzyma” na ziemi

        velocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;

        // Ruch pionowy (grawitacja)
        controller.Move(velocity * Time.deltaTime);
    }
}