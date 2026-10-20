using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Collections;
using static UnityEngine.InputSystem.DefaultInputActions;
using UnityEngine.SceneManagement;

namespace Gameplay
{
    public class PlayerData : NetworkBehaviour
    {
        public PlayerInput PlayerInput;
        public GameObject Camera;
        public PlayerMovementController PlayerMovementController;
        public GameObject PlayerCanvas;
        public GameObject PlayerFollowCamera;
        public NetworkVariable<int> PlayerRank = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        public NetworkVariable<double> PlayerTime = new NetworkVariable<double>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        [SerializeField] private GameObject PseudoCanvas;
        [SerializeField] private GameObject endGamePanel;
        
        public override void OnNetworkSpawn()
        {
            PlayerInput.enabled = IsOwner;
            Camera.SetActive(IsOwner);
            PlayerFollowCamera.SetActive(IsOwner);
            PlayerMovementController.enabled = IsOwner;

            PlayerCanvas.SetActive(IsOwner);
            PseudoCanvas.SetActive(!IsOwner);

            // Hide the entire leaderboard, including its border.
            if (endGamePanel != null)
                endGamePanel.SetActive(false);

            if (IsOwner)
            {
                SetPseudoServerRpc(UI.MainMenu.PendingPseudo);
            }
            if (IsOwner)
            {
                Debug.Log(
                    $"[InitialSpawn] Owner {OwnerClientId} " +
                    $"spawned at {transform.position}"
                );
            }
        }

        [ClientRpc]
        public void TeleportClientRpc(
            Vector3 position,
            Quaternion rotation,
            ClientRpcParams rpcParams = default)
        {
            if (!IsOwner)
                return;

            CharacterController controller =
                GetComponent<CharacterController>();

            PlayerMovementController movement =
                GetComponent<PlayerMovementController>();

            // Disable collisions during teleportation
            if (controller != null)
                controller.enabled = false;

            // Move player to the assigned spawn point
            transform.SetPositionAndRotation(position, rotation);

            // Clear gravity, knockback, and previous movement state
            if (movement != null)
                movement.ResetMovementState();

            // Restore collisions
            if (controller != null)
                controller.enabled = true;

            Debug.Log($"[Teleport] Player {OwnerClientId} moved to {position}");
        }

        public NetworkVariable<FixedString64Bytes> PlayerPseudo =
            new NetworkVariable<FixedString64Bytes>(
                new FixedString64Bytes("Player"),
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

        [ServerRpc]
        public void SetPseudoServerRpc(string pseudo)
        {
            if (string.IsNullOrWhiteSpace(pseudo))
                pseudo = "Player";

            if (pseudo.Length > 60)
                pseudo = pseudo.Substring(0, 60);

            PlayerPseudo.Value = new FixedString64Bytes(pseudo);
        }
        
        public void SetTime(double time)
        {
            if (!IsServer)
                return;

            PlayerTime.Value = time - GameManager.Instance.StartTime.Value;
        }
        public void HideLeaderboard()
        {
            if (endGamePanel != null)
                endGamePanel.SetActive(false);
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Lobby")
                HideLeaderboard();
        }
    }
}