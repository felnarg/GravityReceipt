using UnityEngine;

namespace GravityReceipt.World
{
    /// <summary>
    /// Giro local para agujas de reloj / LEDs (dress, sin gameplay).
    /// </summary>
    public sealed class SpinInPlace : MonoBehaviour
    {
        [SerializeField] private Vector3 axis = Vector3.forward;
        [SerializeField] private float degreesPerSecond = 30f;

        public void Configure(Vector3 localAxis, float degPerSec)
        {
            axis = localAxis;
            degreesPerSecond = degPerSec;
        }

        private void Update()
        {
            transform.Rotate(axis, degreesPerSecond * Time.unscaledDeltaTime, Space.Self);
        }
    }
}
