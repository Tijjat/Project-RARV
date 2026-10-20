
using Gameplay;
using UnityEngine;

public class TrampolineTrap : MonoBehaviour
{
    [Header("Trampoline Settings")]
    [SerializeField] private float launchForce = 12f;
    [SerializeField] private float cooldown = 0.5f;

    private readonly System.Collections.Generic.Dictionary<
        PlayerMovementController, float> lastBounceTimes = new();

    public void Configure(float newLaunchForce, float newCooldown)
    {
        launchForce = Mathf.Max(0f, newLaunchForce);
        cooldown = Mathf.Max(0f, newCooldown);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Trampoline] Trigger entered by: {other.name}");

        TryBounce(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryBounce(other);
    }

    private void TryBounce(Collider other)
    {
        PlayerMovementController player =
            other.GetComponentInParent<PlayerMovementController>();

        if (player == null || !player.IsOwner)
            return;

        if (lastBounceTimes.TryGetValue(player, out float lastTime) &&
            Time.time - lastTime < cooldown)
            return;

        lastBounceTimes[player] = Time.time;
        player.Bounce(launchForce);
    }
}
