using GravityReceipt.Mission;
using GravityReceipt.Player;
using GravityReceipt.UI;
using UnityEngine;

namespace GravityReceipt.Interaction
{
    /// <summary>
    /// Ping radial: marca un punto del mundo ("¡no toques eso!").
    /// </summary>
    public sealed class PlayerPing : MonoBehaviour
    {
        [SerializeField] private float range = 22f;
        [SerializeField] private float lifetime = 2.2f;
        [SerializeField] private float cooldown = 0.85f;
        [SerializeField] private LayerMask mask = ~0;

        private LocalPlayerInput _input;
        private PingMarker _mine;
        private float _readyAt;

        private void Awake()
        {
            _input = GetComponent<LocalPlayerInput>();
        }

        private void Update()
        {
            if (_input == null || !_input.PingPressed())
            {
                return;
            }

            if (Time.unscaledTime < _readyAt)
            {
                return;
            }

            var cam = _input.PlayerCamera;
            if (cam == null)
            {
                return;
            }

            _readyAt = Time.unscaledTime + cooldown;

            var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var point = ray.origin + ray.direction * 6f;
            if (Physics.Raycast(ray, out var hit, range, mask, QueryTriggerInteraction.Ignore))
            {
                point = hit.point;
            }

            SpawnMarker(point);
        }

        private void SpawnMarker(Vector3 point)
        {
            if (_mine != null)
            {
                Destroy(_mine.gameObject);
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "PingMarker";
            var floor = GameObject.Find("OfficeFloor");
            if (floor != null)
            {
                go.transform.SetParent(floor.transform, true);
            }
            go.transform.position = point;
            go.transform.localScale = Vector3.one * 0.45f;
            Object.Destroy(go.GetComponent<Collider>());
            var renderer = go.GetComponent<Renderer>();
            var color = _input != null && _input.Slot == LocalPlayerSlot.Two
                ? new Color(1f, 0.55f, 0.2f)
                : new Color(1f, 0.85f, 0.15f);
            if (renderer != null)
            {
                var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    renderer.sharedMaterial = new Material(shader) { color = color };
                }
            }

            var labelGo = new GameObject("PingLabel");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = _input != null && _input.Slot == LocalPlayerSlot.Two
                ? "P2 ¡NO TOQUES ESO!"
                : "P1 ¡NO TOQUES ESO!";
            tm.characterSize = 0.08f;
            tm.fontSize = 42;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;

            _mine = go.AddComponent<PingMarker>();
            _mine.Begin(lifetime);
            MissionSfx.PlayPing();
        }

        private void OnDisable()
        {
            if (_mine != null)
            {
                Destroy(_mine.gameObject);
                _mine = null;
            }
        }

        private sealed class PingMarker : MonoBehaviour
        {
            private float _dieAt;

            public void Begin(float life)
            {
                _dieAt = Time.time + life;
            }

            private void Update()
            {
                transform.localScale = Vector3.one * (0.45f + 0.08f * Mathf.Sin(Time.time * 10f));
                var cam = FollowBillboard.ClosestCamera(transform.position);
                if (cam != null)
                {
                    var label = GetComponentInChildren<TextMesh>();
                    if (label != null)
                    {
                        label.transform.rotation = Quaternion.LookRotation(
                            label.transform.position - cam.transform.position);
                    }
                }

                if (Time.time >= _dieAt)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
