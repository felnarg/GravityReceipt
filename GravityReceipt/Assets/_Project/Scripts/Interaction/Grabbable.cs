using UnityEngine;

namespace GravityReceipt.Interaction
{
    /// <summary>
    /// Marca un Rigidbody que se puede agarrar (valuables, paquete, props).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Grabbable : MonoBehaviour
    {
        [SerializeField] private bool enabledGrab = true;

        public bool CanGrab => enabledGrab && isActiveAndEnabled;

        public Rigidbody Body => _body != null ? _body : (_body = GetComponent<Rigidbody>());

        private Rigidbody _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }
    }
}
