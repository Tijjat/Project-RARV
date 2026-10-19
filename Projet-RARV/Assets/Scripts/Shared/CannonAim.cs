
using UnityEngine;

public class CannonAim : MonoBehaviour
{
    [Header("Rotation Pivots")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchPivot;

    [Header("Rotation Settings")]
    [SerializeField] private float yawSpeed = 90f;
    [SerializeField] private float pitchSpeed = 45f;
    [SerializeField] private float maxElevation = 45f;
    [SerializeField] private float maxDepression = 10f;

    [Header("Detection")]
    [SerializeField] private Transform detectionCenter;
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Aiming")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float targetHeightOffset = 1f;

    private Transform target;
    private Quaternion initialPitchRotation;

    private void Awake()
    {
        initialPitchRotation = pitchPivot.localRotation;
    }

    private void Update()
    {
        target = FindClosestPlayer();

        if (target == null)
            return;

        AimAtTarget();
    }

    private Transform FindClosestPlayer()
    {
        Collider[] players = Physics.OverlapSphere(
        detectionCenter.position,
        detectionRadius,
        playerLayer
    );

        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider player in players)
        {
            float distance = Vector3.Distance(
                detectionCenter.position,
                player.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = player.transform;
            }
        }

        return closest;
    }

    private void AimAtTarget()
    {
        // Aim at the player's body rather than their feet.
        Vector3 targetPosition =
            target.position + Vector3.up * targetHeightOffset;

        // Horizontal aiming
        Vector3 horizontalDirection =
            targetPosition - yawPivot.position;

        horizontalDirection.y = 0f;

        if (horizontalDirection.sqrMagnitude > 0.001f)
        {
            Quaternion desiredYaw =
                Quaternion.LookRotation(horizontalDirection, Vector3.up);

            yawPivot.rotation = Quaternion.RotateTowards(
                yawPivot.rotation,
                desiredYaw,
                yawSpeed * Time.deltaTime
            );
        }

        // Vertical aiming from the actual projectile origin
        Vector3 direction =
            targetPosition - firePoint.position;

        Vector3 localDirection =
            yawPivot.InverseTransformDirection(direction);

        float desiredAngle = Mathf.Atan2(
            localDirection.y,
            localDirection.z
        ) * Mathf.Rad2Deg;

        desiredAngle = Mathf.Clamp(
            desiredAngle,
            -maxDepression,
            maxElevation
        );

        Quaternion desiredPitch =
            initialPitchRotation *
            Quaternion.Euler(-desiredAngle, 0f, 0f);

        pitchPivot.localRotation = Quaternion.RotateTowards(
            pitchPivot.localRotation,
            desiredPitch,
            pitchSpeed * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (detectionCenter == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            detectionCenter.position,
            detectionRadius
        );
    }

    public Transform CurrentTarget => target;
}
