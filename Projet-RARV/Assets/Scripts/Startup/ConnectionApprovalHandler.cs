using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Netcode.NetworkManager;

public class ConnectionApprovalHandler : MonoBehaviour
{
    private NetworkManager m_NetworkManager;

    public int MaxNumberOfPlayers = 10;

    private bool _gameLaunched = false;

    private void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();

        if (MaxNumberOfPlayers < 1)
            MaxNumberOfPlayers = 1;

        if (m_NetworkManager != null)
        {
            m_NetworkManager.ConnectionApprovalCallback += CheckApprovalCallback;
            m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }
    }

    private void CheckApprovalCallback(
    ConnectionApprovalRequest request,
    ConnectionApprovalResponse response)
    {
        if (_gameLaunched)
        {
            Debug.LogWarning(
                "[Connection Rejected] A client tried to join, but the game has already started!"
            );

            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Reason = "The game has already started!";
            return;
        }

        if (m_NetworkManager.ConnectedClientsIds.Count >= MaxNumberOfPlayers)
        {
            Debug.LogWarning(
                $"[Connection Rejected] Lobby is full ({MaxNumberOfPlayers} players maximum)."
            );

            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Reason = "Too many players in lobby!";
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Position = new Vector3(0, 3, 0);
    }

    public void SetGameLaunched()
    {
        if (m_NetworkManager != null && m_NetworkManager.IsServer)
        {
            _gameLaunched = true;
            Debug.Log("Game launched: new connections are now blocked.");
        }
    }

    private void OnClientDisconnectCallback(ulong clientID)
    {
        if (!m_NetworkManager.IsServer &&
            !string.IsNullOrEmpty(m_NetworkManager.DisconnectReason))
        {
            Debug.Log(
                $"Connection declined: {m_NetworkManager.DisconnectReason}"
            );
        }
    }

    private void OnDestroy()
    {
        if (m_NetworkManager == null)
            return;

        m_NetworkManager.ConnectionApprovalCallback -= CheckApprovalCallback;
        m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }
}