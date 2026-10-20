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
            m_NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }

    }

    private void CheckApprovalCallback(
    ConnectionApprovalRequest request,
    ConnectionApprovalResponse response)
    {
        Debug.Log(
            $"[Approval] Callback called for client {request.ClientNetworkId}"
        );
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
        LobbySpawnManager spawnManager = LobbySpawnManager.Instance;
        Debug.Log(
            $"[Spawn] Manager found: {LobbySpawnManager.Instance != null}"
        );


        if (spawnManager != null)
        {
            Transform spawn = spawnManager.GetSpawnPoint(request.ClientNetworkId);

            if (spawn != null)
            {
                response.Position = spawn.position;
                response.Rotation = spawn.rotation;

                Debug.Log(
                    $"[Spawn] Client {request.ClientNetworkId} " +
                    $"spawning at {response.Position}"
                );
            }
        }
        else
        {
            // Lobby scene hasn't loaded yet.
            // Use a temporary position until the Lobby teleport runs.
            response.Position = new Vector3(0, 3, 0);
        }
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
        if (m_NetworkManager.IsServer &&
            LobbySpawnManager.Instance != null)
        {
            LobbySpawnManager.Instance.ReleaseSpawnPoint(clientID);
        }

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
        m_NetworkManager.OnClientConnectedCallback -= OnClientConnected;
    }
    public void SetGameInLobby()
    {
        if (m_NetworkManager == null || !m_NetworkManager.IsServer)
            return;

        _gameLaunched = false;

        Debug.Log("[Lobby] New connections are allowed again.");
    }
    private void OnClientConnected(ulong clientId)
    {
        if (!m_NetworkManager.IsServer)
            return;

        LobbySpawnManager spawnManager =
            LobbySpawnManager.Instance;

        if (spawnManager == null)
            return;

        spawnManager.PositionNewPlayer(clientId);
    }
}