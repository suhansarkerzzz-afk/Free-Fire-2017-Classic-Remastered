using UnityEngine;
using System.Collections.Generic;

// Aggressive villain AI: unlimited target search, fast pursuit, flanking and evasive movement.
public class BasicBotAI : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 5.6f;
    public float stopDistance = 9f;
    public float engageDistance = float.PositiveInfinity;
    public float turnSpeed = 18f;
    private float targetScanTimer, maneuverTimer, obstacleCheckTimer;
    private Vector3 avoidanceDirection;
    private float flankSign = 1f, movementBurst = 1f;

    private void Update()
    {
        HealthSystem health = GetComponent<HealthSystem>();
        if (health != null && health.IsDead) return;
        targetScanTimer -= Time.deltaTime;
        if (targetScanTimer <= 0f) { targetScanTimer = Random.Range(0.22f, 0.36f); FindNearestEnemy(); }
        maneuverTimer -= Time.deltaTime;
        if (maneuverTimer <= 0f)
        {
            maneuverTimer = Random.Range(1.2f, 2.4f);
            flankSign = Random.value < 0.5f ? -1f : 1f;
            movementBurst = Random.Range(1.0f, 1.45f);
        }

        Vector3 desiredDirection = Vector3.zero;
        bool chasing = false;
        if (target != null)
        {
            HealthSystem targetHealth = target.GetComponent<HealthSystem>();
            if (targetHealth == null || targetHealth.IsDead) target = null;
            else
            {
                Vector3 toTarget = target.position - transform.position; toTarget.y = 0f;
                float distance = toTarget.magnitude;
                if (distance > 0.01f)
                {
                    Vector3 toward = toTarget / distance;
                    if (distance <= 24f)
                    {
                        Vector3 strafe = Vector3.Cross(Vector3.up, toward) * flankSign;
                        float approach = distance > stopDistance ? 0.48f : -0.12f;
                        desiredDirection = (toward * approach + strafe * 0.92f).normalized;
                    }
                    else desiredDirection = (toward + Vector3.Cross(Vector3.up, toward) * flankSign * 0.22f).normalized;
                    chasing = true;
                }
            }
        }
        if (!chasing)
        {
            if (maneuverTimer <= 0.01f) { desiredDirection = Random.insideUnitSphere; desiredDirection.y = 0f; }
            else desiredDirection = transform.forward;
        }
        if (desiredDirection.sqrMagnitude < 0.01f) return;
        desiredDirection.Normalize();

        obstacleCheckTimer -= Time.deltaTime;
        if (obstacleCheckTimer <= 0f)
        {
            obstacleCheckTimer = Random.Range(0.12f, 0.22f);
            avoidanceDirection = desiredDirection;
            Vector3 origin = transform.position + Vector3.up * 0.8f;
            if (Physics.SphereCast(origin, 0.42f, desiredDirection, out RaycastHit hit, 1.8f) && !hit.transform.IsChildOf(transform))
            {
                Vector3 left = Vector3.Cross(Vector3.up, desiredDirection).normalized;
                Vector3 right = -left;
                avoidanceDirection = Random.value < 0.5f ? left : right;
            }
        }
        if (avoidanceDirection.sqrMagnitude > 0.01f) desiredDirection = avoidanceDirection.normalized;
        transform.position += desiredDirection * moveSpeed * movementBurst * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        Vector3 face = target != null ? target.position - transform.position : desiredDirection;
        face.y = 0f;
        if (face.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(face), turnSpeed * Time.deltaTime);
    }

    private void FindNearestEnemy()
    {
        target = null;
        float nearestSqr = float.PositiveInfinity;
        TeamAffiliation mine = GetComponent<TeamAffiliation>();
        List<HealthSystem> candidates = HealthSystem.ActiveActors;
        for (int i = 0; i < candidates.Count; i++)
        {
            HealthSystem candidate = candidates[i];
            if (candidate == null || candidate.gameObject == gameObject || candidate.IsDead) continue;
            TeamAffiliation theirs = candidate.GetComponent<TeamAffiliation>();
            if (mine != null && mine.IsFriendly(theirs)) continue;
            float distanceSqr = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr < nearestSqr) { nearestSqr = distanceSqr; target = candidate.transform; }
        }
    }
}