using UnityEngine;
using UnityEngine.UI;

// Arcade vehicle with enter/exit interaction; intended for local offline play.
public class SimpleVehicleController : MonoBehaviour
{
    public float driveSpeed = 13f;
    public float turnSpeed = 85f;
    private Transform player;
    private CharacterController playerController;
    private PlayerMovement playerMovement;
    private bool occupied;
    private Text hint;
    private Vector3 parkedPosition;

    private void Start()
    {
        parkedPosition = transform.position;
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            GameObject textObject = new GameObject("VehicleHint", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(canvas.transform, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.13f);
            rect.anchorMax = new Vector2(0.5f, 0.13f);
            rect.sizeDelta = new Vector2(360f, 42f);
            hint = textObject.GetComponent<Text>();
            hint.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            hint.fontSize = 18;
            hint.alignment = TextAnchor.MiddleCenter;
            hint.color = Color.white;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject found = GameObject.Find("Player");
            if (found != null) player = found.transform;
            if (player != null)
            {
                playerController = player.GetComponent<CharacterController>();
                playerMovement = player.GetComponent<PlayerMovement>();
            }
        }
        if (player == null) return;

        if (!occupied)
        {
            bool near = Vector3.Distance(player.position, transform.position) < 3.5f;
            if (hint != null) hint.text = near ? "VEHICLE NEARBY  |  PRESS E TO DRIVE" : "";
#if ENABLE_LEGACY_INPUT_MANAGER
            if (near && Input.GetKeyDown(KeyCode.E)) EnterVehicle();
#endif
            return;
        }

        if (hint != null) hint.text = "DRIVING  |  PRESS E TO EXIT";
        float forward = 0f;
        float turn = 0f;
#if ENABLE_LEGACY_INPUT_MANAGER
        forward = Input.GetAxisRaw("Vertical");
        turn = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.E)) ExitVehicle();
#endif
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
        transform.position += transform.forward * forward * driveSpeed * Time.deltaTime;
        player.position = transform.position + Vector3.up * 1.2f;
    }

    public void EnterVehicle()
    {
        if (occupied || player == null || Vector3.Distance(player.position, transform.position) > 3.5f) return;
        occupied = true;
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerController != null) playerController.enabled = false;
        player.SetParent(transform);
        player.localPosition = new Vector3(0f, 1.1f, 0f);
    }

    public void ExitVehicle()
    {
        if (!occupied || player == null) return;
        occupied = false;
        player.SetParent(null);
        player.position = transform.position + transform.right * 2f + Vector3.up;
        if (playerController != null) playerController.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (hint != null) hint.text = "";
    }
}
