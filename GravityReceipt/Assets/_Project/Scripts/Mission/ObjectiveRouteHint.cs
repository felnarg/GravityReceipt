using GravityReceipt.Player;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Marco unlit en la puerta hacia el objetivo (no tapa el hueco).
    /// </summary>
    public sealed class ObjectiveRouteHint : MonoBehaviour
    {
        private Transform _ring;
        private Renderer[] _bars;

        private void LateUpdate()
        {
            var match = MatchDirector.Instance;
            if (match == null || !match.IsPlaying || match.IsInSplash)
            {
                Hide();
                return;
            }

            var zDoor = DoorZ(match.CurrentObjectiveIndex);
            if (zDoor < 0f || PlayersPast(zDoor))
            {
                Hide();
                return;
            }

            EnsureRing();
            _ring.gameObject.SetActive(true);
            var pulse = 0.035f * Mathf.Sin(Time.unscaledTime * 4.2f);
            _ring.position = new Vector3(0f, 1.7f, zDoor);
            _ring.localScale = Vector3.one * (1f + pulse);
            if (_bars == null)
            {
                return;
            }

            var t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4.2f);
            var color = Color.Lerp(
                new Color(1f, 0.85f, 0.2f),
                new Color(1f, 0.55f, 0.12f),
                t);
            for (var i = 0; i < _bars.Length; i++)
            {
                if (_bars[i] != null)
                {
                    _bars[i].material.color = color;
                }
            }
        }

        private static float DoorZ(int objectiveIndex)
        {
            return objectiveIndex switch
            {
                0 => 3.95f,
                1 => 16.05f,
                2 => 43.05f,
                _ => -1f
            };
        }

        private static bool PlayersPast(float zDoor)
        {
            var motors = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
            if (motors == null || motors.Length == 0)
            {
                return true;
            }

            foreach (var m in motors)
            {
                if (m != null && m.transform.position.z < zDoor + 1.2f)
                {
                    return false;
                }
            }

            return true;
        }

        private void Hide()
        {
            if (_ring != null)
            {
                _ring.gameObject.SetActive(false);
            }
        }

        private void EnsureRing()
        {
            if (_ring != null)
            {
                return;
            }

            var root = new GameObject("ObjectiveRouteHint");
            root.transform.SetParent(transform, false);
            _ring = root.transform;
            const float w = 2.85f;
            const float h = 3.2f;
            const float t = 0.11f;
            _bars = new[]
            {
                AddBar(root.transform, "L", new Vector3(-w * 0.5f, 0f, 0f), new Vector3(t, h, t)),
                AddBar(root.transform, "R", new Vector3(w * 0.5f, 0f, 0f), new Vector3(t, h, t)),
                AddBar(root.transform, "T", new Vector3(0f, h * 0.5f, 0f), new Vector3(w + t, t, t)),
                AddBar(root.transform, "B", new Vector3(0f, -h * 0.5f, 0f), new Vector3(w + t, t, t))
            };
        }

        private static Renderer AddBar(Transform parent, string name, Vector3 localPos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Route_" + name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            var rend = go.GetComponent<Renderer>();
            if (rend == null)
            {
                return null;
            }

            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (shader != null)
            {
                rend.sharedMaterial = new Material(shader) { color = new Color(1f, 0.8f, 0.2f) };
            }

            return rend;
        }
    }
}
