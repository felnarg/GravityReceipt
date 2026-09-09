using GravityReceipt.Gravity;
using UnityEngine;

namespace GravityReceipt.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpSpeed = 7f;
        [SerializeField] private float mouseSensitivity = 1.5f;
        [SerializeField] private float groundCheckDistance = 0.35f;
        [SerializeField] private float alignSpeed = 10f;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private GravityManager gravityManager;

        private CharacterController _controller;
        private Vector3 _velocity;
        private float _pitch;
        private Vector3 _lastGravityDir = Vector3.down;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (gravityManager is null)
            {
                gravityManager = FindAnyObjectByType<GravityManager>();
            }

            if (cameraPivot is null && Camera.main is { } cam)
            {
                cameraPivot = cam.transform;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (gravityManager is not null)
            {
                GravityManager.GravityChanged += OnGravityChanged;
                _lastGravityDir = gravityManager.CurrentDirection;
            }
        }

        private void OnDestroy()
        {
            GravityManager.GravityChanged -= OnGravityChanged;
        }

        private void OnGravityChanged(Vector3 _, ValuableItem __)
        {
            // Inercia corta: conserva algo de velocidad lateral, corta la caída anterior.
            _velocity *= 0.35f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Look();
            Move();
        }

        private void Look()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            var mx = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            var my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
            transform.Rotate(0f, mx, 0f, Space.Self);
            _pitch = Mathf.Clamp(_pitch - my, -80f, 80f);
            if (cameraPivot is not null)
            {
                cameraPivot.localEulerAngles = new Vector3(_pitch, 0f, 0f);
            }
        }

        private void Move()
        {
            var gDir = gravityManager is not null ? gravityManager.CurrentDirection : Vector3.down;
            var gMag = gravityManager is not null ? gravityManager.CurrentGravity.magnitude : 9.81f;

            if (Vector3.Dot(_lastGravityDir, gDir) < 0.99f)
            {
                _lastGravityDir = gDir;
            }

            AlignToGravity(gDir);

            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);
            var wish = transform.TransformDirection(input) * moveSpeed;

            var grounded = IsGrounded(gDir);
            if (grounded)
            {
                // Amortigua velocidad hacia el suelo.
                var intoGround = Vector3.Dot(_velocity, gDir);
                if (intoGround > 0f)
                {
                    _velocity -= gDir * intoGround;
                }

                if (Input.GetButtonDown("Jump"))
                {
                    _velocity += -gDir * jumpSpeed;
                }
            }
            else
            {
                _velocity += gDir * gMag * Time.deltaTime;
            }

            // Movimiento horizontal relativo al "arriba" actual.
            var planar = Vector3.ProjectOnPlane(wish, gDir);
            var motion = (planar + _velocity) * Time.deltaTime;
            _controller.Move(motion);
        }

        private bool IsGrounded(Vector3 gDir)
        {
            var origin = transform.position + (-gDir) * 0.1f;
            var distance = (_controller.height * 0.5f) + groundCheckDistance;
            return Physics.SphereCast(
                origin,
                _controller.radius * 0.9f,
                gDir,
                out _,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore);
        }

        private void AlignToGravity(Vector3 gDir)
        {
            var targetUp = -gDir;
            var rot = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                1f - Mathf.Exp(-alignSpeed * Time.deltaTime));
        }
    }
}
