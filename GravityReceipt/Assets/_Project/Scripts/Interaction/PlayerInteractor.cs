using GravityReceipt.Gravity;
using UnityEngine;

namespace GravityReceipt.Interaction
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float reach = 3f;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private float grabWindUpSeconds = 0.4f;
        [SerializeField] private LayerMask interactMask = ~0;

        private Rigidbody _held;
        private ValuableItem _heldValuable;
        private float _windUp;

        private void Update()
        {
            if (_held is not null)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1))
                {
                    Drop();
                }

                return;
            }

            if (!Input.GetKey(KeyCode.E) && !Input.GetMouseButton(0))
            {
                _windUp = 0f;
                return;
            }

            if (!TryGetTarget(out var body, out var valuable))
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
        }

        private void FixedUpdate()
        {
            if (_held is null || holdPoint is null)
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
            var ray = new Ray(transform.position, transform.forward);
            if (Camera.main is { } cam)
            {
                ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }

            if (!Physics.Raycast(ray, out var hit, reach, interactMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            body = hit.rigidbody;
            if (body is null)
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
            if (_held is null)
            {
                return;
            }

            _held.isKinematic = false;
            _held.useGravity = true;
            _heldValuable?.SetHeld(false);
            _heldValuable?.MarkMovedByPlayer();
            _held = null;
            _heldValuable = null;
        }
    }
}
