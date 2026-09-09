using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Máquina de estados de la partida: jugar → ganar/perder → rematch (R).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class MatchDirector : MonoBehaviour
    {
        public static MatchDirector Instance { get; private set; }

        public event Action<int, string> ObjectiveCompleted;
        public event Action<MatchPhase, string> MatchEnded;

        [SerializeField] private float matchSeconds = 600f;
        [SerializeField] private int objectivesToWin = 3;

        private MatchPhase _phase = MatchPhase.Playing;
        private float _remaining;
        private readonly bool[] _objectives = new bool[3];
        private string _endReason = string.Empty;

        public MatchPhase Phase => _phase;
        public float RemainingSeconds => Mathf.Max(0f, _remaining);
        public int ObjectivesDone
        {
            get
            {
                var n = 0;
                for (var i = 0; i < _objectives.Length; i++)
                {
                    if (_objectives[i])
                    {
                        n++;
                    }
                }

                return n;
            }
        }

        public int ObjectivesToWin => objectivesToWin;
        public string EndReason => _endReason;
        public bool IsPlaying => _phase == MatchPhase.Playing;

        public void Configure(float seconds, int needed)
        {
            matchSeconds = seconds;
            objectivesToWin = needed;
        }

        private void Awake()
        {
            Instance = this;
            _phase = MatchPhase.Playing;
            _remaining = matchSeconds;
            Physics.gravity = Vector3.zero;
            GravityReceipt.Gravity.GravityManager.ResetFlipScreenshotFlag();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (_phase == MatchPhase.Playing)
            {
                _remaining -= Time.deltaTime;
                if (_remaining <= 0f)
                {
                    End(MatchPhase.Lost, "Tiempo agotado");
                }
            }

            if (_phase is MatchPhase.Won or MatchPhase.Lost)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Rematch();
                }
            }
        }

        public bool IsObjectiveComplete(int index)
        {
            return index >= 0 && index < _objectives.Length && _objectives[index];
        }

        public void CompleteObjective(int index, string label)
        {
            if (!IsPlaying || index < 0 || index >= _objectives.Length || _objectives[index])
            {
                return;
            }

            if (index > 0 && !_objectives[index - 1])
            {
                return;
            }

            _objectives[index] = true;
            var checkpoints = CheckpointSystem.Instance;
            if (checkpoints != null)
            {
                checkpoints.Advance(index);
            }
            ObjectiveCompleted?.Invoke(index, label);

            if (ObjectivesDone >= objectivesToWin)
            {
                End(MatchPhase.Won, "Objetivos completados");
            }
        }

        public void NotifyPackageDestroyed()
        {
            End(MatchPhase.Lost, "El paquete se destruyó 3 veces");
        }

        public void Rematch()
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        private void End(MatchPhase phase, string reason)
        {
            if (_phase != MatchPhase.Playing)
            {
                return;
            }

            _phase = phase;
            _endReason = reason;
            MatchEnded?.Invoke(phase, reason);
        }
    }
}
