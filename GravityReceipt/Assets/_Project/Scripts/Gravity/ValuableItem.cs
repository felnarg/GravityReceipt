using GravityReceipt.World;
using UnityEngine;

namespace GravityReceipt.Gravity
{
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-10)]
    public sealed class ValuableItem : MonoBehaviour
    {
        [SerializeField] private int price = 100;
        [SerializeField] private GravityManager gravityManager;
        [SerializeField] private bool isActiveValuable = true;

        private Rigidbody _body;
        private bool _wasMoving;
        private Vector3 _homePos;
        private Quaternion _homeRot;
        private bool _homeCaptured;

        public int Price => price;
        public bool IsActiveValuable => isActiveValuable && isActiveAndEnabled;
        public bool IsHeld { get; private set; }
        public GravityManager Manager => gravityManager;

        public void Configure(int newPrice, GravityManager manager)
        {
            price = Mathf.Max(0, newPrice);
            SetGravityManager(manager);
            CaptureHome();
        }

        public void SetGravityManager(GravityManager next)
        {
            if (next == gravityManager)
            {
                return;
            }

            if (gravityManager != null)
            {
                gravityManager.Unregister(this);
            }

            gravityManager = next;
            if (isActiveAndEnabled && gravityManager != null)
            {
                gravityManager.Register(this);
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            CaptureHome();
        }

        private void OnEnable()
        {
            if (gravityManager != null)
            {
                gravityManager.Register(this);
            }
        }

        private void OnDisable()
        {
            if (gravityManager != null)
            {
                gravityManager.Unregister(this);
            }
        }

        private void FixedUpdate()
        {
            var room = RoomRegistry.FindRoom(transform.position);
            if (room != null && room.HasOwnGravity && room.Gravity != null)
            {
                SetGravityManager(room.Gravity);
            }

            if (IsHeld)
            {
                if (gravityManager != null)
                {
                    gravityManager.NotifyValuableMoved(this);
                }

                _wasMoving = false;
                return;
            }

            if (_body == null || _body.isKinematic)
            {
                _wasMoving = false;
                return;
            }

            var moving = _body.linearVelocity.sqrMagnitude > 0.05f
                         || (!_body.IsSleeping() && _body.angularVelocity.sqrMagnitude > 0.05f);
            if (gravityManager != null && moving != _wasMoving)
            {
                gravityManager.NotifyValuableMoved(this);
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
            if (gravityManager != null)
            {
                gravityManager.NotifyValuableMoved(this);
            }
        }

        /// <summary>Llamar al soltar el objeto (recalcula dominante + dirección de g).</summary>
        public void MarkMovedByPlayer()
        {
            if (gravityManager != null)
            {
                gravityManager.NotifyValuableMoved(this);
            }
        }

        public void ResetToHome()
        {
            if (IsHeld)
            {
                return;
            }

            CaptureHome();
            if (_body != null)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.position = _homePos;
                _body.rotation = _homeRot;
            }

            transform.SetPositionAndRotation(_homePos, _homeRot);
            if (gravityManager != null)
            {
                gravityManager.NotifyValuableMoved(this);
            }
        }

        private void CaptureHome()
        {
            if (_homeCaptured)
            {
                return;
            }

            _homePos = transform.position;
            _homeRot = transform.rotation;
            _homeCaptured = true;
        }
    }
}
