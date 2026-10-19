using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    private bool cursorUnlocked = false;
    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            cursorUnlocked = !cursorUnlocked;

            if (cursorUnlocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}