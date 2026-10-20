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
            if (!IsOwner)
                return;

            if (GameManager.Instance == null ||
                !GameManager.Instance.IsSpawned ||
                (!GameManager.Instance.GameEnded.Value && 
                 GameManager.Instance.GameStarted.Value)) {
                    _movementDisabled = false;
                    if (_movement != null)
                        _movement.enabled = true;
                    return;
            }

            _movementDisabled = true;

            if (_movement != null)
                _movement.enabled = false;
        }
    }
}