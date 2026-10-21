using Gameplay;
using UnityEngine;
using Unity.Netcode;

public class CheckpointZone : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [SerializeField]
    private ParticleSystem checkpointConfetti;

    private void PlayCheckpointEffect()
    {
        if (checkpointConfetti == null)
            return;

        checkpointConfetti.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        checkpointConfetti.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject player = other.GetComponentInParent<NetworkObject>();

        if (player == null || !player.IsPlayerObject)
            return;

        if (!NetworkManager.Singleton.IsServer)
            return;

        PlayerData playerData = player.GetComponent<PlayerData>();
        
        playerData.PlayerCheckpointPosition.Value = spawnPoint.position;
        playerData.PlayerCheckpointRotation.Value = spawnPoint.rotation;
        PlayCheckpointEffect();
    }
}