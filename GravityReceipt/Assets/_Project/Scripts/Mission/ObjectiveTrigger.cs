using GravityReceipt.Gravity;
using GravityReceipt.Interaction;
using GravityReceipt.Player;
using GravityReceipt.World;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Objetivo genérico: el paquete (y opcionalmente un jugador) debe permanecer en zona.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    [DefaultExecutionOrder(40)]
    public sealed class ObjectiveTrigger : MonoBehaviour
    {
        [SerializeField] private int objectiveIndex;
        [SerializeField] private string objectiveLabel = "Objetivo";
        [SerializeField] private float requiredSeconds = 0.5f;
        [SerializeField] private bool requirePackage = true;
        [SerializeField] private bool requirePlayer = false;
        [SerializeField] private bool requireHoldInteract = false;

        private BoxCollider _box;
        private float _progress;
        private bool _done;
        private Renderer _renderer;
        private Color _baseColor;
        private Transform _progressBar;
        private Transform _beacon;
        private Renderer _beaconRenderer;
        private GameObject _worldLabel;

        public int Index => objectiveIndex;
        public string Label => objectiveLabel;
        public float ProgressNormalized => requiredSeconds <= 0f ? 1f : Mathf.Clamp01(_progress / requiredSeconds);
        public bool IsDone => _done;

        public void Configure(int index, string label, float seconds, bool package, bool player, bool holdInteract)
        {
            objectiveIndex = index;
            objectiveLabel = label;
            requiredSeconds = seconds;
            requirePackage = package;
            requirePlayer = player;
            requireHoldInteract = holdInteract;
        }

        public void BindWorldLabel(GameObject host)
        {
            _worldLabel = host;
        }

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _baseColor = _renderer.material.color;
            }
        }

        private void Update()
        {
            TickVisual();

            if (_done || MatchDirector.Instance == null || !MatchDirector.Instance.IsPlaying)
            {
                return;
            }

            if (MatchDirector.Instance.IsObjectiveComplete(objectiveIndex))
            {
                MarkCompleteVisual();
                return;
            }

            if (objectiveIndex > 0 && !MatchDirector.Instance.IsObjectiveComplete(objectiveIndex - 1))
            {
                _progress = 0f;
                return;
            }

            if (!ConditionsMet())
            {
                _progress = 0f;
                return;
            }

            _progress += Time.deltaTime;
            if (_progress < requiredSeconds)
            {
                return;
            }

            _done = true;
            MarkCompleteVisual();
            MatchDirector.Instance.CompleteObjective(objectiveIndex, objectiveLabel);
        }

        private void TickVisual()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_done || (MatchDirector.Instance != null && MatchDirector.Instance.IsObjectiveComplete(objectiveIndex)))
            {
                MarkCompleteVisual();
                SetProgressBar(0f);
                SetBeacon(false);
                if (_worldLabel != null)
                {
                    _worldLabel.SetActive(false);
                }
                return;
            }

            var current = MatchDirector.Instance != null
                          && MatchDirector.Instance.CurrentObjectiveIndex == objectiveIndex;
            if (_worldLabel != null)
            {
                _worldLabel.SetActive(current);
            }

            if (current)
            {
                var pulse = 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3.2f));
                _renderer.material.color = Color.Lerp(_baseColor, Color.white, pulse * 0.5f);
                SetProgressBar(_progress);
                SetBeacon(true);
                return;
            }

            _renderer.material.color = Color.Lerp(_baseColor, new Color(0.12f, 0.12f, 0.14f), 0.45f);
            SetProgressBar(0f);
            SetBeacon(false);
        }

        private void SetProgressBar(float progressSeconds)
        {
            var shown = requiredSeconds > 0f ? Mathf.Clamp01(progressSeconds / requiredSeconds) : 0f;
            if (shown <= 0.02f)
            {
                if (_progressBar != null)
                {
                    _progressBar.gameObject.SetActive(false);
                }

                return;
            }

            if (_progressBar == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "ObjectiveProgress";
                go.transform.SetParent(transform.root, true);
                var col = go.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                var rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                    if (shader != null)
                    {
                        rend.sharedMaterial = new Material(shader) { color = new Color(1f, 0.92f, 0.35f) };
                    }
                }

                _progressBar = go.transform;
            }

            _progressBar.gameObject.SetActive(true);
            var up = CurrentUp();
            _progressBar.position = transform.position + up * 0.45f;
            _progressBar.rotation = Quaternion.FromToRotation(Vector3.up, up);
            _progressBar.localScale = new Vector3(Mathf.Max(0.15f, shown * 2.2f), 0.09f, 0.09f);
        }

        private void SetBeacon(bool on)
        {
            if (!on)
            {
                if (_beacon != null)
                {
                    _beacon.gameObject.SetActive(false);
                }

                return;
            }

            if (_beacon == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = "ObjectiveBeacon";
                go.transform.SetParent(transform.root, true);
                var col = go.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                _beaconRenderer = go.GetComponent<Renderer>();
                if (_beaconRenderer != null)
                {
                    _beaconRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                    if (shader != null)
                    {
                        _beaconRenderer.sharedMaterial = new Material(shader)
                        {
                            color = Color.Lerp(_baseColor, Color.white, 0.35f)
                        };
                    }
                }

                _beacon = go.transform;
            }

            _beacon.gameObject.SetActive(true);
            var up = CurrentUp();
            var h = 2.6f + 0.25f * Mathf.Sin(Time.unscaledTime * 4f);
            _beacon.position = transform.position + up * (h * 0.5f + 0.2f);
            _beacon.rotation = Quaternion.FromToRotation(Vector3.up, up);
            _beacon.localScale = new Vector3(0.18f, h * 0.5f, 0.18f);
            if (_beaconRenderer != null)
            {
                var pulse = 0.45f + 0.55f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f));
                var c = Color.Lerp(_baseColor, Color.white, pulse);
                c.a = 1f;
                _beaconRenderer.material.color = c;
            }
        }

        private Vector3 CurrentUp()
        {
            return RoomRegistry.UpAt(transform.position);
        }

        private void MarkCompleteVisual()
        {
            if (!_done)
            {
                GravityFlipBurst.Spawn(transform.position, -CurrentUp(), 8);
            }

            _done = true;
            if (_renderer != null)
            {
                _renderer.material.color = Color.Lerp(_baseColor, new Color(0.15f, 0.15f, 0.15f), 0.65f);
            }
        }

        private void OnDestroy()
        {
            if (_progressBar != null)
            {
                Destroy(_progressBar.gameObject);
                _progressBar = null;
            }

            if (_beacon != null)
            {
                Destroy(_beacon.gameObject);
                _beacon = null;
            }
        }

        private bool ConditionsMet()
        {
            if (requirePackage)
            {
                var pkg = FindAnyObjectByType<MissionPackage>();
                if (pkg == null || !pkg.isActiveAndEnabled || !Contains(pkg.transform.position))
                {
                    return false;
                }
            }

            if (requirePlayer || requireHoldInteract)
            {
                var players = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
                var any = false;
                foreach (var p in players)
                {
                    if (p == null || !Contains(p.transform.position))
                    {
                        continue;
                    }

                    if (requireHoldInteract)
                    {
                        var input = p.GetComponent<LocalPlayerInput>();
                        var inter = p.GetComponent<PlayerInteractor>();
                        var holdingPackage = inter != null && inter.IsHoldingPackage;
                        if (!holdingPackage && (input == null || !input.GrabHeld()))
                        {
                            continue;
                        }
                    }

                    any = true;
                    break;
                }

                if (!any)
                {
                    return false;
                }
            }

            return true;
        }

        private bool Contains(Vector3 worldPos)
        {
            if (_box == null)
            {
                _box = GetComponent<BoxCollider>();
            }

            if (_box == null)
            {
                return false;
            }

            var up = CurrentUp();
            var to = worldPos - transform.position;
            var along = Vector3.Dot(to, up);
            var planar = to - up * along;
            var lossy = transform.lossyScale;
            var radius = Mathf.Max(lossy.x, Mathf.Max(lossy.y, lossy.z)) * 0.85f;
            var minAlong = -Mathf.Max(0.4f, lossy.y * 0.55f);
            var maxAlong = Mathf.Max(3.2f, Mathf.Abs(_box.size.y) * lossy.y * 0.5f);
            return planar.magnitude <= radius && along >= minAlong && along <= maxAlong;
        }
    }
}
