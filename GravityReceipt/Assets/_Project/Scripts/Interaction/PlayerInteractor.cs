using GravityReceipt.Gravity;
using GravityReceipt.Mission;
using GravityReceipt.Player;
using UnityEngine;

namespace GravityReceipt.Interaction
{
    [DefaultExecutionOrder(30)]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        private enum GrabPhase
        {
            Idle,
            Winding,
            Holding
        }

        [SerializeField] private float reach = 3.4f;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private float grabWindUpSeconds = 0.4f;
        [SerializeField] private LayerMask interactMask = ~0;

        private LocalPlayerInput _input;
        private GrabPhase _phase = GrabPhase.Idle;
        private Rigidbody _held;
        private ValuableItem _heldValuable;
        private Grabbable _heldGrab;
        private float _windUp;
        private Renderer _focus;
        private Color _focusColor;

        public float WindUpNormalized =>
            _phase != GrabPhase.Winding || grabWindUpSeconds <= 0f
                ? 0f
                : Mathf.Clamp01(_windUp / grabWindUpSeconds);
        public bool IsHolding => _phase == GrabPhase.Holding && _held != null;
        public bool IsHoldingPackage => IsHolding && _held.GetComponent<MissionPackage>() != null;
        public bool HasLookTarget { get; private set; }

        public void Configure(Transform hold)
        {
            holdPoint = hold;
        }

        private void Awake()
        {
            _input = GetComponent<LocalPlayerInput>();
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            switch (_phase)
            {
                case GrabPhase.Holding:
                    TickHolding();
                    break;
                case GrabPhase.Winding:
                    TickWinding();
                    break;
                default:
                    TickIdle();
                    break;
            }
        }

        private void TickHolding()
        {
            HasLookTarget = false;
            ClearFocus();
            if (_held == null || _input.DropPressed())
            {
                Drop();
            }
        }

        private void TickIdle()
        {
            HasLookTarget = false;
            _windUp = 0f;
            if (!TryGetTarget(out var body, out var valuable, out var grab))
            {
                ClearFocus();
                return;
            }

            HasLookTarget = true;
            SetFocus(body);
            if (!_input.GrabHeld())
            {
                return;
            }

            _phase = GrabPhase.Winding;
            _windUp = 0f;
            TickWinding();
        }

        private void TickWinding()
        {
            if (!TryGetTarget(out var body, out var valuable, out var grab) || !_input.GrabHeld())
            {
                _phase = GrabPhase.Idle;
                _windUp = 0f;
                HasLookTarget = false;
                ClearFocus();
                return;
            }

            HasLookTarget = true;
            SetFocus(body);
            _windUp += Time.deltaTime;
            if (_windUp < grabWindUpSeconds)
            {
                return;
            }

            Grab(body, valuable, grab);
        }

        private void FixedUpdate()
        {
            if (_phase != GrabPhase.Holding || _held == null || holdPoint == null)
            {
                return;
            }

            _held.MovePosition(holdPoint.position);
            _held.MoveRotation(holdPoint.rotation);
        }

        private bool TryGetTarget(out Rigidbody body, out ValuableItem valuable, out Grabbable grab)
        {
            body = null;
            valuable = null;
            grab = null;
            var cam = _input != null && _input.PlayerCamera != null ? _input.PlayerCamera : Camera.main;
            if (cam == null)
            {
                return false;
            }

            var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out var hit, reach, interactMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            body = hit.rigidbody;
            if (body == null)
            {
                return false;
            }

            grab = body.GetComponent<Grabbable>();
            if (grab == null || !grab.CanGrab)
            {
                return false;
            }

            valuable = body.GetComponent<ValuableItem>();
            return true;
        }

        private void Grab(Rigidbody body, ValuableItem valuable, Grabbable grab)
        {
            if (body == null || grab == null || !grab.CanGrab)
            {
                _phase = GrabPhase.Idle;
                _windUp = 0f;
                return;
            }

            _held = body;
            _heldValuable = valuable;
            _heldGrab = grab;
            _held.isKinematic = true;
            _held.useGravity = false;
            grab.BeginGrab();
            if (_heldValuable != null)
            {
                _heldValuable.SetHeld(true);
            }

            IgnoreHeldCollision(true);
            _phase = GrabPhase.Holding;
            _windUp = 0f;
            ClearFocus();
        }

        public void Drop()
        {
            if (_held == null)
            {
                _phase = GrabPhase.Idle;
                _windUp = 0f;
                return;
            }

            _held.isKinematic = false;
            _held.useGravity = false;
            IgnoreHeldCollision(false);
            if (_heldValuable != null)
            {
                _heldValuable.SetHeld(false);
                _heldValuable.MarkMovedByPlayer();
            }

            if (_heldGrab != null)
            {
                _heldGrab.EndGrab();
            }

            var cam = _input != null && _input.PlayerCamera != null ? _input.PlayerCamera : Camera.main;
            if (cam != null)
            {
                var motor = GetComponent<PlayerMotor>();
                var up = motor != null && motor.Gravity != null
                    ? -motor.Gravity.CurrentDirection
                    : Vector3.up;
                _held.linearVelocity = cam.transform.forward * 2.4f + up * 0.55f;
            }

            _held = null;
            _heldValuable = null;
            _heldGrab = null;
            _phase = GrabPhase.Idle;
            _windUp = 0f;
        }

        private void IgnoreHeldCollision(bool ignore)
        {
            if (_held == null)
            {
                return;
            }

            var col = _held.GetComponent<Collider>();
            var cc = GetComponent<CharacterController>();
            if (col == null || cc == null)
            {
                return;
            }

            Physics.IgnoreCollision(col, cc, ignore);
        }

        private void SetFocus(Rigidbody body)
        {
            var renderer = body.GetComponent<Renderer>();
            if (renderer == _focus)
            {
                return;
            }

            ClearFocus();
            if (renderer == null)
            {
                return;
            }

            _focus = renderer;
            _focusColor = renderer.material.color;
            renderer.material.color = Color.Lerp(_focusColor, Color.white, 0.45f);
        }

        private void ClearFocus()
        {
            if (_focus != null)
            {
                _focus.material.color = _focusColor;
            }

            _focus = null;
        }

        private void OnDisable()
        {
            ClearFocus();
            Drop();
        }
    }
}
