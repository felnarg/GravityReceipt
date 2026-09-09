using GravityReceipt.Interaction;
using GravityReceipt.Player;
using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Flecha local a los pies: hacia dónde tira g ahora (o el telegráfo).
    /// </summary>
    public sealed class GravityCompass : MonoBehaviour
    {
        private PlayerMotor _motor;
        private Transform _arrow;
        private Renderer _renderer;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
        }

        private void LateUpdate()
        {
            if (_motor == null)
            {
                return;
            }

            var g = _motor.Gravity;
            if (g == null)
            {
                if (_arrow != null)
                {
                    _arrow.gameObject.SetActive(false);
                }

                return;
            }

            EnsureArrow();
            var inter = GetComponent<PlayerInteractor>();
            var holdingDom = inter != null
                             && inter.HeldValuable != null
                             && g.Dominant == inter.HeldValuable;
            var dir = g.ShowFlipBanner
                ? g.BannerDirection
                : holdingDom
                    ? g.PreviewDirection
                    : g.CurrentDirection;
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = Vector3.down;
            }

            var unusual = g.IsAnchored || g.ShowFlipBanner || holdingDom || Vector3.Dot(dir, Vector3.down) < 0.92f;
            _arrow.gameObject.SetActive(unusual);
            if (!unusual)
            {
                return;
            }

            _arrow.position = transform.position + (-dir) * 1.05f + transform.forward * 0.45f;
            var up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.95f ? Vector3.forward : Vector3.up;
            _arrow.rotation = Quaternion.LookRotation(dir, up);
            _arrow.localScale = new Vector3(0.12f, 0.12f, 0.42f);
            if (_renderer != null)
            {
                Color color;
                if (g.IsAnchored)
                {
                    color = new Color(0.35f, 0.85f, 1f);
                }
                else if (g.ShowFlipBanner)
                {
                    color = Color.Lerp(new Color(1f, 0.9f, 0.2f), new Color(1f, 0.35f, 0.1f), g.TelegraphNormalized);
                }
                else if (holdingDom)
                {
                    color = new Color(1f, 0.82f, 0.2f);
                }
                else
                {
                    color = new Color(0.95f, 0.95f, 0.95f, 0.85f);
                }

                _renderer.material.color = color;
            }
        }

        private void EnsureArrow()
        {
            if (_arrow != null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "GravityCompass";
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
                    _renderer.sharedMaterial = new Material(shader) { color = Color.white };
                }
            }

            _arrow = go.transform;
        }
    }
}
