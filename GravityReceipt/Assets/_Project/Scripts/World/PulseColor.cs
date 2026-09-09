using UnityEngine;

namespace GravityReceipt.World
{
    /// <summary>
    /// Pulso unlit para LEDs / luces de dress.
    /// </summary>
    public sealed class PulseColor : MonoBehaviour
    {
        [SerializeField] private Color from = Color.green;
        [SerializeField] private Color to = Color.black;
        [SerializeField] private float speed = 4f;

        private Renderer _renderer;

        public void Configure(Color a, Color b, float pulseSpeed)
        {
            from = a;
            to = b;
            speed = pulseSpeed;
        }

        private void LateUpdate()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            if (_renderer == null)
            {
                return;
            }

            var t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * speed);
            _renderer.material.color = Color.Lerp(from, to, t);
        }
    }
}
