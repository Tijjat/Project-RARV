using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private float matchDuration = 90f;

        public NetworkVariable<bool> GameStarted = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<bool> GameEnded = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<double> EndTime = new NetworkVariable<double>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public bool IsGameActive =>
            GameStarted.Value && !GameEnded.Value;

        public double RemainingTime
        {
            get
            {
                if (!GameStarted.Value || GameEnded.Value)
                    return 0;

                double currentTime =
                    NetworkManager.Singleton.ServerTime.Time;

                return System.Math.Max(0, EndTime.Value - currentTime);
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        public void StartGame()
        {
            if (!IsServer || GameStarted.Value)
                return;

            EndTime.Value =
                NetworkManager.ServerTime.Time + matchDuration;

            GameStarted.Value = true;

            Debug.Log($"Game started: {matchDuration} seconds remaining.");
        }

        private void Update()
        {
            if (!IsServer || !IsGameActive)
                return;

            if (NetworkManager.ServerTime.Time >= EndTime.Value)
                EndGame();
        }

        private void EndGame()
        {
            GameEnded.Value = true;

            Debug.Log("Game ended.");
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}