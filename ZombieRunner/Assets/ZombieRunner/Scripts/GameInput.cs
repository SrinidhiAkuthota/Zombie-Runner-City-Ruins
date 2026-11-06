using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace ZombieRunner
{
    /// <summary>
    /// Keyboard + mouse + touch swipe input. Works with the legacy Input Manager
    /// and with the new Input System package, whichever the project uses.
    /// </summary>
    public static class GameInput
    {
        public static bool Left, Right, Jump, Slide, Confirm, Pause, Tap;

        static bool dragging;
        static Vector2 startPos;

        public static void Poll()
        {
            Left = Right = Jump = Slide = Confirm = Pause = Tap = false;
            bool down = false, up = false, held = false;
            Vector2 pos = Vector2.zero;

#if ENABLE_LEGACY_INPUT_MANAGER
            Left = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
            Right = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
            Jump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            Slide = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftControl);
            Confirm = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R);
            Pause = Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape);
            down = Input.GetMouseButtonDown(0);
            up = Input.GetMouseButtonUp(0);
            held = Input.GetMouseButton(0);
            pos = Input.mousePosition;
#elif ENABLE_INPUT_SYSTEM
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                Left = kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame;
                Right = kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame;
                Jump = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
                Slide = kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame || kb.leftCtrlKey.wasPressedThisFrame;
                Confirm = kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.rKey.wasPressedThisFrame;
                Pause = kb.pKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame;
            }
            Pointer p = Pointer.current;
            if (p != null)
            {
                down = p.press.wasPressedThisFrame;
                up = p.press.wasReleasedThisFrame;
                held = p.press.isPressed;
                pos = p.position.ReadValue();
            }
#endif

            // Swipe / tap detection (mouse drag or touch)
            float min = Mathf.Max(30f, Screen.height * 0.05f);
            if (down) { dragging = true; startPos = pos; }
            if (dragging && held)
            {
                Vector2 d = pos - startPos;
                if (d.magnitude > min)
                {
                    if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) { if (d.x > 0f) Right = true; else Left = true; }
                    else { if (d.y > 0f) Jump = true; else Slide = true; }
                    dragging = false;
                }
            }
            if (up)
            {
                if (dragging) { Tap = true; Confirm = true; }
                dragging = false;
            }
        }
    }
}
