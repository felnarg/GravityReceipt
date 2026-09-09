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
        [SerializeField] private float range = 18f;
        [SerializeField] private float lifetime = 2.2f;
        [SerializeField] private LayerMask mask = ~0;

        private LocalPlayerInput _input;
        private static PingMarker _active;

        private void Awake()
        {
            _input = GetComponent<LocalPlayerInput>();
        }

        private void Update()
        {
            if (_input is not { } || !_input.PingPressed())
            {
                return;
            }

            var cam = _input.PlayerCamera;
            if (cam is null)
            {
                return;
            }

            var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var point = ray.origin + ray.direction * 6f;
            if (Physics.Raycast(ray, out var hit, range, mask, QueryTriggerInteraction.Ignore))
            {
                point = hit.point;
            }

            SpawnMarker(point);
        }

        private static void SpawnMarker(Vector3 point)
        {
            if (_active is not null)
            {
                Destroy(_active.gameObject);
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "PingMarker";
            go.transform.position = point;
            go.transform.localScale = Vector3.one * 0.45f;
            Object.Destroy(go.GetComponent<Collider>());
            var renderer = go.GetComponent<Renderer>();
            if (renderer is not null)
            {
                var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                if (shader is not null)
                {
                    renderer.sharedMaterial = new Material(shader) { color = new Color(1f, 0.85f, 0.15f) };
                }
            }

            var labelGo = new GameObject("PingLabel");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = "¡NO TOQUES ESO!";
            tm.characterSize = 0.08f;
            tm.fontSize = 42;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(1f, 0.95f, 0.35f);

            _active = go.AddComponent<PingMarker>();
            _active.Begin(lifetime);
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
                if (cam is not null)
                {
                    var label = GetComponentInChildren<TextMesh>();
                    if (label is not null)
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
