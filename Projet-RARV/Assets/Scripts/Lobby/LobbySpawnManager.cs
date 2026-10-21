
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Gameplay;

public class LobbySpawnManager : MonoBehaviour
{
    public static LobbySpawnManager Instance { get; private set; }

    [SerializeField] private Transform[] spawnPoints;

    private readonly Dictionary<ulong, int> assignedSpawns = new();

    private void Awake()
    {
        Instance = this;

        Debug.Log(
            $"[Spawn] LobbySpawnManager initialized with {spawnPoints.Length} points."
        );
    }
    private void Start()
    {
        ReserveHostSpawn();
    }
    public Transform GetSpawnPoint(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[Spawn] No lobby spawn points assigned!");
            return null;
        }

        if (assignedSpawns.TryGetValue(clientId, out int existingIndex))
            return spawnPoints[existingIndex];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
                continue;

            if (assignedSpawns.ContainsValue(i))
                continue;

            assignedSpawns[clientId] = i;

            Debug.Log($"[Spawn] Client {clientId} assigned spawn {i}");

            return spawnPoints[i];
        }

        Debug.LogWarning("[Spawn] All lobby spawn points occupied.");
        return null;
    }

    public void ReleaseSpawnPoint(ulong clientId)
    {
        assignedSpawns.Remove(clientId);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    public void ReserveHostSpawn()
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
            return;

        ulong hostId = NetworkManager.Singleton.LocalClientId;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        if (assignedSpawns.ContainsKey(hostId))
            return;

        assignedSpawns[hostId] = 0;

        Debug.Log("[Spawn] Reserved SpawnPoint 1 for host.");
    }
    public void PositionExistingPlayers()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null || !networkManager.IsServer)
            return;

        foreach (var client in networkManager.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerData player =
                client.PlayerObject.GetComponent<PlayerData>();

            if (player == null)
                continue;

            Transform spawn = GetSpawnPoint(client.ClientId);

            if (spawn == null)
                continue;

            // Teleport only the player who owns this character.
            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { client.ClientId }
                }
            };

            player.TeleportClientRpc(
                spawn.position,
                spawn.rotation,
                rpcParams
            );

            Debug.Log(
                $"[Lobby] Teleporting client {client.ClientId} " +
                $"to {spawn.position}"
            );
        }
    }
    public void PositionNewPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(
            clientId, out NetworkClient client))
            return;

        if (client.PlayerObject == null)
            return;

        Gameplay.PlayerData player =
            client.PlayerObject.GetComponent<Gameplay.PlayerData>();

        if (player == null)
            return;

        Transform spawn = GetSpawnPoint(clientId);

        if (spawn == null)
            return;

        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };

        player.TeleportClientRpc(
            spawn.position,
            spawn.rotation,
            rpcParams
        );

        Debug.Log(
            $"[Lobby] Initial teleport for client {clientId} " +
            $"to {spawn.position}"
        );
    }
}
