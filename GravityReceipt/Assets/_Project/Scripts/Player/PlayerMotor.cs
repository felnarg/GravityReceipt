using GravityReceipt.Gravity;
using GravityReceipt.Interaction;
using GravityReceipt.Mission;
using GravityReceipt.World;
using UnityEngine;

namespace GravityReceipt.Player
{
    public enum LocomotionPhase
    {
        Grounded = 0,
        Airborne = 1
    }

    [RequireComponent(typeof(CharacterController))]
    [DefaultExecutionOrder(20)]
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
        private LocalPlayerInput _input;
        private PlayerRole _role;
        private Vector3 _velocity;
        private float _pitch;
        private Vector3 _lastGravityDir = Vector3.down;
        private float _airFall;
        private bool _ownsCursor;
        private bool _pendingUnstuck;
        private Camera _cam;
        private float _baseFov = 60f;
        private float _fovPunch;
        private float _shake;
        private float _sprintFov;
        private Vector3 _camBaseLocal;
        private float _jumpBuffer;
        private float _coyote;

        public GravityManager Gravity => gravityManager;
        public bool Grounded { get; private set; }
        public LocomotionPhase Locomotion { get; private set; }

        public void Configure(Transform pivot, GravityManager gravity)
        {
            cameraPivot = pivot;
            CacheCamera();
            SetGravityManager(gravity);
        }

        public void SetGravityManager(GravityManager next)
        {
            if (next == gravityManager)
            {
                return;
            }

            if (gravityManager != null)
            {
                gravityManager.GravityChanged -= OnGravityChanged;
            }

            gravityManager = next;
            if (gravityManager != null)
            {
                gravityManager.GravityChanged += OnGravityChanged;
                _lastGravityDir = gravityManager.CurrentDirection;
            }
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<LocalPlayerInput>();
            _role = GetComponent<PlayerRole>();
            _ownsCursor = _input != null && _input.UsesMouseLook;
            CacheCamera();

            if (cameraPivot == null && _input != null && _input.PlayerCamera != null)
            {
                cameraPivot = _input.PlayerCamera.transform;
                CacheCamera();
            }

            if (_ownsCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (gravityManager != null)
            {
                gravityManager.GravityChanged += OnGravityChanged;
                _lastGravityDir = gravityManager.CurrentDirection;
            }
        }

        private void OnDestroy()
        {
            if (gravityManager != null)
            {
                gravityManager.GravityChanged -= OnGravityChanged;
            }
        }

        private void CacheCamera()
        {
            if (cameraPivot == null)
            {
                return;
            }

            _camBaseLocal = cameraPivot.localPosition;
            _cam = cameraPivot.GetComponent<Camera>();
            if (_cam != null)
            {
                _baseFov = _cam.fieldOfView;
            }
        }

        private void OnGravityChanged(Vector3 _, ValuableItem __)
        {
            _velocity *= 0.35f;
            _pendingUnstuck = true;
            _fovPunch = 14f;
            _shake = 0.28f;
            GravityFlipSfx.Play();
            var rec = MatchHighlightRecorder.Instance;
            if (rec != null)
            {
                rec.ReportFlip();
            }
        }

        private void Update()
        {
            if (_ownsCursor)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
                {
                    var match = MatchDirector.Instance;
                    if (match == null || match.IsPlaying)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                }
            }

            var room = RoomRegistry.FindRoom(transform.position);
            if (room != null && room.HasOwnGravity && room.Gravity != null)
            {
                SetGravityManager(room.Gravity);
            }

            Look();
            ApplyFlipFeel();
            Move();
        }

        private void ApplyFlipFeel()
        {
            if (cameraPivot == null)
            {
                return;
            }

            _fovPunch = Mathf.MoveTowards(_fovPunch, 0f, Time.deltaTime * 38f);
            _shake = Mathf.MoveTowards(_shake, 0f, Time.deltaTime * 1.1f);
            var sprintTarget = _role != null && _role.MoveMultiplier > 1.05f ? 7f : 0f;
            _sprintFov = Mathf.MoveTowards(_sprintFov, sprintTarget, Time.deltaTime * 36f);
            if (_cam != null)
            {
                _cam.fieldOfView = _baseFov + _fovPunch + _sprintFov;
            }

            var offset = _shake > 0.01f
                ? new Vector3(
                    (Mathf.PerlinNoise(Time.time * 28f, 0.3f) - 0.5f) * _shake * 0.12f,
                    (Mathf.PerlinNoise(0.7f, Time.time * 31f) - 0.5f) * _shake * 0.12f,
                    0f)
                : Vector3.zero;
            cameraPivot.localPosition = _camBaseLocal + offset;
        }

