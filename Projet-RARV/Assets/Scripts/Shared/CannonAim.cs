
using UnityEngine;

public class CannonAim : MonoBehaviour
{
    [Header("Rotation Pivots")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchPivot;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Rotation Settings")]
    [SerializeField] private float yawSpeed = 90f;
    [SerializeField] private float pitchSpeed = 45f;
    [SerializeField] private float maxElevation = 45f;
    [SerializeField] private float maxDepression = 10f;

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
            transform.position,
            detectionRadius,
            playerLayer
        );

        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider player in players)
        {
            float distance = Vector3.Distance(
                transform.position,
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
        // Horizontal aiming
        Vector3 direction = target.position - yawPivot.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion desiredYaw =
                Quaternion.LookRotation(direction, Vector3.up);

            yawPivot.rotation = Quaternion.RotateTowards(
                yawPivot.rotation,
                desiredYaw,
                yawSpeed * Time.deltaTime
            );
        }

        // Vertical aiming
        Vector3 localDirection =
            yawPivot.InverseTransformDirection(
                target.position - pitchPivot.position
            );

        float angle = Mathf.Atan2(
            localDirection.y,
            localDirection.z
        ) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(
            angle,
            -maxDepression,
            maxElevation
        );

        Quaternion desiredPitch =
            initialPitchRotation *
            Quaternion.Euler(-angle, 0f, 0f);

        pitchPivot.localRotation = Quaternion.RotateTowards(
            pitchPivot.localRotation,
            desiredPitch,
            pitchSpeed * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius
        );
    }

    public Transform CurrentTarget => target;
}
