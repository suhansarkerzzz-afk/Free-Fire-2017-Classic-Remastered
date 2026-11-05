using UnityEngine;

public class BasicBotAI : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 2.1f;
    public float stopDistance = 4.5f;
    public float engageDistance = 32f;
    public float turnSpeed = 5f;
    private float roamTimer;
    private Vector3 roamDirection;

    private void Update()
    {
        HealthSystem health = GetComponent<HealthSystem>();
        if (health != null && health.IsDead) return;

        Vector3 destinationDirection = Vector3.zero;
        bool chasing = false;
        if (target != null)
        {
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude <= engageDistance && toTarget.magnitude > stopDistance)
            {
                destinationDirection = toTarget.normalized;
                chasing = true;
            }
        }

        if (!chasing)
        {
            roamTimer -= Time.deltaTime;
            if (roamTimer <= 0f)
            {
                roamDirection = Random.insideUnitSphere;
                roamDirection.y = 0f;
                roamTimer = Random.Range(2.5f, 6f);
            }
            destinationDirection = roamDirection.normalized;
        }

        if (destinationDirection.sqrMagnitude < 0.01f) return;
        Vector3 origin = transform.position + Vector3.up * 0.8f;
        if (Physics.SphereCast(origin, 0.45f, destinationDirection, out RaycastHit hit, 1.4f))
        {
            if (!hit.transform.IsChildOf(transform))
            {
                destinationDirection = Quaternion.Euler(0f, Random.Range(65f, 130f), 0f) * destinationDirection;
                if (chasing) destinationDirection = Vector3.Cross(Vector3.up, destinationDirection).normalized;
            }
        }

        transform.position += destinationDirection * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        Quaternion facing = Quaternion.LookRotation(destinationDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, facing, turnSpeed * Time.deltaTime);
    }
}
