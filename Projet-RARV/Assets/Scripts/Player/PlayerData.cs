using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Collections;
using static UnityEngine.InputSystem.DefaultInputActions;

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

        public override void OnNetworkSpawn()
        {
            PlayerInput.enabled = IsOwner;
            Camera.SetActive(IsOwner);
            PlayerFollowCamera.SetActive(IsOwner);
            PlayerMovementController.enabled = IsOwner;
            PlayerCanvas.SetActive(IsOwner);
            PseudoCanvas.SetActive(!IsOwner);
            if (IsOwner)
            {
                SetPseudoServerRpc(
                    UI.MainMenu.PendingPseudo
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

            if (controller != null)
                controller.enabled = false;

            transform.SetPositionAndRotation(position, rotation);

            if (controller != null)
                controller.enabled = true;
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
    }
}