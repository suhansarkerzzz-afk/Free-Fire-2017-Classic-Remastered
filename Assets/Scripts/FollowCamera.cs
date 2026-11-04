using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5f, -7f);
    public float followSpeed = 8f;
    public float lookHeight = 1.4f;
    public float lookSensitivity = 0.16f;
    public float minPitch = -15f;
    public float maxPitch = 55f;

    private float yaw;
    private float pitch = 18f;
    private Vector2 lookDelta;

    public void AddLookDelta(Vector2 delta) { lookDelta += delta; }

    private void LateUpdate()
    {
        if (target == null) return;
        yaw += lookDelta.x * lookSensitivity;
        pitch = Mathf.Clamp(pitch - lookDelta.y * lookSensitivity, minPitch, maxPitch);
        lookDelta = Vector2.zero;

        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desired = target.position + orbit * offset;
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(target.position + Vector3.up * lookHeight - transform.position);
    }
}