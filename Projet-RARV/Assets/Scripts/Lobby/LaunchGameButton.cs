using Gameplay;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LaunchGameButton : NetworkBehaviour
{
    public string GameSceneName = "Laboratory";

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            GetComponentInParent<Canvas>().gameObject.SetActive(false);
            return;
        }
        GetComponent<Button>().onClick.AddListener(LaunchGame);
    }

    private void LaunchGame()
    {
        if (!IsServer)
            return;
        
        ConnectionApprovalHandler approvalHandler = NetworkManager.Singleton.GetComponent<ConnectionApprovalHandler>();

        if (approvalHandler != null)
        {
            approvalHandler.SetGameLaunched();
        }
        // Launch the actual game
        NetworkManager.SceneManager.LoadScene(
            GameSceneName,
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }
}