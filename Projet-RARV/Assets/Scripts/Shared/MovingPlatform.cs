
using Unity.Netcode;
using UnityEngine;

public class MovingPlatform : NetworkBehaviour
{
    [Header("Default Movement Settings")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float pauseAtEnds = 0.5f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float progress;
    private float pauseTimer;
    private int direction = 1;
    private Vector3 previousPosition;

    public Vector3 DeltaPosition { get; private set; }
    // Called by LevelMapLoader before NetworkObject.Spawn()
    public void Configure(
        float newDistance,
        float newSpeed,
        float newPauseAtEnds)
    {
        distance = Mathf.Max(0f, newDistance);
        speed = Mathf.Max(0.01f, newSpeed);
        pauseAtEnds = Mathf.Max(0f, newPauseAtEnds);
    }

    public override void OnNetworkSpawn()
    {
        Vector3 centerPosition = transform.position;

        // Move along the platform's local X axis
        startPosition = centerPosition - transform.right * distance;
        endPosition = centerPosition + transform.right * distance;

        // Start in the middle
        progress = 0.5f;
        direction = 1;

        previousPosition = transform.position;
        DeltaPosition = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!IsSpawned)
            return;

        if (IsServer)
        {
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.fixedDeltaTime;
            }
            else
            {
                float pathLength = Mathf.Max(
                    Vector3.Distance(startPosition, endPosition),
                    0.001f
                );

                progress += direction *
                    speed / pathLength * Time.fixedDeltaTime;

                if (progress >= 1f)
                {
                    progress = 1f;
                    direction = -1;
                    pauseTimer = pauseAtEnds;
                }
                else if (progress <= 0f)
                {
                    progress = 0f;
                    direction = 1;
                    pauseTimer = pauseAtEnds;
                }

                transform.position = Vector3.Lerp(
                    startPosition,
                    endPosition,
                    progress
                );
            }
        }

        // Calculate movement on both server and clients
        DeltaPosition = transform.position - previousPosition;
        previousPosition = transform.position;
    }
}
