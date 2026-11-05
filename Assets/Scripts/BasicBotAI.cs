using UnityEngine;
using System.Collections.Generic;

public class BasicBotAI : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 2.1f;
    public float stopDistance = 4.5f;
    public float engageDistance = 32f;
    public float turnSpeed = 5f;
    private float roamTimer;
    private float targetScanTimer;
    private Vector3 roamDirection;
    private float obstacleCheckTimer;
    private Vector3 avoidanceDirection;

    private void Update()
    {
        HealthSystem health = GetComponent<HealthSystem>();
        if (health != null && health.IsDead) return;

        targetScanTimer -= Time.deltaTime;
        if (targetScanTimer <= 0f)
        {
            targetScanTimer = Random.Range(1.4f, 2.0f);
            target = null;
            float nearest = 34f;
            TeamAffiliation mine = GetComponent<TeamAffiliation>();
            List<HealthSystem> candidates = HealthSystem.ActiveActors;
            for (int i = 0; i < candidates.Count; i++)
            {
                HealthSystem candidate = candidates[i];
                if (candidate == null || candidate.gameObject == gameObject || candidate.IsDead) continue;
                TeamAffiliation theirs = candidate.GetComponent<TeamAffiliation>();
                if (mine != null && mine.IsFriendly(theirs)) continue;
                float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                if (distance < nearest * nearest)
                {
                    nearest = Mathf.Sqrt(distance);
                    target = candidate.transform;
                }
            }
        }

        Vector3 destinationDirection = Vector3.zero;
        bool chasing = false;
        bool holdingPosition = false;
        if (target != null)
        {
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude <= engageDistance)
            {
                if (toTarget.magnitude > stopDistance)
                {
                    destinationDirection = toTarget.normalized;
                    chasing = true;
                }
                else holdingPosition = true;
            }
        }

        if (holdingPosition) return;

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
        obstacleCheckTimer -= Time.deltaTime;
        if (obstacleCheckTimer <= 0f)
        {
            obstacleCheckTimer = Random.Range(0.18f, 0.30f);
            avoidanceDirection = destinationDirection;
            Vector3 origin = transform.position + Vector3.up * 0.8f;
            if (Physics.SphereCast(origin, 0.45f, destinationDirection, out RaycastHit hit, 1.4f) &&
                !hit.transform.IsChildOf(transform))
            {
                avoidanceDirection = Quaternion.Euler(0f, Random.Range(65f, 130f), 0f) * destinationDirection;
                if (chasing) avoidanceDirection = Vector3.Cross(Vector3.up, avoidanceDirection).normalized;
            }
        }
        if (avoidanceDirection.sqrMagnitude > 0.01f) destinationDirection = avoidanceDirection;

        transform.position += destinationDirection * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        Quaternion facing = Quaternion.LookRotation(destinationDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, facing, turnSpeed * Time.deltaTime);
    }
}
