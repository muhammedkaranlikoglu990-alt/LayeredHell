// Unified input aggregator polling Keyboard, Mouse, Gamepad (Unity Input System) and MobileControls touch state.
using UnityEngine;
using UnityEngine.InputSystem;

namespace LayeredHell
{
    public static class InputRouter
    {
        public static Vector2 Move { get; private set; }
        public static Vector2 Look { get; private set; }
        public static bool FireHeld { get; private set; }
        public static bool FirePressed { get; private set; }
        public static bool AimHeld { get; private set; }
        public static bool ReloadPressed { get; private set; }
        public static bool InteractPressed { get; private set; }
        public static bool JumpPressed { get; private set; }
        public static bool CrouchPressed { get; private set; }
        public static bool SprintHeld { get; private set; }
        public static bool FlashlightPressed { get; private set; }
        public static bool MenuPressed { get; private set; }
        public static bool QuickHealPressed { get; private set; }
        public static bool NextWeaponPressed { get; private set; }

        // Touch state pushed by MobileControls
        private static Vector2 _touchMove;
        private static Vector2 _touchLookAccum;
        private static bool _touchFireHeld;
        private static bool _touchFirePressedPending;
        private static bool _touchAimHeld;
        private static bool _touchSprintHeld;
        private static bool _touchReloadPending;
        private static bool _touchInteractPending;
        private static bool _touchJumpPending;
        private static bool _touchCrouchPending;
        private static bool _touchFlashlightPending;
        private static bool _touchMenuPending;
        private static bool _touchQuickHealPending;
        private static bool _touchNextWeaponPending;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetFrameState()
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
            FireHeld = false;
            FirePressed = false;
            AimHeld = false;
            ReloadPressed = false;
            InteractPressed = false;
            JumpPressed = false;
            CrouchPressed = false;
            SprintHeld = false;
            FlashlightPressed = false;
            MenuPressed = false;
            QuickHealPressed = false;
            NextWeaponPressed = false;

            _touchMove = Vector2.zero;
            _touchLookAccum = Vector2.zero;
            _touchFireHeld = false;
            _touchFirePressedPending = false;
            _touchAimHeld = false;
            _touchSprintHeld = false;
            _touchReloadPending = false;
            _touchInteractPending = false;
            _touchJumpPending = false;
            _touchCrouchPending = false;
            _touchFlashlightPending = false;
            _touchMenuPending = false;
            _touchQuickHealPending = false;
            _touchNextWeaponPending = false;
        }

        public static void SetTouchMove(Vector2 move)
        {
            _touchMove = Vector2.ClampMagnitude(move, 1f);
        }

        public static void AddTouchLook(Vector2 deltaPixels)
        {
            _touchLookAccum += deltaPixels;
        }

        public static void SetTouchFire(bool held, bool pressedThisFrame = false)
        {
            _touchFireHeld = held;
            if (pressedThisFrame) _touchFirePressedPending = true;
        }

        public static void SetTouchAim(bool held)
        {
            _touchAimHeld = held;
        }

        public static void ToggleTouchAim()
        {
            _touchAimHeld = !_touchAimHeld;
        }

        public static void SetTouchSprint(bool held)
        {
            _touchSprintHeld = held;
        }

        public static void ToggleTouchSprint()
        {
            _touchSprintHeld = !_touchSprintHeld;
        }

        public static void TriggerTouchReload() { _touchReloadPending = true; }
        public static void TriggerTouchInteract() { _touchInteractPending = true; }
        public static void TriggerTouchJump() { _touchJumpPending = true; }
        public static void TriggerTouchCrouch() { _touchCrouchPending = true; }
        public static void TriggerTouchFlashlight() { _touchFlashlightPending = true; }
        public static void TriggerTouchMenu() { _touchMenuPending = true; }
        public static void TriggerTouchQuickHeal() { _touchQuickHealPending = true; }
        public static void TriggerTouchNextWeapon() { _touchNextWeaponPending = true; }

