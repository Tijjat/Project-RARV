
using Gameplay;
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

        PlayerData playerData = player.GetComponent<PlayerData>();
        
        playerData.TeleportClientRpc(
            playerData.PlayerCheckpointPosition.Value,
            playerData.PlayerCheckpointRotation.Value
        );
    }
}
