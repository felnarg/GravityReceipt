using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Flecha 3D durante el telegráfo: indica a dónde va a tirar g.
    /// </summary>
    public sealed class GravityTelegraphArrow : MonoBehaviour
    {
        [SerializeField] private GravityManager gravityManager;

        private Transform _arrow;
        private Renderer _renderer;

        private void Awake()
        {
            if (gravityManager == null)
            {
                gravityManager = GetComponent<GravityManager>();
            }
        }

        private void Update()
        {
            if (gravityManager == null || !gravityManager.IsTelegraphing)
            {
                if (_arrow != null)
                {
                    _arrow.gameObject.SetActive(false);
                }

                return;
            }

            EnsureArrow();
            _arrow.gameObject.SetActive(true);
            _arrow.position = transform.position + (-gravityManager.PendingDirection) * 0.35f;
            var dir = gravityManager.PendingDirection;
            if (dir.sqrMagnitude > 0.01f)
            {
                var up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.95f ? Vector3.forward : Vector3.up;
                _arrow.rotation = Quaternion.LookRotation(dir, up);
            }

            var pulse = 2.1f + 0.45f * Mathf.Sin(Time.time * 14f);
            _arrow.localScale = new Vector3(0.7f, 0.7f, pulse);
            if (_renderer != null)
            {
                _renderer.material.color = Color.Lerp(
                    new Color(1f, 0.85f, 0.15f),
                    new Color(1f, 0.35f, 0.1f),
                    gravityManager.TelegraphNormalized);
            }
        }

        private void EnsureArrow()
        {
            if (_arrow != null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "TelegraphArrow";
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
                    _renderer.sharedMaterial = new Material(shader) { color = new Color(1f, 0.8f, 0.1f) };
                }
            }

            _arrow = go.transform;
        }
    }
}
