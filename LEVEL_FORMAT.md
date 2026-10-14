# Factory Frenzy — Shared Level Format (v1)

The VR editor creates and saves levels. The PC multiplayer game reads them. Copy `LevelData.cs` into both Unity projects (for example `Assets/Scripts/Shared/LevelData.cs`) and keep the same field names and prefab IDs.

## Files

- `LevelData.cs`: Unity `JsonUtility`-compatible classes and serialization helpers.
- `LevelTemplate.json`: illustrative work-in-progress level with every gameplay prefab type.
- `LEVEL_FORMAT.md`: this contract.

## Root fields

| Field | Type | Meaning |
|---|---|---|
| `formatVersion` | int | Schema version; currently `1` |
| `levelId` | string | Stable unique level ID |
| `levelName` | string | Display name / suggested filename |
| `isValid` | bool | Last editor validation result; false for incomplete levels |
| `grid` | object | Editable horizontal map dimensions |
| `objects` | array | Placed objects, including decorations |

`grid.width` and `grid.length` are counts of cells on X and Z; `grid.cellSize` is in Unity world units. `grid.origin` is the world-space origin. Objects use world-space coordinates, including Y for vertical levels. This is not a tile-by-tile map serialization.

## Object fields

| Field | Type | Meaning |
|---|---|---|
| `id` | string | Unique instance ID within this level |
| `prefabId` | string | Stable ID resolved by the PC and VR prefab registries |
| `position` | Vector3 JSON object | World-space `{x,y,z}` |
| `rotationY` | int | Yaw: `0`, `90`, `180`, `270` |
| `parameters` | object | Prefab-specific gameplay parameters; `{}` for defaults |

No Unity `GameObject` references or prefab asset paths are serialized. No snap connections, editor locks, placement previews, or validation rules are transferred.

## Prefab IDs

- `platform_start`
- `platform_finish`
- `platform_basic`
- `platform_moving`
- `platform_trampoline`
- `checkpoint`
- `trap_bumper`
- `trap_launcher`
- `trap_fan`
- `decoration_crate_01` (example; other decorations can use their own IDs)

These IDs are a proposed common contract; configure the matching prefabs in both applications.

## Object parameters

`parameters` is a shared class because Unity's built-in `JsonUtility` does not support arbitrary dictionaries. Only fields relevant to the object's prefab are used.

| Field | Used by | Meaning |
|---|---|---|
| `moveStart`, `moveEnd` | Moving platform | World-space endpoints |
| `travelTime` | Moving platform | One-way travel duration (seconds) |
| `detectionRange`, `launchForce` | Launcher | Optional launcher configuration |
| `bumperForce` | Bumper | Optional collision impulse |
| `fanForce` | Fan | Optional wind force |
| `trampolineForce` | Trampoline | Optional bounce force |

If the VR editor does not expose a parameter, use the PC prefab's default. A missing numeric field in `JsonUtility` deserialization may be zero, so the importer should distinguish supported overrides by prefab and use agreed defaults. For the first implementation, always export all configured moving-platform values.

## Validation and saving

Validation belongs to the **VR editor**, not the JSON format. The editor's agreed requirements are:

1. Exactly one start platform and one finish platform.
2. At least one checkpoint.
3. At least 10 gameplay objects total. Start, finish, checkpoints, platforms, and traps **all count**; decorations do not.
4. Valid placement, map boundaries, 90-degree rotations, and a playable route according to the editor's validator.

Revalidate when editing and before saving. **Always allow save**, even if validation fails, and write `"isValid": false`. The example file is deliberately marked false; its example coordinates have not been certified playable.

The PC app may import invalid levels for inspection but must **not** start a multiplayer race on one. On import, also verify the schema version, parseability, required objects, unique IDs, and known prefab IDs; `isValid` is a status flag, not a security guarantee.

## Unity example

```csharp
// VR editor: assign isValid from the editor's validation result before saving.
string json = LevelJson.Serialize(levelData);
System.IO.File.WriteAllText(filePath, json);

// PC: read, validate basic structure, then instantiate matching prefabs.
LevelData imported = LevelJson.Deserialize(System.IO.File.ReadAllText(filePath));
if (!imported.isValid) {
    Debug.LogWarning("Level is a work in progress; multiplayer race disabled.");
}
```

In multiplayer, the host/server should choose the level and ensure all clients load the same data. Runtime network synchronization and prefab spawning are implemented in the PC project, not in this file format.
