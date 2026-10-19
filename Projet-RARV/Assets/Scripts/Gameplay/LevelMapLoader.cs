using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Attach to an in-scene GameObject with a NetworkObject.
// Call LoadLevelOnServer(json) from your host's level-selection/start flow.
public class LevelMapLoader : NetworkBehaviour
{
    [Serializable]
    public class PrefabEntry
    {
        public string prefabId;
        public GameObject prefab;
        [Tooltip("Enable for prefabs that contain a NetworkObject (e.g. the cannon trap).")]
        public bool networked;
    }

    [SerializeField] private List<PrefabEntry> prefabs = new List<PrefabEntry>();
    [SerializeField] private Transform localLevelRoot;
    [SerializeField] private bool allowInvalidLevelsForTesting = false;

    private readonly List<GameObject> localInstances = new List<GameObject>();
    private readonly List<NetworkObject> networkInstances = new List<NetworkObject>();
    private Dictionary<string, PrefabEntry> registry;

    private void Awake()
    {
        registry = new Dictionary<string, PrefabEntry>(StringComparer.Ordinal);
        foreach (PrefabEntry entry in prefabs)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.prefabId) || entry.prefab == null)
                continue;
            if (!registry.TryAdd(entry.prefabId, entry))
                Debug.LogError($"Duplicate prefab ID: {entry.prefabId}");
        }
    }

    // Use this on the host/server. JSON may come from a TextAsset, file, or level-selection UI.
    public void LoadLevelOnServer(string json)
    {
        Debug.Log(
            $"[LevelMapLoader] Load requested | " +
            $"IsServer={IsServer} | " +
            $"IsSpawned={IsSpawned} | " +
            $"JSON length={json?.Length ?? 0}"
        );
        if (!IsServer || !IsSpawned)
        {
            Debug.LogError("Only the spawned server LevelMapLoader can load a multiplayer level.");
            return;
        }

        LevelData level;
        try
        {
            level = LevelJson.Deserialize(json);
            ValidateLevel(level);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Level import rejected: {exception.Message}");
            return;
        }
        Debug.Log(
            $"[LevelMapLoader] Validation passed. " +
            $"Objects: {level.objects.Count}"
        );
        // Clear the previous level on the server, then tell all clients to rebuild static geometry.
        ClearNetworkInstances();
        BuildStaticLevelRpc(json);

        foreach (LevelObjectData obj in level.objects)
        {
            PrefabEntry entry = registry[obj.prefabId];
            if (!entry.networked)
                continue;

            GameObject instance = Instantiate(
                entry.prefab,
                obj.position,
                Quaternion.Euler(0f, obj.rotationY, 0f)
            );
            instance.name = obj.id;
            if (obj.prefabId == "platform_moving")
            {
                MovingPlatform movingPlatform =
                    instance.GetComponent<MovingPlatform>();

                if (movingPlatform != null)
                {
                    if (obj.parameters != null)
                    {
                        movingPlatform.Configure(
                            obj.parameters.distance,
                            obj.parameters.speed,
                            obj.parameters.pauseAtEnds
                        );
                    }
                }
            }
            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError($"Networked prefab '{obj.prefabId}' needs a NetworkObject on its root.");
                Destroy(instance);
                continue;
            }
            networkObject.Spawn(true);
            networkInstances.Add(networkObject);
            // Add prefab-specific parameter configuration here when corresponding gameplay APIs exist.
        }

        Debug.Log($"Loaded level '{level.levelName}' ({level.objects.Count} objects).");
    }

    [Rpc(SendTo.Everyone)]
    private void BuildStaticLevelRpc(string json)
    {
        ClearLocalInstances();
        LevelData level;
        try { level = LevelJson.Deserialize(json); }
        catch (Exception exception)
        {
            Debug.LogError($"Client failed to deserialize level: {exception.Message}");
            return;
        }

        foreach (LevelObjectData obj in level.objects)
        {
            if (!registry.TryGetValue(obj.prefabId, out PrefabEntry entry) || entry.networked)
                continue;

            GameObject instance = Instantiate(
                entry.prefab,
                obj.position,
                Quaternion.Euler(0f, obj.rotationY, 0f),
                localLevelRoot
            );
            instance.name = obj.id;
            localInstances.Add(instance);
        }
    }

    private void ValidateLevel(LevelData level)
    {
        if (!level.isValid && !allowInvalidLevelsForTesting)
            throw new FormatException("Level is marked isValid=false. Enable testing override only for inspection.");
        if (level.grid.width <= 0 || level.grid.length <= 0 || level.grid.cellSize <= 0f)
            throw new FormatException("Invalid grid dimensions.");

        int starts = 0, finishes = 0, checkpoints = 0, gameplayObjects = 0;
        HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (LevelObjectData obj in level.objects)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.id) || !ids.Add(obj.id))
                throw new FormatException("Missing or duplicate object ID.");
            if (!registry.ContainsKey(obj.prefabId))
                throw new FormatException($"Unknown prefab ID: {obj.prefabId}");
            if (obj.rotationY != 0 && obj.rotationY != 90 && obj.rotationY != 180 && obj.rotationY != 270)
                throw new FormatException($"Invalid rotationY for {obj.id}.");

            if (obj.prefabId == "platform_start") starts++;
            if (obj.prefabId == "platform_finish") finishes++;
            if (obj.prefabId == "checkpoint") checkpoints++;
            if (!obj.prefabId.StartsWith("decoration_", StringComparison.Ordinal)) gameplayObjects++;
        }
        if (starts != 1 || finishes != 1 || checkpoints < 1 || gameplayObjects < 10)
            throw new FormatException($"Level needs 1 start, 1 finish, >=1 checkpoint and >=10 gameplay objects. Found {starts}, {finishes}, {checkpoints}, {gameplayObjects}.");
    }

    private void ClearLocalInstances()
    {
        foreach (GameObject instance in localInstances)
            if (instance != null) Destroy(instance);
        localInstances.Clear();
    }

    private void ClearNetworkInstances()
    {
        foreach (NetworkObject instance in networkInstances)
            if (instance != null && instance.IsSpawned) instance.Despawn(true);
        networkInstances.Clear();
    }

    public override void OnNetworkDespawn()
    {
        ClearLocalInstances();
        if (IsServer) ClearNetworkInstances();
    }
}
