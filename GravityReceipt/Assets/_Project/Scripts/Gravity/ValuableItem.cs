using GravityReceipt.World;
using UnityEngine;

namespace GravityReceipt.Gravity
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ValuableItem : MonoBehaviour
    {
        [SerializeField] private int price = 100;
        [SerializeField] private GravityManager gravityManager;
        [SerializeField] private bool isActiveValuable = true;

        private Rigidbody _body;
        private bool _wasMoving;

        public int Price => price;
        public bool IsActiveValuable => isActiveValuable && isActiveAndEnabled;
        public bool IsHeld { get; private set; }
        public GravityManager Manager => gravityManager;

        public void Configure(int newPrice, GravityManager manager)
        {
            price = Mathf.Max(0, newPrice);
            SetGravityManager(manager);
        }

        public void SetGravityManager(GravityManager next)
        {
            if (next == gravityManager)
            {
                return;
            }

            gravityManager?.Unregister(this);
            gravityManager = next;
            if (isActiveAndEnabled)
            {
                gravityManager?.Register(this);
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            gravityManager?.Register(this);
        }

        private void OnDisable()
        {
            gravityManager?.Unregister(this);
        }

        private void FixedUpdate()
        {
            var room = RoomRegistry.FindRoom(transform.position);
            if (room != null && room.HasOwnGravity && room.Gravity != null)
            {
                SetGravityManager(room.Gravity);
            }

            if (IsHeld || _body == null || _body.isKinematic)
            {
                _wasMoving = false;
                return;
            }

            var moving = _body.linearVelocity.sqrMagnitude > 0.05f
                         || (!_body.IsSleeping() && _body.angularVelocity.sqrMagnitude > 0.05f);
            if (moving && !_wasMoving)
            {
                gravityManager?.NotifyValuableMoved(this);
            }

            _wasMoving = moving;
        }

        public void SetHeld(bool held)
        {
            IsHeld = held;
        }

        public void SetPrice(int newPrice)
        {
            price = Mathf.Max(0, newPrice);
            gravityManager?.NotifyValuableMoved(this);
        }

        /// <summary>Llamar al soltar el objeto (recalcula dominante + dirección de g).</summary>
        public void MarkMovedByPlayer()
        {
            gravityManager?.NotifyValuableMoved(this);
        }
    }
}
