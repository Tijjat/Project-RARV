using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    public bool IsOccupied { get; set; }

    public Transform SnapTransform => transform;
}