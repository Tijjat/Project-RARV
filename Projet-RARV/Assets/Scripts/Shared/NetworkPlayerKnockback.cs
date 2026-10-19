
using UnityEngine;
using Unity.Netcode;
using Gameplay;

public class NetworkPlayerKnockback : NetworkBehaviour
{
    private PlayerMovementController controller;

    private void Awake()
    {
        controller = GetComponentInChildren<PlayerMovementController>();
    }

    public void ApplyKnockbackFromServer(Vector3 force)
    {
        if (!IsServer)
            return;

        ApplyKnockbackRpc(force);
    }

    [Rpc(SendTo.Owner)]
    private void ApplyKnockbackRpc(Vector3 force)
    {
        if (controller == null)
        {
            Debug.LogError("PlayerMovementController not found!");
            return;
        }

        controller.ApplyKnockback(force);
    }
}
