
using UnityEngine;
using Unity.Netcode;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        NetworkObject player = other.GetComponentInParent<NetworkObject>();

        if (player == null || !player.IsPlayerObject)
            return;

        if (!NetworkManager.Singleton.IsServer)
            return;

        Debug.Log($"[DeathZone] Player {player.OwnerClientId} fell off the map.");

        // TODO: Call your player death / respawn system here.
    }
}
