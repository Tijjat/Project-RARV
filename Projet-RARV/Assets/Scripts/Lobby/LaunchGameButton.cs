
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LaunchGameButton : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private GameLevelLauncher gameLevelLauncher;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(LaunchGame);

        Debug.Log("[Launch] Button listener registered.");
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(LaunchGame);
    }

    private void LaunchGame()
    {
        Debug.Log("[Launch] Button clicked.");

        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
            return;

        if (gameLevelLauncher == null)
        {
            gameLevelLauncher =
                FindAnyObjectByType<GameLevelLauncher>();
        }

        if (gameLevelLauncher == null)
        {
            Debug.LogError("GameLevelLauncher not found.");
            return;
        }

        if (!gameLevelLauncher.PrepareLevel())
        {
            Debug.LogError("Failed to prepare selected map.");
            return;
        }

        ConnectionApprovalHandler approvalHandler =
            NetworkManager.Singleton
                .GetComponent<ConnectionApprovalHandler>();

        if (approvalHandler != null)
            approvalHandler.SetGameLaunched();

        var status = NetworkManager.Singleton.SceneManager.LoadScene(
            gameSceneName,
            LoadSceneMode.Single
        );

        Debug.Log($"[Launch] Scene load status: {status}");
    }
}
