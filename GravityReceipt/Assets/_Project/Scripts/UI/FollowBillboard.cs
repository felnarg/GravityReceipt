using UnityEngine;

namespace GravityReceipt.UI
{
    /// <summary>
    /// Sigue a un target y mira a la cámara (etiquetas $ de valuables).
    /// </summary>
    public sealed class FollowBillboard : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 worldOffset = Vector3.up;

        public void Configure(Transform follow, Vector3 offset)
        {
            target = follow;
            worldOffset = offset;
        }

        private void LateUpdate()
        {
            if (target is not { })
            {
                Destroy(gameObject);
                return;
            }

            transform.position = target.position + worldOffset;
            var cam = Camera.main;
            if (cam is not null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            }
        }
    }
}
