using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Resalta el valuable dominante con un pulso de escala + color.
    /// </summary>
    public sealed class DominantValuableOutline : MonoBehaviour
    {
        [SerializeField] private GravityManager gravityManager;
        [SerializeField] private Color dominantColor = new(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private float pulseSpeed = 2.5f;

        private ValuableItem _current;
        private Renderer _renderer;
        private Color _baseColor;
        private Vector3 _baseScale;

        public void Bind(GravityManager manager)
        {
            Unbind();
            gravityManager = manager;
            Bind();
        }

        private void Awake()
        {
            if (gravityManager is null)
            {
                gravityManager = GetComponent<GravityManager>();
            }
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
            Clear();
        }

        private void Start()
        {
            if (gravityManager is { Dominant: { } d })
            {
                SetDominant(d);
            }
        }

        private void Update()
        {
            if (gravityManager is { Dominant: { } current } && current != _current)
            {
                SetDominant(current);
            }

            if (_current is null || _renderer is null)
            {
                return;
            }

            if (_current is { IsHeld: true })
            {
                _current.transform.localScale = _baseScale;
                return;
            }

            var pulse = 1f + 0.05f * Mathf.Sin(Time.time * pulseSpeed);
            _current.transform.localScale = _baseScale * pulse;
        }

        private void Bind()
        {
            if (gravityManager is not null)
            {
                gravityManager.GravityChanged += OnGravityChanged;
            }
        }

        private void Unbind()
        {
            if (gravityManager is not null)
            {
                gravityManager.GravityChanged -= OnGravityChanged;
            }
        }

        private void OnGravityChanged(Vector3 _, ValuableItem dominant)
        {
            SetDominant(dominant);
        }

        private void SetDominant(ValuableItem item)
        {
            Clear();
            if (item is null)
            {
                return;
            }

            _current = item;
            _baseScale = item.transform.localScale;
            _renderer = item.GetComponent<Renderer>();
            if (_renderer is not null && _renderer.material is { } mat)
            {
                _baseColor = mat.color;
                mat.color = dominantColor;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", dominantColor * 0.4f);
                }
            }
        }

        private void Clear()
        {
            if (_current is not null)
            {
                _current.transform.localScale = _baseScale;
            }

            if (_renderer is not null && _renderer.material is { } mat)
            {
                mat.color = _baseColor;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }

            _current = null;
            _renderer = null;
        }
    }
}
