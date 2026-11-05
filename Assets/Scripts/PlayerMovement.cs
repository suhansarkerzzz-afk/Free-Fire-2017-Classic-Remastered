using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 12f;
    public float sprintMultiplier = 1.45f;
    public float jumpHeight = 1.15f;
    public float gravity = -20f;
    private CharacterController controller;
    private float verticalVelocity;
    private static Vector2 mobileMove;
    private static bool mobileSprint;
    private static bool mobileJump;

    public static Vector2 CurrentMobileMove { get { return mobileMove; } }
    public static bool MobileSprint { get { return mobileSprint; } }
    public static void SetMobileMove(Vector2 value) { mobileMove = Vector2.ClampMagnitude(value, 1f); }
    public static void SetMobileSprint(bool value) { mobileSprint = value; }
    public static void RequestJump() { mobileJump = true; }

    private void Awake() { controller = GetComponent<CharacterController>(); }

    private void Update()
    {
        Vector2 input = mobileMove;
        bool sprint = mobileSprint;
        bool jump = mobileJump;
        mobileJump = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        Vector2 keyboard = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (keyboard.sqrMagnitude > 0.01f) input = Vector2.ClampMagnitude(keyboard, 1f);
        sprint = sprint || Input.GetKey(KeyCode.LeftShift);
        jump = jump || Input.GetKeyDown(KeyCode.Space);
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
        if (controller != null && controller.enabled)
        {
            if (controller.isGrounded)
            {
                if (verticalVelocity < 0f) verticalVelocity = -2f;
                if (jump) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else verticalVelocity += gravity * Time.deltaTime;

            float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);
            Vector3 velocity = direction.normalized * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }}