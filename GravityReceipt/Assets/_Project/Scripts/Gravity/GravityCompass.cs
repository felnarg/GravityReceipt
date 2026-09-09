using GravityReceipt.Player;
using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Flecha local a los pies: hacia dónde tira g ahora (o el telegráfo).
    /// </summary>
    public sealed class GravityCompass : MonoBehaviour
    {
        private PlayerMotor _motor;
        private Transform _arrow;
        private Renderer _renderer;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
        }

        private void LateUpdate()
        {
            if (_motor == null)
            {
                return;
            }

            var g = _motor.Gravity;
            if (g == null)
            {
                if (_arrow != null)
                {
                    _arrow.gameObject.SetActive(false);
                }

                return;
            }

            EnsureArrow();
            _arrow.gameObject.SetActive(true);
            var dir = g.IsTelegraphing ? g.PendingDirection : g.CurrentDirection;
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = Vector3.down;
            }

            _arrow.position = transform.position - dir * 0.55f;
            var up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.95f ? Vector3.forward : Vector3.up;
            _arrow.rotation = Quaternion.LookRotation(dir, up);
            _arrow.localScale = new Vector3(0.18f, 0.18f, 0.7f);
            if (_renderer != null)
            {
                _renderer.material.color = g.IsTelegraphing
                    ? Color.Lerp(new Color(1f, 0.9f, 0.2f), new Color(1f, 0.35f, 0.1f), g.TelegraphNormalized)
                    : new Color(0.95f, 0.95f, 0.95f, 0.85f);
            }
        }

        private void EnsureArrow()
        {
            if (_arrow != null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "GravityCompass";
            go.transform.SetParent(transform, false);
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            _renderer = go.GetComponent<Renderer>();
            if (_renderer != null)
            {
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
