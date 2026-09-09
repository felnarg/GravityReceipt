using GravityReceipt.Interaction;
using GravityReceipt.Player;
using GravityReceipt.World;
using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Mancha en la pared hacia la que flippearía g si llevas el dominante.
    /// </summary>
    public sealed class PredictedFlipWall : MonoBehaviour
    {
        private PlayerMotor _motor;
        private PlayerInteractor _interactor;
        private Transform _quad;
        private Renderer _renderer;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _interactor = GetComponent<PlayerInteractor>();
        }

        private void LateUpdate()
        {
            var g = _motor != null ? _motor.Gravity : null;
            var holdingDom = _interactor != null
                             && _interactor.HeldValuable != null
                             && g != null
                             && g.Dominant == _interactor.HeldValuable;
            if (!holdingDom)
            {
                Hide();
                return;
            }

            var preview = g.PreviewDirection;
            if (Vector3.Dot(preview, g.CurrentDirection) > 0.99f)
            {
                Hide();
                return;
            }

            var room = RoomRegistry.FindRoom(transform.position);
            if (room == null || !room.HasOwnGravity)
            {
                Hide();
                return;
            }

            EnsureQuad();
            _quad.gameObject.SetActive(true);
            var ext = room.WorldExtents;
            var dir = preview.normalized;
            _quad.position = room.Center + Vector3.Scale(dir, ext) - dir * 0.22f;
            if (Mathf.Abs(dir.x) > 0.5f)
            {
                _quad.localScale = new Vector3(0.08f, 1.9f, 1.9f);
            }
            else if (Mathf.Abs(dir.y) > 0.5f)
            {
                _quad.localScale = new Vector3(1.9f, 0.08f, 1.9f);
            }
            else
            {
                _quad.localScale = new Vector3(1.9f, 1.9f, 0.08f);
            }

            if (_renderer != null)
            {
                var pulse = 0.45f + 0.55f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 8f));
                _renderer.material.color = Color.Lerp(
                    new Color(1f, 0.85f, 0.15f),
                    new Color(1f, 0.4f, 0.1f),
                    pulse);
            }
        }

        private void Hide()
        {
            if (_quad != null)
            {
                _quad.gameObject.SetActive(false);
            }
        }

        private void EnsureQuad()
        {
            if (_quad != null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PredictedFlipWall";
            var floor = GameObject.Find("OfficeFloor");
            if (floor != null)
            {
                go.transform.SetParent(floor.transform, true);
            }
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
                    _renderer.sharedMaterial = new Material(shader) { color = new Color(1f, 0.7f, 0.15f) };
                }
            }

            _quad = go.transform;
        }

        private void OnDestroy()
        {
            if (_quad != null)
            {
                Destroy(_quad.gameObject);
            }
        }
    }
}