        internal static void PollDevices()
        {
            GameSettings.EnsureLoaded();

            Vector2 move = _touchMove;
            Vector2 look = _touchLookAccum * GameSettings.TouchSensitivity;
            _touchLookAccum = Vector2.zero;

            bool fireHeld = _touchFireHeld;
            bool firePressed = _touchFirePressedPending;
            bool aimHeld = _touchAimHeld;
            bool sprintHeld = _touchSprintHeld || (_touchMove.y > 0.92f);
            bool reloadPressed = _touchReloadPending;
            bool interactPressed = _touchInteractPending;
            bool jumpPressed = _touchJumpPending;
            bool crouchPressed = _touchCrouchPending;
            bool flashlightPressed = _touchFlashlightPending;
            bool menuPressed = _touchMenuPending;
            bool quickHealPressed = _touchQuickHealPending;
            bool nextWeaponPressed = _touchNextWeaponPending;

            _touchFirePressedPending = false;
            _touchReloadPending = false;
            _touchInteractPending = false;
            _touchJumpPending = false;
            _touchCrouchPending = false;
            _touchFlashlightPending = false;
            _touchMenuPending = false;
            _touchQuickHealPending = false;
            _touchNextWeaponPending = false;

            // Poll Keyboard
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                float kx = 0f;
                float ky = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) kx -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) kx += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) ky -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) ky += 1f;

                Vector2 kbMove = new Vector2(kx, ky);
                if (kbMove.sqrMagnitude > 0.001f)
                {
                    move += kbMove;
                }

                if (kb.leftShiftKey.isPressed) sprintHeld = true;
                if (kb.rKey.wasPressedThisFrame) reloadPressed = true;
                if (kb.eKey.wasPressedThisFrame || kb.fKey.wasPressedThisFrame) interactPressed = true;
                if (kb.spaceKey.wasPressedThisFrame) jumpPressed = true;
                if (kb.cKey.wasPressedThisFrame || kb.leftCtrlKey.wasPressedThisFrame) crouchPressed = true;
                if (kb.tKey.wasPressedThisFrame || kb.lKey.wasPressedThisFrame) flashlightPressed = true;
                if (kb.escapeKey.wasPressedThisFrame || kb.tabKey.wasPressedThisFrame || kb.iKey.wasPressedThisFrame) menuPressed = true;
                if (kb.hKey.wasPressedThisFrame || kb.vKey.wasPressedThisFrame) quickHealPressed = true;
                if (kb.qKey.wasPressedThisFrame || kb.digit1Key.wasPressedThisFrame || kb.digit2Key.wasPressedThisFrame || kb.digit3Key.wasPressedThisFrame) nextWeaponPressed = true;
            }

            // Poll Mouse only when no active touches are present (prevents touch-simulated mouse doubling)
            bool hasActiveTouches = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
            Mouse mouse = Mouse.current;
            if (mouse != null && !hasActiveTouches && !Application.isMobilePlatform)
            {
                Vector2 mDelta = mouse.delta.ReadValue();
                look += mDelta * (0.12f * GameSettings.LookSensitivity);

                if (mouse.leftButton.isPressed) fireHeld = true;
                if (mouse.leftButton.wasPressedThisFrame) firePressed = true;
                if (mouse.rightButton.isPressed) aimHeld = true;

                float scrollY = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scrollY) > 0.1f) nextWeaponPressed = true;
            }

            // Poll Gamepad
            Gamepad gp = Gamepad.current;
            if (gp != null)
            {
                Vector2 ls = gp.leftStick.ReadValue();
                if (ls.sqrMagnitude > 0.02f) move += ls;

                Vector2 rs = gp.rightStick.ReadValue();
                if (rs.sqrMagnitude > 0.02f)
                {
                    look += rs * (160f * Time.unscaledDeltaTime * GameSettings.LookSensitivity);
                }

                if (gp.rightTrigger.isPressed || gp.rightShoulder.isPressed) fireHeld = true;
                if (gp.rightTrigger.wasPressedThisFrame || gp.rightShoulder.wasPressedThisFrame) firePressed = true;
                if (gp.leftTrigger.isPressed) aimHeld = true;
                if (gp.leftStickButton.isPressed) sprintHeld = true;
                if (gp.buttonWest.wasPressedThisFrame) reloadPressed = true;
                if (gp.buttonSouth.wasPressedThisFrame) jumpPressed = true;
                if (gp.buttonEast.wasPressedThisFrame) crouchPressed = true;
                if (gp.buttonNorth.wasPressedThisFrame) nextWeaponPressed = true;
                if (gp.leftShoulder.wasPressedThisFrame) interactPressed = true;
                if (gp.dpad.up.wasPressedThisFrame) flashlightPressed = true;
                if (gp.dpad.down.wasPressedThisFrame) quickHealPressed = true;
                if (gp.startButton.wasPressedThisFrame || gp.selectButton.wasPressedThisFrame) menuPressed = true;
            }

            if (GameSettings.InvertY)
            {
                look.y = -look.y;
            }

            Move = Vector2.ClampMagnitude(move, 1f);
            Look = look;
            FireHeld = fireHeld;
            FirePressed = firePressed;
            AimHeld = aimHeld;
            ReloadPressed = reloadPressed;
            InteractPressed = interactPressed;
            JumpPressed = jumpPressed;
            CrouchPressed = crouchPressed;
            SprintHeld = sprintHeld;
            FlashlightPressed = flashlightPressed;
            MenuPressed = menuPressed;
            QuickHealPressed = quickHealPressed;
            NextWeaponPressed = nextWeaponPressed;
        }
    }

    [DefaultExecutionOrder(-100)]
    public sealed class InputRouterBehaviour : MonoBehaviour
    {
        private void Update()
        {
            InputRouter.PollDevices();
        }
    }
}
