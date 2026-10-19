
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CannonAim))]
public class CannonShooter : NetworkBehaviour
{
    private enum CannonState
    {
        Idle,
        Aiming,
        Cooldown
    }

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject cannonballPrefab;

    [Header("Firing")]
    [SerializeField] private float aimTime = 1.5f;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float projectileSpeed = 20f;

    [Header("Accuracy")]
    [SerializeField] private float maxAimError = 8f;

    private CannonAim cannonAim;
    private CannonState state = CannonState.Idle;
    private float timer;

    private void Awake()
    {
        cannonAim = GetComponent<CannonAim>();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        Transform target = cannonAim.CurrentTarget;

        switch (state)
        {
            case CannonState.Idle:
                if (target != null)
                {
                    timer = aimTime;
                    state = CannonState.Aiming;
                }
                break;

            case CannonState.Aiming:
                if (target == null)
                {
                    state = CannonState.Idle;
                    break;
                }

                timer -= Time.deltaTime;

                if (timer <= 0f && IsAimedAt(target))
                {
                    Fire();
                    timer = cooldown;
                    state = CannonState.Cooldown;
                }
                break;

            case CannonState.Cooldown:
                timer -= Time.deltaTime;

                if (timer <= 0f)
                    state = CannonState.Idle;
                break;
        }
    }

    private bool IsAimedAt(Transform target)
    {
        Vector3 targetPosition =
            target.position + Vector3.up * 1f;

        Vector3 direction =
            targetPosition - firePoint.position;

        return Vector3.Angle(
            firePoint.forward,
            direction
        ) <= maxAimError;
    }

    private void Fire()
    {
        if (cannonballPrefab == null || firePoint == null)
        {
            Debug.LogError("Missing cannonball prefab or FirePoint!");
            return;
        }

        GameObject ball = Instantiate(
            cannonballPrefab,
            firePoint.position + firePoint.forward * 0.5f,
            firePoint.rotation
        );
        ball.GetComponent<Cannonball>()
            .SetTravelDirection(firePoint.forward);

        Collider[] cannonColliders = GetComponentsInChildren<Collider>();
        Collider[] ballColliders = ball.GetComponentsInChildren<Collider>();

        foreach (Collider cannonCollider in cannonColliders)
        {
            foreach (Collider ballCollider in ballColliders)
            {
                Physics.IgnoreCollision(cannonCollider, ballCollider, true);
            }
        }

        NetworkObject networkObject =
            ball.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError("Cannonball needs a NetworkObject!");
            Destroy(ball);
            return;
        }

        networkObject.Spawn();

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        Debug.Log("Network cannonball spawned!");
    }
}
