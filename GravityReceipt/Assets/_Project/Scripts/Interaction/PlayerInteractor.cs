using GravityReceipt.Gravity;
using GravityReceipt.Player;
using UnityEngine;

namespace GravityReceipt.Interaction
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float reach = 3f;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private float grabWindUpSeconds = 0.4f;
        [SerializeField] private LayerMask interactMask = ~0;

        private LocalPlayerInput _input;
        private Rigidbody _held;
        private ValuableItem _heldValuable;
        private float _windUp;
        private Renderer _focus;
        private Color _focusColor;

        public float WindUpNormalized =>
            grabWindUpSeconds <= 0f ? 0f : Mathf.Clamp01(_windUp / grabWindUpSeconds);
        public bool IsHolding => _held != null;
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

            HasLookTarget = false;
            if (_held == null)
            {
                _heldValuable = null;
            }
            else
            {
                ClearFocus();
                if (_input.DropPressed())
                {
                    Drop();
                }

                return;
            }

            if (!TryGetTarget(out var body, out var valuable))
            {
                _windUp = 0f;
                ClearFocus();
                return;
            }

            HasLookTarget = true;

            SetFocus(body);
            if (!_input.GrabHeld())
            {
                _windUp = 0f;
                return;
            }

            _windUp += Time.deltaTime;
            if (_windUp < grabWindUpSeconds)
            {
                return;
            }

            Grab(body, valuable);
            _windUp = 0f;
            ClearFocus();
        }

        private void FixedUpdate()
        {
            if (_held == null || holdPoint == null)
            {
                return;
            }

            _held.MovePosition(holdPoint.position);
            _held.MoveRotation(holdPoint.rotation);
        }

        private bool TryGetTarget(out Rigidbody body, out ValuableItem valuable)
        {
            body = null;
            valuable = null;
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

            var grabbable = body.GetComponent<Grabbable>();
            if (grabbable == null || !grabbable.CanGrab)
            {
                return false;
            }

            valuable = body.GetComponent<ValuableItem>();
            return true;
        }

        private void Grab(Rigidbody body, ValuableItem valuable)
        {
            _held = body;
            _heldValuable = valuable;
            _held.isKinematic = true;
            _held.useGravity = false;
            _heldValuable?.SetHeld(true);
        }

        private void Drop()
        {
            if (_held == null)
            {
                return;
            }

            _held.isKinematic = false;
            _held.useGravity = false;
            _heldValuable?.SetHeld(false);
            _heldValuable?.MarkMovedByPlayer();
            _held = null;
            _heldValuable = null;
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
        }
    }
}
