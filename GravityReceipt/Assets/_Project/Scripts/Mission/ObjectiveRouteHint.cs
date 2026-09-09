using GravityReceipt.Player;
using GravityReceipt.World;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Aro unlit en la puerta hacia el objetivo en curso (Hub→Archive→Pasillo→Office→Executive).
    /// </summary>
    public sealed class ObjectiveRouteHint : MonoBehaviour
    {
        private Transform _ring;
        private Renderer _renderer;

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
            var pos = new Vector3(0f, 1.7f, zDoor);
            var up = RoomRegistry.UpAt(pos);
            var pulse = 0.12f * Mathf.Sin(Time.unscaledTime * 4.2f);
            _ring.position = pos + up * (0.15f + pulse);
            _ring.localScale = new Vector3(DoorWFrame(), 3.15f, 0.12f);
            if (_renderer != null)
            {
                var t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4.2f);
                _renderer.material.color = Color.Lerp(
                    new Color(1f, 0.85f, 0.2f),
                    new Color(1f, 0.55f, 0.12f),
                    t);
            }
        }

        private static float DoorWFrame()
        {
            return 2.7f;
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

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "ObjectiveRouteHint";
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
                    _renderer.sharedMaterial = new Material(shader) { color = new Color(1f, 0.8f, 0.2f) };
                }
            }

            _ring = go.transform;
        }
    }
}
