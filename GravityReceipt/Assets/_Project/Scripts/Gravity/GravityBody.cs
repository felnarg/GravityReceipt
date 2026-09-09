using GravityReceipt.World;
using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Aplica la gravedad de la sala actual (Physics.gravity se deja en cero).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(10)]
    public sealed class GravityBody : MonoBehaviour
    {
        [SerializeField] private GravityManager gravityManager;

        private Rigidbody _body;
        private Vector3 _appliedGravity;

        public GravityManager Manager => gravityManager;

        public void SetManager(GravityManager manager)
        {
            gravityManager = manager;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            if (_body != null)
            {
                _body.useGravity = false;
            }
        }

        private void FixedUpdate()
        {
            if (_body == null || _body.isKinematic)
            {
                return;
            }

            var room = RoomRegistry.FindRoom(_body.position);
            if (room != null && room.HasOwnGravity && room.Gravity != null)
            {
                gravityManager = room.Gravity;
            }

            var g = gravityManager != null
                ? gravityManager.CurrentGravity
                : Vector3.down * 9.81f;
            if ((g - _appliedGravity).sqrMagnitude > 1f)
            {
                _body.WakeUp();
                _appliedGravity = g;
            }

            _body.AddForce(g, ForceMode.Acceleration);
        }
    }
}
