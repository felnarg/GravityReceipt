using GravityReceipt.Gravity;
using UnityEngine;

namespace GravityReceipt.World
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class RoomVolume : MonoBehaviour
    {
        [SerializeField] private string roomId = "Room";
        [SerializeField] private GravityManager gravityManager;
        [SerializeField] private bool inheritGravity;

        private BoxCollider _box;

        public string RoomId => roomId;
        public GravityManager Gravity => gravityManager;
        public bool HasOwnGravity => !inheritGravity && gravityManager != null;
        public Vector3 Center => transform.position;

        public void Configure(string id, GravityManager manager, bool inherit)
        {
            roomId = id;
            gravityManager = manager;
            inheritGravity = inherit;
        }

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;
        }

        private void OnEnable()
        {
            RoomRegistry.Register(this);
        }

        private void OnDisable()
        {
            RoomRegistry.Unregister(this);
        }

        public bool Contains(Vector3 worldPos)
        {
            if (_box is null)
            {
                _box = GetComponent<BoxCollider>();
            }

            if (_box is null)
            {
                return false;
            }

            var local = transform.InverseTransformPoint(worldPos);
            var e = _box.size * 0.5f;
            var c = _box.center;
            local -= c;
            return Mathf.Abs(local.x) <= e.x
                   && Mathf.Abs(local.y) <= e.y
                   && Mathf.Abs(local.z) <= e.z;
        }
    }
}
