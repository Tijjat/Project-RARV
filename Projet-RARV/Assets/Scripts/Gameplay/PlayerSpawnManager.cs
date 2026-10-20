using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay
{
    public class PlayerSpawnManager : NetworkBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
                return;

            NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadCompleted;
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null && NetworkManager.SceneManager != null)
                NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadCompleted;
        }

        private void OnLoadCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        System.Collections.Generic.List<ulong> clientsCompleted,
        System.Collections.Generic.List<ulong> clientsTimedOut)
        {
            if (sceneName != "Game" || !IsServer)
                return;

            if (clientsTimedOut.Count > 0)
            {
                Debug.LogWarning(
                    $"{clientsTimedOut.Count} client(s) failed to load Game."
                );

                return;
            }

            SpawnPlayers();

            Debug.Log("Player Spawned");
            if (GameManager.Instance != null)
            {
                Debug.Log("Game manager instance null to instantiate");
                GameManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("GameManager not found in Laboratory!");
            }
        }


        private void SpawnPlayers()
        {
            int index = 0;

            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                if (index >= spawnPoints.Length)
                    break;

                if (client.PlayerObject == null)
                    continue;

                Transform point = spawnPoints[index];

                PlayerData playerData =
                    client.PlayerObject.GetComponent<PlayerData>();

                if (playerData == null)
                    continue;

                ClientRpcParams rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { client.ClientId }
                    }
                };

                playerData.TeleportClientRpc(
                    point.position,
                    point.rotation,
                    rpcParams
                );

                Debug.Log(
                    $"Teleport request for player {client.ClientId} to {point.position}"
                );

                index++;
            }
        }

    }
}