        private void Look()
        {
            if (_input == null)
            {
                return;
            }

            if (_ownsCursor && Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            var look = _input.LookDelta();
            var mx = look.x * mouseSensitivity;
            var my = look.y * mouseSensitivity;
            transform.Rotate(0f, mx, 0f, Space.Self);
            _pitch = Mathf.Clamp(_pitch - my, -80f, 80f);
            if (cameraPivot != null)
            {
                cameraPivot.localEulerAngles = new Vector3(_pitch, 0f, 0f);
            }
        }

        private void Move()
        {
            var gDir = gravityManager != null ? gravityManager.CurrentDirection : Vector3.down;
            var gMag = gravityManager != null ? gravityManager.CurrentGravity.magnitude : 9.81f;

            if (Vector3.Dot(_lastGravityDir, gDir) < 0.99f)
            {
                _lastGravityDir = gDir;
            }

            AlignToGravity(gDir);
            if (_pendingUnstuck)
            {
                _pendingUnstuck = false;
                TryUnstuck(gDir);
            }

            var axes = _input != null ? _input.MoveAxes() : Vector2.zero;
            var input = new Vector3(axes.x, 0f, axes.y);
            input = Vector3.ClampMagnitude(input, 1f);
            var speed = moveSpeed * (_role != null ? _role.MoveMultiplier : 1f);
            var wish = transform.TransformDirection(input) * speed;

            Grounded = IsGrounded(gDir);
            Locomotion = Grounded ? LocomotionPhase.Grounded : LocomotionPhase.Airborne;
            if (_input != null && _input.JumpPressed())
            {
                _jumpBuffer = 0.12f;
            }

            _jumpBuffer = Mathf.Max(0f, _jumpBuffer - Time.deltaTime);
            if (Locomotion == LocomotionPhase.Grounded)
            {
                _coyote = 0.12f;
                if (_airFall > 0.5f && _input != null)
                {
                    var rec = MatchHighlightRecorder.Instance;
                    if (rec != null)
                    {
                        rec.ReportFall(_input.Slot, _airFall);
                    }
                }

                if (_airFall > 2.2f)
                {
                    _fovPunch = Mathf.Max(_fovPunch, 8f);
                    _shake = Mathf.Max(_shake, 0.18f);
                }

                _airFall = 0f;

                var intoGround = Vector3.Dot(_velocity, gDir);
                if (intoGround > 0f)
                {
                    _velocity -= gDir * intoGround;
                }
            }
            else
            {
                _coyote = Mathf.Max(0f, _coyote - Time.deltaTime);
                _velocity += gDir * gMag * Time.deltaTime;
                var fallStep = Vector3.Dot(_velocity, gDir) * Time.deltaTime;
                if (fallStep > 0f)
                {
                    _airFall += fallStep;
                }
            }

            if (_jumpBuffer > 0f && _coyote > 0f)
            {
                _jumpBuffer = 0f;
                _coyote = 0f;
                var intoGround = Vector3.Dot(_velocity, gDir);
                if (intoGround > 0f)
                {
                    _velocity -= gDir * intoGround;
                }

                _velocity += -gDir * jumpSpeed;
            }

            var planar = Vector3.ProjectOnPlane(wish, gDir);
            var motion = (planar + _velocity) * Time.deltaTime;
            _controller.Move(motion);
        }

        private void TryUnstuck(Vector3 gDir)
        {
            _controller.enabled = false;
            var center = transform.TransformPoint(_controller.center);
            var overlapping = Physics.CheckSphere(
                center,
                _controller.radius * 1.15f,
                ~0,
                QueryTriggerInteraction.Ignore);
            if (overlapping)
            {
                transform.position += -gDir * 0.55f;
                var stillStuck = Physics.CheckSphere(
                    transform.TransformPoint(_controller.center),
                    _controller.radius * 1.15f,
                    ~0,
                    QueryTriggerInteraction.Ignore);
                if (stillStuck)
                {
                    var right = Vector3.Cross(gDir, transform.forward);
                    if (right.sqrMagnitude < 0.01f)
                    {
                        right = Vector3.Cross(gDir, transform.up);
                    }

                    right.Normalize();
                    transform.position += right * 0.35f;
                }
            }

            _controller.enabled = true;
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

        public void Warp(Vector3 position)
        {
            var interactor = GetComponent<PlayerInteractor>();
            if (interactor != null && interactor.IsHolding)
            {
                interactor.Drop();
            }

            if (_controller != null)
            {
                _controller.enabled = false;
            }

            transform.SetPositionAndRotation(position, Quaternion.identity);
            _velocity = Vector3.zero;
            _pitch = 0f;
            _fovPunch = 0f;
            _shake = 0f;
            _sprintFov = 0f;
            _jumpBuffer = 0f;
            _coyote = 0f;
            if (cameraPivot != null)
            {
                cameraPivot.localEulerAngles = Vector3.zero;
                cameraPivot.localPosition = _camBaseLocal;
            }

            if (_cam != null)
            {
                _cam.fieldOfView = _baseFov;
            }

            if (_controller != null)
            {
                _controller.enabled = true;
            }
        }
    }
}
