using GravityReceipt.Interaction;
using GravityReceipt.Mission;
using UnityEngine;

namespace GravityReceipt.Player
{
    /// <summary>
    /// Flecha a los pies: primero el paquete si está lejos; si no, el objetivo actual.
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
            var match = MatchDirector.Instance;
            if (match == null || !match.IsPlaying)
            {
                Hide();
                return;
            }

            if (!TryPickTarget(match, out var worldPos, out var color))
            {
                Hide();
                return;
            }

            var to = worldPos - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.04f)
            {
                Hide();
                return;
            }

            EnsureArrow();
            _arrow.gameObject.SetActive(true);
            var dir = to.normalized;
            var gDir = _motor != null && _motor.Gravity != null
                ? _motor.Gravity.CurrentDirection
                : Vector3.down;
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

        private bool TryPickTarget(MatchDirector match, out Vector3 worldPos, out Color color)
        {
            worldPos = default;
            color = Color.white;

            var pkg = FindAnyObjectByType<MissionPackage>();
            var holdingPkg = _interactor != null && _interactor.IsHoldingPackage;
            if (!holdingPkg && pkg != null)
            {
                var toPkg = pkg.transform.position - transform.position;
                toPkg.y = 0f;
                if (toPkg.sqrMagnitude >= 16f)
                {
                    worldPos = pkg.transform.position;
                    color = new Color(1f, 0.55f, 0.12f);
                    return true;
                }
            }

            var target = FindCurrentObjective(match.CurrentObjectiveIndex);
            if (target == null)
            {
                return false;
            }

            var to = target.transform.position - transform.position;
            to.y = 0f;
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
            return true;
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
