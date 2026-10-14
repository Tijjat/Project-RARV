using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    public class PlayerGameStateController : NetworkBehaviour
    {
        private PlayerMovementController _movement;
        private bool _movementDisabled;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovementController>();
        }

        private void Update()
        {
            if (!IsOwner || _movementDisabled)
                return;

            if (GameManager.Instance == null ||
                !GameManager.Instance.IsSpawned)
                return;

            if (!GameManager.Instance.GameEnded.Value)
                return;

            _movementDisabled = true;

            if (_movement != null)
                _movement.enabled = false;
        }
    }
}