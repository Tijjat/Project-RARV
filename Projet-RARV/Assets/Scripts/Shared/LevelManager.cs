using UnityEngine;
using Unity.Netcode;

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance { get; private set; }

    public LevelData CurrentLevelData { get; private set; } = new LevelData();
    public LevelData SelectedLevel { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
            Instance = null;

        base.OnNetworkDespawn();
    }

    public void SetLevelData(LevelData level)
    {
        SelectedLevel = level;

        Debug.Log($"[LevelManager] Selected map: {level.levelName}");
    }

    [ClientRpc]
    private void ReceiveLevelDataClientRpc(string json)
    {
        CurrentLevelData = LevelJson.Deserialize(json);
        Debug.Log($"Received level: {CurrentLevelData.levelName}");
    }
}