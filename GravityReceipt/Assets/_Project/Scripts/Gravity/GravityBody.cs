using GravityReceipt.World;
using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Aplica la gravedad de la sala actual (Physics.gravity se deja en cero).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class GravityBody : MonoBehaviour
    {
        [SerializeField] private GravityManager gravityManager;

        private Rigidbody _body;

        public GravityManager Manager => gravityManager;

        public void SetManager(GravityManager manager)
        {
            gravityManager = manager;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            if (_body is { })
            {
                _body.useGravity = false;
            }
        }

        private void FixedUpdate()
        {
            if (_body is not { isKinematic: false })
            {
                return;
            }

            var room = RoomRegistry.FindRoom(_body.position);
            if (room is { HasOwnGravity: true, Gravity: { } roomGravity })
            {
                gravityManager = roomGravity;
            }

            var g = gravityManager is not null
                ? gravityManager.CurrentGravity
                : Vector3.down * 9.81f;
            _body.AddForce(g, ForceMode.Acceleration);
        }
    }
}
