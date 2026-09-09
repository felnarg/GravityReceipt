using GravityReceipt.Player;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Moment of the Match local: mayor caída medida.
    /// </summary>
    public sealed class MatchHighlightRecorder : MonoBehaviour
    {
        public static MatchHighlightRecorder Instance { get; private set; }

        private float _bestFall;
        private LocalPlayerSlot _bestSlot;

        public float BestFall => _bestFall;
        public LocalPlayerSlot BestSlot => _bestSlot;
        public string Summary =>
            _bestFall < 0.5f
                ? "sin caída destacada"
                : $"{_bestFall:0.0} m (P{(int)_bestSlot + 1})";

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ReportFall(LocalPlayerSlot slot, float meters)
        {
            if (meters <= _bestFall)
            {
                return;
            }

            _bestFall = meters;
            _bestSlot = slot;
        }
    }
}
