using GravityReceipt.UI;
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
        private GameObject _beacon;

        public void Bind(GravityManager manager)
        {
            Unbind();
            gravityManager = manager;
            Bind();
        }

        private void Awake()
        {
            if (gravityManager == null)
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
            if (gravityManager != null && gravityManager.Dominant != null)
            {
                SetDominant(gravityManager.Dominant);
            }
        }

        private void Update()
        {
            if (gravityManager != null && gravityManager.Dominant != _current)
            {
                SetDominant(gravityManager.Dominant);
            }

            if (_current == null || _renderer == null)
            {
                return;
            }

            if (_current.IsHeld)
            {
                _current.transform.localScale = _baseScale;
                return;
            }

            var pulse = 1f + 0.1f * Mathf.Sin(Time.time * pulseSpeed);
            _current.transform.localScale = _baseScale * pulse;
        }

        private void Bind()
        {
            if (gravityManager != null)
            {
                gravityManager.GravityChanged += OnGravityChanged;
            }
        }

        private void Unbind()
        {
            if (gravityManager != null)
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
            if (item == null)
            {
                return;
            }

            _current = item;
            _baseScale = item.transform.localScale;
            _renderer = item.GetComponent<Renderer>();
            if (_renderer != null && _renderer.material != null)
            {
                var mat = _renderer.material;
                _baseColor = mat.color;
                mat.color = dominantColor;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", dominantColor * 0.4f);
                }
            }

            SpawnBeacon(item);
        }

        private void SpawnBeacon(ValuableItem item)
        {
            ClearBeacon();
            _beacon = new GameObject("DominantBeacon");
            if (item.transform.root != null)
            {
                _beacon.transform.SetParent(item.transform.root, true);
            }
            var follow = _beacon.AddComponent<FollowBillboard>();
            var height = _baseScale.y * 0.5f + 0.55f;
            follow.Configure(item.transform, Vector3.up * height);
            WorldLabel.Create(_beacon.transform, "Text", "¡ESTE TIRA DE G!", Vector3.zero, new Color(1f, 0.9f, 0.25f), 0.09f);
        }

        private void Clear()
        {
            ClearBeacon();
            if (_current != null)
            {
                _current.transform.localScale = _baseScale;
            }

            if (_renderer != null && _renderer.material != null)
            {
                var mat = _renderer.material;
                mat.color = _baseColor;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }

            _current = null;
            _renderer = null;
        }

        private void ClearBeacon()
        {
            if (_beacon != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(_beacon);
                }
                else
                {
                    Object.DestroyImmediate(_beacon);
                }

                _beacon = null;
            }
        }
    }
}
