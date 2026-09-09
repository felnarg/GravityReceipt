using UnityEngine;

namespace GravityReceipt.Player
{
    public enum LocalPlayerSlot
    {
        One = 0,
        Two = 1
    }

    /// <summary>
    /// Input local por jugador (teclado + un pad). P1 no comparte flechas con P2.
    /// </summary>
    public sealed class LocalPlayerInput : MonoBehaviour
    {
        [SerializeField] private LocalPlayerSlot slot = LocalPlayerSlot.One;
        [SerializeField] private bool mouseLook = true;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float keyLookDegreesPerSecond = 110f;

        public LocalPlayerSlot Slot => slot;
        public bool UsesMouseLook => mouseLook;
        public Camera PlayerCamera => playerCamera;

        public void Configure(LocalPlayerSlot playerSlot, bool useMouseLook, Camera camera)
        {
            slot = playerSlot;
            mouseLook = useMouseLook;
            playerCamera = camera;
        }

        public Vector2 MoveAxes()
        {
            float x;
            float z;
            if (slot == LocalPlayerSlot.One)
            {
                x = KeyAxis(KeyCode.A, KeyCode.D) + SafeAxis("P1JoyX");
                z = KeyAxis(KeyCode.S, KeyCode.W) + SafeAxis("P1JoyY");
            }
            else
            {
                x = KeyAxis(KeyCode.LeftArrow, KeyCode.RightArrow) + SafeAxis("P2JoyX");
                z = KeyAxis(KeyCode.DownArrow, KeyCode.UpArrow) + SafeAxis("P2JoyY");
            }

            var v = new Vector2(x, z);
            return v.sqrMagnitude > 1f ? v.normalized : v;
        }

        public Vector2 LookDelta()
        {
            if (mouseLook && Cursor.lockState == CursorLockMode.Locked)
            {
                return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            }

            var yaw = 0f;
            var pitch = 0f;
            if (slot == LocalPlayerSlot.Two)
            {
                yaw = KeyAxis(KeyCode.Keypad4, KeyCode.Keypad6)
                      + KeyAxis(KeyCode.U, KeyCode.O)
                      + KeyAxis(KeyCode.J, KeyCode.L);
                pitch = KeyAxis(KeyCode.Keypad5, KeyCode.Keypad8)
                        + KeyAxis(KeyCode.H, KeyCode.Y)
                        + KeyAxis(KeyCode.K, KeyCode.I);
                yaw += SafeAxis("P2LookX");
                pitch += SafeAxis("P2LookY");
            }

            return new Vector2(yaw, pitch) * keyLookDegreesPerSecond * Time.deltaTime;
        }

        public bool JumpPressed()
        {
            return slot == LocalPlayerSlot.One
                ? Input.GetKeyDown(KeyCode.Space)
                : Input.GetKeyDown(KeyCode.RightControl)
                  || Input.GetKeyDown(KeyCode.KeypadEnter)
                  || Input.GetKeyDown(KeyCode.Joystick1Button0);
        }

        public bool GrabHeld()
        {
            return slot == LocalPlayerSlot.One
                ? Input.GetKey(KeyCode.E) || Input.GetMouseButton(0)
                : Input.GetKey(KeyCode.RightShift)
                  || Input.GetKey(KeyCode.KeypadPeriod)
                  || Input.GetKey(KeyCode.Joystick1Button1);
        }

        public bool DropPressed()
        {
            return slot == LocalPlayerSlot.One
                ? Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)
                : Input.GetKeyDown(KeyCode.RightShift)
                  || Input.GetKeyDown(KeyCode.KeypadPeriod)
                  || Input.GetKeyDown(KeyCode.Joystick1Button1);
        }

        public bool PingPressed()
        {
            return slot == LocalPlayerSlot.One
                ? Input.GetKeyDown(KeyCode.Q)
                : Input.GetKeyDown(KeyCode.Slash) || Input.GetKeyDown(KeyCode.Joystick1Button2);
        }

        public bool SprintHeld()
        {
            return slot == LocalPlayerSlot.One
                ? Input.GetKey(KeyCode.LeftShift)
                : Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.Joystick1Button4);
        }

        public bool AbilityPressed()
        {
            return slot == LocalPlayerSlot.One
                ? Input.GetKeyDown(KeyCode.F)
                : Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Joystick1Button5);
        }

        public bool CycleRolePressed()
        {
            return slot == LocalPlayerSlot.One
                ? Input.GetKeyDown(KeyCode.Tab)
                : Input.GetKeyDown(KeyCode.Keypad7);
        }

        /// <summary>
        /// 4 emotes. P1: 1–4. P2: KP1 / KP2 / KP3 / KP9 (evita teclas de mirada).
        /// </summary>
        public int EmotePressed()
        {
            if (slot == LocalPlayerSlot.One)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) return 0;
                if (Input.GetKeyDown(KeyCode.Alpha2)) return 1;
                if (Input.GetKeyDown(KeyCode.Alpha3)) return 2;
                if (Input.GetKeyDown(KeyCode.Alpha4)) return 3;
                return -1;
            }

            if (Input.GetKeyDown(KeyCode.Keypad1)) return 0;
            if (Input.GetKeyDown(KeyCode.Keypad2)) return 1;
            if (Input.GetKeyDown(KeyCode.Keypad3)) return 2;
            if (Input.GetKeyDown(KeyCode.Keypad9)) return 3;
            return -1;
        }

        private static float KeyAxis(KeyCode negative, KeyCode positive)
        {
            var v = 0f;
            if (Input.GetKey(negative))
            {
                v -= 1f;
            }

            if (Input.GetKey(positive))
            {
                v += 1f;
            }

            return v;
        }

        private static float SafeAxis(string name)
        {
            try
            {
                return Input.GetAxisRaw(name);
            }
            catch (System.ArgumentException)
            {
                return 0f;
            }
        }
    }
}
