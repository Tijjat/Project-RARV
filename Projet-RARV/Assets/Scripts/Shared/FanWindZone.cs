
using Gameplay;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class FanWindZone : MonoBehaviour
{
    [SerializeField] private FanTrap fanTrap;
    [SerializeField] private Transform windOrigin;

    private void OnTriggerStay(Collider other)
    {
        PlayerMovementController player =
            other.GetComponentInParent<PlayerMovementController>();

        if (player == null || !player.IsOwner ||
        fanTrap == null || !fanTrap.IsActive ||
        windOrigin == null)
                return;

        Vector3 direction = windOrigin.forward.normalized;

        player.ApplyWindForce(
            direction * fanTrap.WindForce
        );
    }
}
