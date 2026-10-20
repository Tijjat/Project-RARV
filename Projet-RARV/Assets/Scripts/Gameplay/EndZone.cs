using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    [RequireComponent(typeof(BoxCollider))]
    public class EndZone : NetworkBehaviour
    {
        public static EndZone Instance { get; private set; }

        private BoxCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            Instance = this;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
                Instance = null;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (GameManager.Instance.IsGameActive && other.gameObject.GetComponent<PlayerData>() != null)
            {
                double time = NetworkManager.Singleton.ServerTime.Time;
                TryArrival(other.gameObject.GetComponent<PlayerData>(), time);
            }

            return;
        }

        public bool ContainsPlayer(Vector3 position)
        {
            // Convert the player's position into collider local space.
            Vector3 localPosition =
                _collider.transform.InverseTransformPoint(position);

            Bounds bounds = new Bounds(
                _collider.center,
                _collider.size
            );

            return bounds.Contains(localPosition);
        }

        public bool TryArrival(PlayerData player, double time)
        {
            if (player == null)
                return false;

            if (!player.IsSpawned)
                return false;
            
            player.SetTime(time);
            if (!GameManager.Instance.FirstArrived.Value)
            {
                GameManager.Instance.EndTime.Value = NetworkManager.Singleton.ServerTime.Time + 10;
                GameManager.Instance.FirstArrived.Value = true;
            }

            return true;
        }
    }
}