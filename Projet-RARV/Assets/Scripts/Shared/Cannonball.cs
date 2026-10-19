
using UnityEngine;
using Unity.Netcode;
using StarterAssets;

[RequireComponent(typeof(Rigidbody))]
public class Cannonball : NetworkBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float horizontalKnockback = 12f;
    [SerializeField] private float verticalKnockback = 8f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 6f;

    private float spawnTime;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            spawnTime = Time.time;
    }

    private void Update()
    {
        if (!IsServer || !IsSpawned)
            return;

        if (Time.time - spawnTime >= lifetime)
            NetworkObject.Despawn();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || !IsSpawned)
            return;

        Debug.Log($"Cannonball hit: {collision.gameObject.name}");

        NetworkPlayerKnockback player =
            collision.collider.GetComponentInParent<NetworkPlayerKnockback>();

        if (player != null)
        {
            // Push the player in the projectile's travel direction
            Vector3 direction = _travelDirection;
            direction.y = 0f;
            direction.Normalize();

            if (direction.sqrMagnitude > 0.001f)
                direction.Normalize();
            else
                direction = transform.forward;

            Vector3 knockback =
                direction * horizontalKnockback +
                Vector3.up * verticalKnockback;

            Debug.Log($"Sending knockback: {knockback}");

            player.ApplyKnockbackFromServer(knockback);
        }

        NetworkObject.Despawn();
    }

    private Vector3 _travelDirection;

    public void SetTravelDirection(Vector3 direction)
    {
        _travelDirection = direction.normalized;
    }
}
