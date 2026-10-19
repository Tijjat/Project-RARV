
using Gameplay;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LaunchGameButton : NetworkBehaviour
{
    public string GameSceneName = "Game";

    [SerializeField] private GameLevelLauncher gameLevelLauncher;

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

        // Find the persistent LevelManager automatically
        if (gameLevelLauncher == null)
        {
            gameLevelLauncher =
                FindFirstObjectByType<GameLevelLauncher>();
        }

        if (gameLevelLauncher == null)
        {
            Debug.LogError(
                "GameLevelLauncher not found in the scene!"
            );
            return;
        }

        if (!gameLevelLauncher.PrepareLevel())
            return;

        ConnectionApprovalHandler approvalHandler =
            NetworkManager.Singleton
                .GetComponent<ConnectionApprovalHandler>();

        if (approvalHandler != null)
        {
            approvalHandler.SetGameLaunched();
        }

        NetworkManager.SceneManager.LoadScene(
            GameSceneName,
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }
}
