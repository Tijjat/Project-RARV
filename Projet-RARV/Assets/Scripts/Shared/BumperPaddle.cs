
using Gameplay;
using System.Collections;
using UnityEngine;

public class BumperPaddle : MonoBehaviour
{
    private Vector3 initialPosition;
    private BumperTrap bumperTrap;
    private bool isExtending;

    public void Initialize(BumperTrap trap)
    {
        bumperTrap = trap;
    }
    private void Awake()
    {
        initialPosition = transform.localPosition;
    }

    public IEnumerator Animate(
        Vector3 direction,
        float distance,
        float extensionDuration,
        float retractionDuration)
    {
        Vector3 targetPosition =
            initialPosition + direction.normalized * distance;

        // Extend
        isExtending = true;
        yield return MoveTo(targetPosition, extensionDuration);

        // Retract
        isExtending = false;
        yield return MoveTo(initialPosition, retractionDuration);

        transform.localPosition = initialPosition;

        // Ensure exact starting position
        transform.localPosition = initialPosition;
    }

    private IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localPosition;

        if (duration <= 0f)
        {
            transform.localPosition = target;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            transform.localPosition = Vector3.Lerp(
                start,
                target,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        transform.localPosition = target;
    }
    public Vector3 GetPushDirection(Vector3 playerPosition)
    {
        Vector3 direction = playerPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        return direction.normalized;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!isExtending || bumperTrap == null)
            return;

        PlayerMovementController player =
            other.GetComponentInParent<PlayerMovementController>();

        if (player == null || !player.IsOwner)
            return;

        bumperTrap.TryHitPlayer(player, this);
    }
}
