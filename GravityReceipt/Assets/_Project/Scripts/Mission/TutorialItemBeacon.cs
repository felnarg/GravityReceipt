using GravityReceipt.Gravity;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Orbe sobre la taza tutorial hasta que la agarran (regla en 30 s).
    /// </summary>
    public sealed class TutorialItemBeacon : MonoBehaviour
    {
        private Transform _orb;
        private Renderer _renderer;
        private ValuableItem _item;

        private void Awake()
        {
            _item = GetComponent<ValuableItem>();
        }

        private void LateUpdate()
        {
            EnsureOrb();
            var held = _item != null && _item.IsHeld;
            _orb.gameObject.SetActive(!held);
            if (held)
            {
                return;
            }

            var pulse = 0.18f + 0.07f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6.2f));
            _orb.localScale = Vector3.one * pulse;
            _orb.position = transform.position + transform.up * 0.62f;
            if (_renderer != null)
            {
                var t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6.2f);
                _renderer.material.color = Color.Lerp(
                    new Color(1f, 0.82f, 0.2f),
                    new Color(1f, 0.95f, 0.55f),
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
            go.name = "TutorialBeacon";
            go.transform.SetParent(transform, false);
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            _renderer = go.GetComponent<Renderer>();
            if (_renderer != null)
            {
                _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    _renderer.sharedMaterial = new Material(shader) { color = new Color(1f, 0.85f, 0.25f) };
                }
            }

            _orb = go.transform;
        }
    }
}
