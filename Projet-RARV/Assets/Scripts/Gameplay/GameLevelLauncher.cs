
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLevelLauncher : NetworkBehaviour
{
    [Header("Level")]
    [SerializeField] private LevelMapLoader levelMapLoader;
    [SerializeField] private TextAsset levelJson;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    private bool waitingForScene;

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

        if (levelMapLoader == null || levelJson == null)
        {
            Debug.LogError("LevelMapLoader or Level JSON is missing.");
            return false;
        }

        waitingForScene = true;
        return true;
    }

    private void OnSceneLoaded(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (!IsServer || !waitingForScene)
            return;

        if (sceneName != gameSceneName)
            return;

        waitingForScene = false;

        if (clientsTimedOut.Count > 0)
        {
            Debug.LogError("Some clients failed to load the game scene.");
            return;
        }

        Debug.Log("Game scene loaded. Generating level...");

        levelMapLoader.LoadLevelOnServer(levelJson.text);
    }
}
