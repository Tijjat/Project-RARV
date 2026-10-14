using UnityEngine;

public class PseudoBillboard : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;

        transform.rotation = cam.transform.rotation;
    }
}