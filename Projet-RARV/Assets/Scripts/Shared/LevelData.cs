using System;
using System.Collections.Generic;
using UnityEngine;

// Shared by the VR editor and the PC multiplayer game.
// Do not rename serialized fields without incrementing formatVersion.
[Serializable]
public class LevelData
{
    public int formatVersion = 1;
    public string levelId = "";
    public string levelName = "";
    public bool isValid = false;
    public LevelGrid grid = new LevelGrid();
    public List<LevelObjectData> objects = new List<LevelObjectData>();
}

[Serializable]
public class LevelGrid
{
    public int width = 20;
    public int length = 30;
    public float cellSize = 2f;
    public Vector3 origin = Vector3.zero;
}

[Serializable]
public class LevelObjectData
{
    public string id = "";
    public string prefabId = "";
    // World-space transform; rotationY must be 0, 90, 180 or 270.
    public Vector3 position = Vector3.zero;
    public int rotationY = 0;
    // Prefab-specific values. Unused values are ignored by other prefabs.
    public LevelObjectParameters parameters = new LevelObjectParameters();
}

[Serializable]
public class LevelObjectParameters
{
    // Moving platform: distance, speed and pause.
    public float distance = 5f;
    public float speed = 2f;
    public float pauseAtEnds = 0.5f;

    // Launcher, bumper, fan, trampoline: optional per-object overrides.
    // The PC game can use prefab defaults where these values are not configurable.
    public float detectionRange = 0f;
    public float launchForce = 0f;
    public float bumperForce = 0f;
    public float fanForce = 0f;
    public float trampolineForce = 0f;
}

public static class LevelJson
{
    public static string Serialize(LevelData level)
    {
        if (level == null) throw new ArgumentNullException(nameof(level));
        return JsonUtility.ToJson(level, true);
    }

    public static LevelData Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("Level JSON is empty.", nameof(json));

        LevelData level = JsonUtility.FromJson<LevelData>(json);
        if (level == null || level.formatVersion != 1 || level.grid == null || level.objects == null)
            throw new FormatException("Invalid or unsupported Factory Frenzy level format.");
        return level;
    }
}
