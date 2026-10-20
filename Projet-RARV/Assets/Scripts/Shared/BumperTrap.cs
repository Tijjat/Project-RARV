
using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

public class BumperTrap : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float upwardForce = 3f;

    [Header("Timing")]
    [SerializeField] private float cooldown = 1f;

    [Header("Paddles")]
    [SerializeField] private BumperPaddle[] paddles;

    [Header("Paddle Movement")]
    [SerializeField] private Vector3 movementDirection = Vector3.forward;
    [SerializeField] private float extensionDistance = 1.5f;
    [SerializeField] private float extensionDuration = 0.2f;
    [SerializeField] private float retractionDuration = 0.3f;

    private readonly Dictionary<PlayerMovementController, float>
        lastHitTimes = new();

    private void Start()
    {
        foreach (BumperPaddle paddle in paddles)
        {
            if (paddle != null)
                paddle.Initialize(this);
        }

        StartCoroutine(BumperCycle());
    }

    public void Configure(
        float newPushForce,
        float newUpwardForce,
        float newCooldown)
    {
        pushForce = Mathf.Max(0f, newPushForce);
        upwardForce = Mathf.Max(0f, newUpwardForce);
        cooldown = Mathf.Max(0f, newCooldown);
    }

    private IEnumerator BumperCycle()
    {
        while (true)
        {
            // Extend and retract all paddles simultaneously
            foreach (BumperPaddle paddle in paddles)
            {
                if (paddle != null)
                {
                    StartCoroutine(paddle.Animate(
                        movementDirection,
                        extensionDistance,
                        extensionDuration,
                        retractionDuration
                    ));
                }
            }

            // Wait until all paddles have fully retracted
            yield return new WaitForSeconds(
                extensionDuration + retractionDuration
            );

            // Wait before activating again
            yield return new WaitForSeconds(cooldown);
        }
    }

    public void TryHitPlayer(
    PlayerMovementController player,
    BumperPaddle paddle)
    {
        if (lastHitTimes.TryGetValue(player, out float lastHit) &&
            Time.time - lastHit < cooldown)
            return;

        // Push in the direction the paddle is extending
        Vector3 direction =
            paddle.transform.parent.TransformDirection(movementDirection);

        direction.y = 0f;
        direction.Normalize();

        Vector3 knockback =
            direction * pushForce +
            Vector3.up * upwardForce;

        player.ApplyKnockback(knockback);

        lastHitTimes[player] = Time.time;
    }
}
