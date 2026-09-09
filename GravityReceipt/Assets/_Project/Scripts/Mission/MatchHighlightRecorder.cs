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
        private int _flips;
        private float _lastFlip = -10f;

        public float BestFall => _bestFall;
        public LocalPlayerSlot BestSlot => _bestSlot;
        public int Flips => _flips;
        public string Summary
        {
            get
            {
                if (_bestFall >= 0.5f)
                {
                    return $"{_bestFall:0.0} m (P{(int)_bestSlot + 1})"
                           + (_flips > 0 ? $" · {_flips} flip(s)" : string.Empty);
                }

                return _flips > 0 ? $"{_flips} flip(s) de gravedad" : "sin caída destacada";
            }
        }

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

        public void ReportFlip()
        {
            if (Time.unscaledTime - _lastFlip < 0.08f)
            {
                return;
            }

            _lastFlip = Time.unscaledTime;
            _flips++;
        }
    }
}
