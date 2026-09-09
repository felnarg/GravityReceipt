using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Pulso naranja sobre el paquete para no perderlo tras un flip.
    /// </summary>
    public sealed class PackageBeacon : MonoBehaviour
    {
        private Transform _orb;
        private Renderer _renderer;

        private void LateUpdate()
        {
            EnsureOrb();
            var pulse = 0.22f + 0.08f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5.5f));
            _orb.localScale = Vector3.one * pulse;
            _orb.position = transform.position + transform.up * 0.85f;
            if (_renderer != null)
            {
                var t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5.5f);
                _renderer.material.color = Color.Lerp(
                    new Color(1f, 0.55f, 0.12f),
                    new Color(1f, 0.92f, 0.35f),
                    t);
            }
        }

        private void EnsureOrb()
        {
            if (_orb != null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "PackageBeacon";
            go.transform.SetParent(transform, false);
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            _renderer = go.GetComponent<Renderer>();
            if (_renderer != null)
            {
                var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    _renderer.sharedMaterial = new Material(shader) { color = new Color(1f, 0.6f, 0.15f) };
                }
            }

            _orb = go.transform;
        }
    }
}
