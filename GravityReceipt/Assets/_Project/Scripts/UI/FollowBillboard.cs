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

        public static bool Hidden { get; set; }

        public void Configure(Transform follow, Vector3 offset)
        {
            target = follow;
            worldOffset = offset;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = target.position + target.rotation * worldOffset;
            var show = !Hidden;
            var renderers = GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = show;
                }
            }
            var cam = ClosestCamera(transform.position);
            if (cam != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            }
        }

        public static Camera ClosestCamera(Vector3 worldPos)
        {
            Camera best = null;
            var bestSqr = float.MaxValue;
            var cams = Camera.allCameras;
            for (var i = 0; i < cams.Length; i++)
            {
                var cam = cams[i];
                if (cam == null || !cam.isActiveAndEnabled)
                {
                    continue;
                }

                var d = (cam.transform.position - worldPos).sqrMagnitude;
                if (d >= bestSqr)
                {
                    continue;
                }

                bestSqr = d;
                best = cam;
            }

            return best != null ? best : Camera.main;
        }
    }
}
