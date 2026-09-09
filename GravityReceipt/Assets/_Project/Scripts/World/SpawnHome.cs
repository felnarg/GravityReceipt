using UnityEngine;

namespace GravityReceipt.World
{
    /// <summary>
    /// Props (y opcionalmente otros rigidbodies) vuelven a su spawn si caen al vacío.
    /// </summary>
    public sealed class SpawnHome : MonoBehaviour
    {
        private Vector3 _pos;
        private Quaternion _rot;
        private bool _captured;
        private Rigidbody _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            Capture();
        }

        public void Capture()
        {
            _pos = transform.position;
            _rot = transform.rotation;
            _captured = true;
        }

        public void ReturnHome()
        {
            if (_body != null && _body.isKinematic)
            {
                return;
            }

            if (!_captured)
            {
                Capture();
            }

            if (_body != null)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.position = _pos;
                _body.rotation = _rot;
            }

                transform.SetPositionAndRotation(_pos, _rot);
            }
        }
    }
}
