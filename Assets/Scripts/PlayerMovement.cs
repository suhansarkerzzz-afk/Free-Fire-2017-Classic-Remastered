using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 12f;
    private CharacterController controller;
    private static Vector2 mobileMove;

    public static void SetMobileMove(Vector2 value) { mobileMove = Vector2.ClampMagnitude(value, 1f); }

    private void Awake() { controller = GetComponent<CharacterController>(); }

    private void Update()
    {
        Vector2 input = mobileMove;
#if ENABLE_LEGACY_INPUT_MANAGER
        Vector2 keyboard = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (keyboard.sqrMagnitude > 0.01f) input = Vector2.ClampMagnitude(keyboard, 1f);
#endif
        Camera cam = Camera.main;
        Vector3 forward = cam ? Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 right = cam ? Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized : Vector3.right;
        Vector3 direction = forward * input.y + right * input.x;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
        }
        if (controller != null)
        {
            Vector3 velocity = direction.normalized * moveSpeed;
            velocity.y = -9.81f;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
