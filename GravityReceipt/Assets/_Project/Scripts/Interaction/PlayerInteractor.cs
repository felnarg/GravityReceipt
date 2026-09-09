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

        [SerializeField] private float reach = 3.8f;
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
        public bool IsWinding => _phase == GrabPhase.Winding;
        public bool IsHoldingPackage => IsHolding && _held.GetComponent<MissionPackage>() != null;
        public ValuableItem HeldValuable => _heldValuable;
        public ValuableItem LookValuable { get; private set; }
        public bool HasLookTarget { get; private set; }
        public string LookHint { get; private set; } = "";

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
            LookValuable = _heldValuable;
            LookHint = HoldHint();
            ClearFocus();
            if (_held == null || _input.DropPressed())
            {
                Drop();
            }
        }

        private string HoldHint()
        {
            if (_held == null)
            {
                return "";
            }

            if (_heldValuable == null)
            {
                return _held.GetComponent<MissionPackage>() != null
                    ? "soltar · enchúfalo (no tira de g)"
                    : "soltar · sin $ · no tira de g";
            }

            var g = _heldValuable.Manager;
            if (g == null || g.Dominant != _heldValuable)
            {
                return "soltar";
            }

            if (g.IsTelegraphing)
            {
                return "soltar · ¡FLIP en camino!";
            }

            return "soltar · acércala a una PARED";
        }

        private void TickIdle()
        {
            HasLookTarget = false;
            LookHint = "";
            LookValuable = null;
            _windUp = 0f;
            if (!TryGetTarget(out var body, out var valuable, out var grab, out var occupied))
            {
                if (occupied && body != null)
                {
                    HasLookTarget = true;
                    LookHint = $"{FormatHint(body, valuable)}  · ocupado";
                    LookValuable = valuable;
                }

                ClearFocus();
                return;
            }

            HasLookTarget = true;
            LookHint = FormatHint(body, valuable);
            LookValuable = valuable;
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
            if (!TryGetTarget(out var body, out var valuable, out var grab, out _) || !_input.GrabHeld())
            {
                _phase = GrabPhase.Idle;
                _windUp = 0f;
                HasLookTarget = false;
                LookHint = "";
                LookValuable = null;
                ClearFocus();
                return;
            }

            HasLookTarget = true;
            LookHint = FormatHint(body, valuable);
            LookValuable = valuable;
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

            var target = holdPoint.position;
            var cam = _input != null ? _input.PlayerCamera : null;
            if (cam != null)
            {
                var origin = cam.transform.position;
                var to = target - origin;
                var dist = to.magnitude;
                if (dist > 0.08f)
                {
                    var dir = to / dist;
                    if (Physics.SphereCast(origin, 0.14f, dir, out var hit, dist, ~0, QueryTriggerInteraction.Ignore)
                        && hit.collider != null
                        && hit.collider.transform != _held.transform
                        && !hit.collider.transform.IsChildOf(_held.transform)
                        && hit.collider.GetComponent<CharacterController>() == null)
                    {
                        target = hit.point - dir * 0.28f;
                    }
                }
            }

            _held.MovePosition(target);
            _held.MoveRotation(holdPoint.rotation);
        }

        private bool TryGetTarget(out Rigidbody body, out ValuableItem valuable, out Grabbable grab, out bool occupied)
        {
            body = null;
            valuable = null;
            grab = null;
            occupied = false;
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
            if (grab == null)
            {
                return false;
            }

            valuable = body.GetComponent<ValuableItem>();
            if (!grab.CanGrab)
            {
                occupied = true;
                return false;
            }

            return true;
        }

        private static string FormatHint(Rigidbody body, ValuableItem valuable)
        {
            if (body == null)
            {
                return "";
            }

            var pretty = PrettyName(body.name);
            if (body.GetComponent<MissionPackage>() != null)
            {
                return "PAQUETE";
            }

            return valuable != null ? $"{pretty}  ${valuable.Price}" : $"{pretty}  (sin $)";
        }

        private static string PrettyName(string n)
        {
            if (n is not { Length: > 0 })
            {
                return "objeto";
            }

            if (n.StartsWith("Valuable_"))
            {
                var rest = n[9..];
                var us = rest.LastIndexOf('_');
                return us > 0 ? rest[..us] : rest;
            }

            if (n.StartsWith("Prop_"))
            {
                return n[5..];
            }

            return n;
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
            MissionSfx.PlayGrab();
            var match = MatchDirector.Instance;
            if (match == null)
            {
                return;
            }

            if (_heldValuable != null)
            {
                match.NotifyFirstValuableGrab();
            }
            else if (body.GetComponent<MissionPackage>() != null)
            {
                match.NotifyFirstPackageGrab();
            }
            else
            {
                match.NotifyFirstGrayGrab();
            }
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
            LookHint = "";
            LookValuable = null;
            HasLookTarget = false;
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

            if (!ignore && _held.GetComponent<MissionPackage>() != null)
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
