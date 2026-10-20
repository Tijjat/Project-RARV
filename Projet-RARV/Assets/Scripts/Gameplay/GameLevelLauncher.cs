
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLevelLauncher : NetworkBehaviour
{
    [Header("Level")]
    [SerializeField] private LevelMapLoader levelMapLoader;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    private bool waitingForScene;
    private string selectedLevelJson;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager != null &&
            NetworkManager.SceneManager != null)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }
    }

    public bool PrepareLevel()
    {
        if (!IsServer)
            return false;

        if (levelMapLoader == null)
        {
            Debug.LogError("LevelMapLoader is missing.");
            return false;
        }

        if (LevelManager.Instance == null ||
            LevelManager.Instance.SelectedLevel == null)
        {
            Debug.LogError(
                "No map selected. Select a map before launching."
            );
            return false;
        }

        // Capture the selected map before switching scenes.
        selectedLevelJson = LevelJson.Serialize(
            LevelManager.Instance.SelectedLevel
        );

        waitingForScene = true;

        Debug.Log(
            $"[GameLevelLauncher] Prepared map: " +
            LevelManager.Instance.SelectedLevel.levelName
        );

        return true;
    }


    private void OnSceneLoaded(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        Debug.Log(
            $"[GameLevelLauncher] Scene event: {sceneName}, " +
            $"Waiting={waitingForScene}, IsServer={IsServer}, " +
            $"TimedOut={clientsTimedOut.Count}"
        );

        if (!IsServer)
            return;

        // RETURNING TO LOBBY
        if (sceneName == "Lobby")
        {
            waitingForScene = false;

            if (clientsTimedOut.Count > 0)
            {
                Debug.LogWarning(
                    "[Lobby] Some clients did not finish loading."
                );
            }

            LobbySpawnManager spawnManager =
                LobbySpawnManager.Instance;

            if (spawnManager == null)
            {
                Debug.LogError(
                    "[Lobby] LobbySpawnManager not found!"
                );
                return;
            }

            Debug.Log(
                "[Lobby] Lobby loaded. Repositioning players..."
            );

            spawnManager.PositionExistingPlayers();

            return;
        }

        // LAUNCHING GAME
        if (!waitingForScene)
            return;

        if (sceneName != gameSceneName)
            return;

        waitingForScene = false;

        if (clientsTimedOut.Count > 0)
        {
            Debug.LogError(
                "Some clients failed to load the game scene."
            );
            return;
        }

        if (string.IsNullOrEmpty(selectedLevelJson))
        {
            Debug.LogError("Selected level JSON is empty.");
            return;
        }

        Debug.Log(
            "Game scene loaded. Generating selected level..."
        );

        levelMapLoader.LoadLevelOnServer(selectedLevelJson);
    }

}
