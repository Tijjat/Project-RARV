
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReturnToLobbyButton : MonoBehaviour
{
    [SerializeField] private string lobbySceneName = "Lobby";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ReturnToLobby);
    }

    private void Start()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        // Only the host sees this button.
        gameObject.SetActive(
            networkManager != null && networkManager.IsServer
        );
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(ReturnToLobby);
    }

    private void ReturnToLobby()
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null || !networkManager.IsServer)
            return;

        var status = networkManager.SceneManager.LoadScene(
            lobbySceneName,
            LoadSceneMode.Single
        );

        Debug.Log($"[Lobby] Scene load status: {status}");

        if (status != SceneEventProgressStatus.Started)
            return;

        ConnectionApprovalHandler approvalHandler =
            networkManager.GetComponent<ConnectionApprovalHandler>();

        if (approvalHandler != null)
            approvalHandler.SetGameInLobby();
    }
}
