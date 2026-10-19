using Gameplay;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SelectMapButton : NetworkBehaviour
{
    public string jsonPath;
    
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

    private void SelectMap()
    {
        if (!IsServer)
            return;
        
        jsonPath = EditorUtility.OpenFilePanel("", "Assets", ".json");
        TextAsset jsonData = Resources.Load<TextAsset>(jsonPath);
        if (jsonData == null)
            return;
        LevelManager.Instance.SetLevelData(LevelJson.Deserialize(jsonData.text));
    }
}