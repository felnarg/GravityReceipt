using UnityEngine;

namespace GravityReceipt.Interaction
{
    /// <summary>
    /// Marca un Rigidbody que se puede agarrar (valuables, paquete, props).
    /// Un solo holder a la vez (split 2p).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Grabbable : MonoBehaviour
    {
        [SerializeField] private bool enabledGrab = true;

        private Rigidbody _body;
        private int _holders;

        public bool CanGrab => enabledGrab && isActiveAndEnabled && _holders == 0;

        public Rigidbody Body => _body != null ? _body : (_body = GetComponent<Rigidbody>());

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        public void BeginGrab()
        {
            _holders++;
        }

        public void EndGrab()
        {
            _holders = Mathf.Max(0, _holders - 1);
        }
    }
}
