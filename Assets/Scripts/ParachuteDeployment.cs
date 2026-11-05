using UnityEngine;

// Short offline plane flyover followed by a controllable parachute descent.
public class ParachuteDeployment : MonoBehaviour
{
    public float dropHeight = 38f;
    public float descentSpeed = 4.2f;
    public float glideSpeed = 3.2f;
    public float deploymentDelay = 1.5f;
    private CharacterController controller;
    private GameObject canopy;
    private float delay;
    private bool landed;
    private Vector3 drift;
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        transform.position = new Vector3(0f, dropHeight, -24f);
        drift = Vector3.forward;
        if (controller != null) controller.enabled = false;
        GameObject parachute = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        parachute.name = "ParachuteCanopy";
        parachute.transform.SetParent(transform, false);
        parachute.transform.localPosition = new Vector3(0f, 2.9f, 0f);
        parachute.transform.localScale = new Vector3(3.2f, 0.55f, 2.4f);
        parachute.GetComponent<Collider>().enabled = false;
        parachute.GetComponent<Renderer>().material.color = new Color(0.88f, 0.22f, 0.12f);
        canopy = parachute;
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plane.name = "IntroTransportPlane";
        plane.transform.position = new Vector3(0f, dropHeight + 8f, -38f);
        plane.transform.localScale = new Vector3(5f, 1f, 8f);
        plane.GetComponent<Renderer>().material.color = new Color(0.22f, 0.25f, 0.29f);
        Destroy(plane, 5f);
    }
    private void Update()
    {
        if (landed) return;
        delay += Time.deltaTime;
        if (delay < deploymentDelay) return;
        if (controller != null && !controller.enabled) controller.enabled = true;
        Vector3 horizontal = drift * glideSpeed;
#if ENABLE_LEGACY_INPUT_MANAGER
        horizontal += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")) * 2f;
#endif
        Vector3 motion = horizontal + Vector3.down * descentSpeed;
        if (controller != null) controller.Move(motion * Time.deltaTime);
        else transform.position += motion * Time.deltaTime;
        if (transform.position.y <= 1.15f)
        {
            Vector3 p = transform.position; p.y = 1.1f; transform.position = p;
            landed = true;
            if (controller != null) controller.enabled = false;
            if (canopy != null) Destroy(canopy);
            if (controller != null) controller.enabled = true;
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = true;
            Destroy(this);
        }
    }
}
