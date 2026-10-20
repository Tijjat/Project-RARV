
using System.IO;
using Gameplay;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SelectMapButton : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text mapNameText;

    [Header("Selected Map")]
    [SerializeField] private string jsonPath;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            GetComponentInParent<Canvas>().gameObject.SetActive(false);
            return;
        }

        GetComponent<Button>().onClick.AddListener(SelectMap);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            GetComponent<Button>().onClick.RemoveListener(SelectMap);

        base.OnNetworkDespawn();
    }

    private void SelectMap()
    {
        if (!IsServer)
            return;

        FileBrowser.SetFilters(
            true,
            new FileBrowser.Filter("JSON Maps", ".json")
        );

        FileBrowser.SetDefaultFilter(".json");

        FileBrowser.ShowLoadDialog(
            paths =>
            {
                if (paths.Length > 0)
                    LoadSelectedMap(paths[0]);
            },
            () => Debug.Log("Map selection cancelled"),
            FileBrowser.PickMode.Files,
            false,
            null,
            null,
            "Select Factory Frenzy Map",
            "Load Map"
        );
    }

    private void LoadSelectedMap(string path)
    {
        try
        {
            string json = File.ReadAllText(path);

            LevelData level = LevelJson.Deserialize(json);

            if (level == null)
            {
                Debug.LogError("Failed to deserialize map.");
                return;
            }

            if (LevelManager.Instance == null)
            {
                Debug.LogError("LevelManager not found.");
                return;
            }

            // Store the selected map for the launch process.
            LevelManager.Instance.SetLevelData(level);

            jsonPath = path;

            // Display the name from the JSON.
            if (mapNameText != null)
                mapNameText.text = level.levelName;

            Debug.Log(
                $"Selected map: {level.levelName}"
            );
        }
        catch (System.Exception exception)
        {
            Debug.LogError(
                $"Failed to load selected map: {exception.Message}"
            );
        }
    }
}
