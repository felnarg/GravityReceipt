using GravityReceipt.Gravity;
using GravityReceipt.Interaction;
using GravityReceipt.Mission;
using UnityEngine;

namespace GravityReceipt.Player
{
    /// <summary>
    /// Flecha a los pies: taza tutorial en Hub; paquete si está lejos; si no, el objetivo.
    /// </summary>
    public sealed class ObjectiveWaypoint : MonoBehaviour
    {
        private Transform _arrow;
        private Renderer _renderer;
        private PlayerMotor _motor;
        private PlayerInteractor _interactor;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _interactor = GetComponent<PlayerInteractor>();
        }

        private void LateUpdate()
        {
            if (_interactor == null)
            {
                _interactor = GetComponent<PlayerInteractor>();
            }

            if (_motor == null)
            {
                _motor = GetComponent<PlayerMotor>();
            }

            var match = MatchDirector.Instance;
            if (match == null || !match.IsPlaying)
            {
                Hide();
                return;
            }

            if (!TryPickTarget(match, out var worldPos, out var color, out _))
            {
                Hide();
                return;
            }

            var gDir = CurrentGDir();
            var to = Vector3.ProjectOnPlane(worldPos - transform.position, gDir);
            if (to.sqrMagnitude < 0.04f)
            {
                Hide();
                return;
            }

            EnsureArrow();
            _arrow.gameObject.SetActive(true);
            var dir = to.normalized;
            _arrow.position = transform.position + (-gDir) * 0.12f + dir * 0.85f;
            var up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.95f ? Vector3.forward : Vector3.up;
            _arrow.rotation = Quaternion.LookRotation(dir, up);
            var len = Mathf.Clamp(0.28f + to.magnitude * 0.02f, 0.32f, 0.55f);
            _arrow.localScale = new Vector3(0.11f, 0.11f, len);
            if (_renderer != null)
            {
                _renderer.material.color = color;
            }
        }

        public static bool TryPeekTarget(PlayerMotor motor, out Vector3 worldPos, out Color color, out string label)
        {
            worldPos = default;
            color = Color.white;
            label = string.Empty;
            var match = MatchDirector.Instance;
            if (motor == null || match == null || !match.IsPlaying)
            {
                return false;
            }

            var interactor = motor.GetComponent<PlayerInteractor>();
            return TryPickTarget(
                match,
                motor.transform.position,
                interactor,
                motor.Gravity,
                out worldPos,
                out color,
                out label);
        }

        private bool TryPickTarget(MatchDirector match, out Vector3 worldPos, out Color color, out string label)
        {
            return TryPickTarget(
                match,
                transform.position,
                _interactor,
                _motor != null ? _motor.Gravity : null,
                out worldPos,
                out color,
                out label);
        }

        private static bool TryPickTarget(
            MatchDirector match,
            Vector3 from,
            PlayerInteractor interactor,
            GravityManager gravity,
            out Vector3 worldPos,
            out Color color,
            out string label)
        {
            worldPos = default;
            color = Color.white;
            label = string.Empty;

            var held = interactor != null ? interactor.HeldValuable : null;
            if (held != null && held.Manager != null && held.Manager.Dominant == held)
            {
                return false;
            }

            var gDir = gravity != null ? gravity.CurrentDirection : Vector3.down;

            if (match.ObjectivesDone == 0)
            {
                var mug = FindTutorialMug();
                var holdingMug = held != null && held == mug;
                if (mug != null && !holdingMug && from.z < 8f)
                {
                    worldPos = mug.transform.position;
                    color = new Color(1f, 0.82f, 0.2f);
                    label = "TAZA $15";
                    return true;
                }
            }

            var pkg = FindAnyObjectByType<MissionPackage>();
            var holdingPkg = interactor != null && interactor.IsHoldingPackage;
            if (!holdingPkg && pkg != null)
            {
                var toPkg = Vector3.ProjectOnPlane(pkg.transform.position - from, gDir);
                if (toPkg.sqrMagnitude >= 16f)
                {
                    worldPos = pkg.transform.position;
                    color = new Color(1f, 0.55f, 0.12f);
                    label = "PAQUETE";
                    return true;
                }
            }

            var target = FindCurrentObjective(match.CurrentObjectiveIndex);
            if (target == null)
            {
                return false;
            }

            var to = Vector3.ProjectOnPlane(target.transform.position - from, gDir);
            if (to.sqrMagnitude < 12.25f)
            {
                return false;
            }

            worldPos = target.transform.position;
            color = match.CurrentObjectiveIndex switch
            {
                0 => new Color(0.35f, 1f, 0.65f),
                1 => new Color(0.4f, 0.7f, 1f),
                _ => new Color(1f, 0.82f, 0.25f)
            };
            label = match.CurrentObjectiveIndex switch
            {
                0 => "ENCHUFAR",
                1 => "ENTREGAR",
                _ => "SELLAR"
            };
            return true;
        }

        private Vector3 CurrentGDir()
        {
            return _motor != null && _motor.Gravity != null
                ? _motor.Gravity.CurrentDirection
                : Vector3.down;
        }

        private static ObjectiveTrigger FindCurrentObjective(int index)
        {
            if (index < 0)
            {
                return null;
            }

            var triggers = FindObjectsByType<ObjectiveTrigger>(FindObjectsSortMode.None);
            foreach (var t in triggers)
            {
                if (t != null && t.Index == index && !t.IsDone)
                {
                    return t;
                }
            }

            return null;
        }

        private static ValuableItem _cachedMug;
        private static float _mugCacheUntil;

        private static ValuableItem FindTutorialMug()
        {
            if (_cachedMug != null && Time.unscaledTime < _mugCacheUntil)
            {
                return _cachedMug;
            }

            _mugCacheUntil = Time.unscaledTime + 0.5f;
            var items = FindObjectsByType<ValuableItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                if (item != null && item.Price == 15)
                {
                    _cachedMug = item;
                    return item;
                }
            }

            _cachedMug = null;
            return null;
        }

        private void Hide()
        {
            if (_arrow != null)
            {
                _arrow.gameObject.SetActive(false);
            }
        }

        private void EnsureArrow()
        {
            if (_arrow != null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "ObjectiveWaypoint";
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
                    _renderer.sharedMaterial = new Material(shader) { color = Color.white };
                }
            }

            _arrow = go.transform;
        }
    }
}
