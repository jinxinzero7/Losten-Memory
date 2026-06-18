using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public static class GameInput
{
    public static bool InteractPressed => Keyboard.current?.eKey.wasPressedThisFrame == true;
    public static bool CancelPressed => Keyboard.current?.escapeKey.wasPressedThisFrame == true;
    public static bool SolvePuzzlePressed => Keyboard.current?.tKey.wasPressedThisFrame == true;
    public static bool ManualSavePressed => Keyboard.current?.f5Key.wasPressedThisFrame == true;
    public static bool PrimaryClickPressed => Mouse.current?.leftButton.wasPressedThisFrame == true;

    public static Vector2 Movement
    {
        get
        {
            Vector2 movement = Vector2.zero;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                movement.x = ReadAxis(keyboard.aKey, keyboard.leftArrowKey, keyboard.dKey, keyboard.rightArrowKey);
                movement.y = ReadAxis(keyboard.sKey, keyboard.downArrowKey, keyboard.wKey, keyboard.upArrowKey);
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > movement.sqrMagnitude)
                {
                    movement = stick;
                }
            }

            return Vector2.ClampMagnitude(movement, 1f);
        }
    }

    public static void ConfigureUiInput()
    {
        foreach (EventSystem eventSystem in Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include))
        {
            foreach (StandaloneInputModule legacyModule in eventSystem.GetComponents<StandaloneInputModule>())
            {
                legacyModule.enabled = false;
            }

            InputSystemUIInputModule[] inputSystemModules = eventSystem.GetComponents<InputSystemUIInputModule>();
            InputSystemUIInputModule activeModule;
            if (inputSystemModules.Length == 0)
            {
                activeModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                activeModule.AssignDefaultActions();
            }
            else
            {
                activeModule = inputSystemModules[0];
            }

            for (int i = 0; i < inputSystemModules.Length; i++)
            {
                inputSystemModules[i].enabled = inputSystemModules[i] == activeModule;
            }

            activeModule.enabled = true;
        }
    }

    private static float ReadAxis(KeyControl negativePrimary, KeyControl negativeAlternate, KeyControl positivePrimary, KeyControl positiveAlternate)
    {
        float negative = negativePrimary.isPressed || negativeAlternate.isPressed ? 1f : 0f;
        float positive = positivePrimary.isPressed || positiveAlternate.isPressed ? 1f : 0f;
        return positive - negative;
    }
}
