using GravityReceipt.Interaction;
using GravityReceipt.UI;
using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Mancha unlit en la cara que ahora es "abajo" (tras un flip se pega a la pared/techo).
    /// </summary>
    public sealed class GravityDownPad : MonoBehaviour
    {
        private GravityManager _gravity;
        private Transform _pad;
        private Renderer _renderer;

        private void Awake()
        {
            _gravity = GetComponent<GravityManager>();
        }

        private void LateUpdate()
        {
            if (_gravity == null)
            {
                return;
            }

            EnsurePad();
            var dir = _gravity.CurrentDirection;
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = Vector3.down;
            }

            var unusual = _gravity.IsAnchored
                          || _gravity.IsTelegraphing
                          || Vector3.Dot(dir, Vector3.down) < 0.92f;
            if (!unusual || !TryHitDown(dir, out var point))
            {
                _pad.gameObject.SetActive(false);
                return;
            }

            _pad.gameObject.SetActive(true);
            _pad.position = point - dir * 0.05f;
            var up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.92f ? Vector3.forward : Vector3.up;
            _pad.rotation = Quaternion.LookRotation(dir, up);
            _pad.localScale = new Vector3(2.35f, 2.35f, 0.045f);
            if (_renderer == null)
            {
                return;
            }

            Color color;
            if (_gravity.IsAnchored)
            {
                color = new Color(0.3f, 0.8f, 1f, 1f);
            }
            else if (_gravity.IsTelegraphing)
            {
                color = Color.Lerp(new Color(0.25f, 0.85f, 1f), new Color(1f, 0.45f, 0.12f), _gravity.TelegraphNormalized);
            }
            else
            {
                color = new Color(0.2f, 0.78f, 0.95f, 1f);
            }

            _renderer.material.color = color;
        }

        private bool TryHitDown(Vector3 dir, out Vector3 point)
        {
            point = Vector3.zero;
            var origin = transform.position - dir * 0.35f;
            var hits = Physics.RaycastAll(origin, dir, 14f, ~0, QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            var best = float.PositiveInfinity;
            var found = false;
            foreach (var hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                if (hit.collider.GetComponent<CharacterController>() != null)
                {
                    continue;
                }

                if (hit.collider.GetComponent<ValuableItem>() != null
                    || hit.collider.GetComponent<Grabbable>() != null)
                {
                    continue;
                }

                if (hit.distance < best)
                {
                    best = hit.distance;
                    point = hit.point;
                    found = true;
                }
            }

            return found;
        }

        private void EnsurePad()
        {
            if (_pad != null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "GravityDownPad";
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
                    _renderer.sharedMaterial = new Material(shader) { color = new Color(0.2f, 0.78f, 0.95f) };
                }
            }

            _pad = go.transform;

            var word = new GameObject("DownWord");
            word.transform.SetParent(_pad, false);
            word.transform.localPosition = new Vector3(0f, 0f, -0.12f);
            word.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            WorldLabel.Create(word.transform, "Text", "ABAJO", Vector3.zero, new Color(0.35f, 0.95f, 1f), 0.13f);
        }
    }
}
