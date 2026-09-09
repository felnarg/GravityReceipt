using UnityEngine;

namespace GravityReceipt.World
{
    /// <summary>
    /// Mantiene un offset a lo largo del “arriba” de g (etiquetas de losa tras un flip).
    /// Corre después de FollowBillboard para no pelear la posición.
    /// </summary>
    [DefaultExecutionOrder(15)]
    public sealed class GravityUpFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float height = 1.15f;

        public void Configure(Transform follow, float alongUp)
        {
            target = follow;
            height = alongUp;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            var up = RoomRegistry.UpAt(target.position);
            transform.position = target.position + up * height;
        }
    }
}
