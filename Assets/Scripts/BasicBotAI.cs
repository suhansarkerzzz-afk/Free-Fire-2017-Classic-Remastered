using UnityEngine;

public class BasicBotAI : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 2.2f;
    public float stopDistance = 3f;
    private float roamTimer;
    private Vector3 roamDirection;

    private void Update()
    {
        if (target != null)
        {
            Vector3 delta = target.position - transform.position;
            delta.y = 0f;
            if (delta.magnitude > stopDistance)
            {
                transform.position += delta.normalized * moveSpeed * Time.deltaTime;
                if (delta.sqrMagnitude > 0.05f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), 5f * Time.deltaTime);
            }
            return;
        }
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f) { roamDirection = Random.insideUnitSphere; roamDirection.y = 0f; roamTimer = Random.Range(2f, 5f); }
        transform.position += roamDirection.normalized * moveSpeed * 0.35f * Time.deltaTime;
    }
}
