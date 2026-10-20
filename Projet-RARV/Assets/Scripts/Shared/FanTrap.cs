
using Unity.Netcode;
using UnityEngine;

public class FanTrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform propeller;
    [SerializeField] private ParticleSystem windParticles;

    [Header("Sweep")]
    [SerializeField] private float sweepAngle = 45f;
    [SerializeField] private float sweepSpeed = 45f;

    [Header("Propeller")]
    [SerializeField] private Vector3 propellerAxis = Vector3.forward;
    [SerializeField] private float propellerSpeed = 720f;

    [Header("Wind")]
    [SerializeField] private float windForce = 30f;

    [Header("Cycle")]
    [SerializeField] private float activeDuration = 5f;
    [SerializeField] private float cooldownDuration = 1f;

    private Quaternion initialYawRotation;
    private Quaternion initialPropellerRotation;

    public float WindForce => windForce;
    public bool IsActive { get; private set; }

    private void Awake()
    {
        if (yawPivot != null)
            initialYawRotation = yawPivot.localRotation;

        if (propeller != null)
            initialPropellerRotation = propeller.localRotation;
    }

    public void Configure(
        float newWindForce,
        float newSweepAngle,
        float newSweepSpeed,
        float newActiveDuration,
        float newCooldownDuration)
    {
        windForce = Mathf.Max(0f, newWindForce);
        sweepAngle = Mathf.Clamp(newSweepAngle, 0f, 90f);
        sweepSpeed = Mathf.Max(0f, newSweepSpeed);
        activeDuration = Mathf.Max(0.01f, newActiveDuration);
        cooldownDuration = Mathf.Max(0f, newCooldownDuration);
    }

    private double GetSynchronizedTime()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            return NetworkManager.Singleton.ServerTime.Time;
        }

        return Time.timeAsDouble;
    }

    private void Update()
    {
        double time = GetSynchronizedTime();

        float cycleDuration =
            activeDuration + cooldownDuration;

        float cycleTime =
            (float)(time % cycleDuration);

        IsActive = cycleTime < activeDuration;

        // Only advance animation time during active periods.
        double completedCycles = System.Math.Floor(
            time / cycleDuration
        );

        double animationTime =
            completedCycles * activeDuration +
            System.Math.Min(cycleTime, activeDuration);

        if (yawPivot != null)
        {
            float angle = Mathf.PingPong(
                (float)(animationTime * sweepSpeed),
                sweepAngle * 2f
            ) - sweepAngle;

            yawPivot.localRotation =
                initialYawRotation *
                Quaternion.Euler(0f, angle, 0f);
        }

        if (propeller != null)
        {
            float rotation =
                (float)(animationTime * propellerSpeed % 360.0);

            propeller.localRotation =
                initialPropellerRotation *
                Quaternion.AngleAxis(
                    rotation,
                    propellerAxis.normalized
                );
        }

        if (windParticles != null)
        {
            if (IsActive && !windParticles.isEmitting)
                windParticles.Play();

            else if (!IsActive && windParticles.isEmitting)
                windParticles.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmitting
                );
        }
    }
}
