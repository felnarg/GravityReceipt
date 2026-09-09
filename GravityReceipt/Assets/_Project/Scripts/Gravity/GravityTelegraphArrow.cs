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
            if (gravityManager is null)
            {
                gravityManager = GetComponent<GravityManager>();
            }
        }

        private void Update()
        {
            if (gravityManager == null || !gravityManager.IsTelegraphing)
            {
                if (_arrow is not null)
                {
                    _arrow.gameObject.SetActive(false);
                }

                return;
            }

            EnsureArrow();
            _arrow.gameObject.SetActive(true);
            _arrow.position = transform.position + (-gravityManager.PendingDirection) * 0.5f;
            var dir = gravityManager.PendingDirection;
            if (dir.sqrMagnitude > 0.01f)
            {
                var up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.95f ? Vector3.forward : Vector3.up;
                _arrow.rotation = Quaternion.LookRotation(dir, up);
            }

            var pulse = 1.6f + 0.35f * Mathf.Sin(Time.time * 14f);
            _arrow.localScale = new Vector3(0.55f, 0.55f, pulse);
            if (_renderer is not null)
            {
                _renderer.material.color = Color.Lerp(
                    new Color(1f, 0.85f, 0.15f),
                    new Color(1f, 0.35f, 0.1f),
                    gravityManager.TelegraphNormalized);
            }
        }

        private void EnsureArrow()
        {
            if (_arrow is not null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "TelegraphArrow";
            go.transform.SetParent(transform, false);
            var col = go.GetComponent<Collider>();
            if (col is not null)
            {
                col.enabled = false;
            }

            _renderer = go.GetComponent<Renderer>();
            if (_renderer is not null)
            {
                var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                if (shader is not null)
                {
                    _renderer.sharedMaterial = new Material(shader) { color = new Color(1f, 0.8f, 0.1f) };
                }
            }

            _arrow = go.transform;
        }
    }
}
