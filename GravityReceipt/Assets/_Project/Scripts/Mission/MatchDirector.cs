using System;
using System.Collections;
using GravityReceipt.Player;
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
        public event Action PackageDented;
        public event Action PlayerRespawned;
        public event Action<string> Hint;

        [SerializeField] private float matchSeconds = 600f;
        [SerializeField] private int objectivesToWin = 3;

        private MatchPhase _phase = MatchPhase.Playing;
        private float _remaining;
        private readonly bool[] _objectives = new bool[3];
        private string _endReason = string.Empty;
        private bool _paused;
        private float _splashLeft = 9f;
        private bool _splashEnded;
        private bool _ruleNudgeSent;

        public MatchPhase Phase => _phase;
        public float RemainingSeconds => Mathf.Max(0f, _remaining);
        public bool IsPaused => _paused;
        public bool IsInSplash => _splashLeft > 0f;
        public float SplashSecondsLeft => Mathf.Max(0f, _splashLeft);
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

        public int CurrentObjectiveIndex
        {
            get
            {
                for (var i = 0; i < _objectives.Length; i++)
                {
                    if (!_objectives[i])
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

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
            _splashLeft = 9f;
            _paused = false;
            _splashEnded = false;
            _ruleNudgeSent = false;
            Physics.gravity = Vector3.zero;
            Physics.defaultSolverIterations = 10;
            Physics.defaultSolverVelocityIterations = 4;
            Time.timeScale = 1f;
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            GravityReceipt.Gravity.GravityManager.ResetFlipScreenshotFlag();
            GravityReceipt.UI.FollowBillboard.Hidden = false;
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
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Rematch();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F4) && IsPlaying)
            {
                SkipSplash();
                DebugWarpCheckpoint();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F6) && IsPlaying)
            {
                SkipSplash();
                DebugSkipObjective();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F7) && IsPlaying)
            {
                var pkg = FindAnyObjectByType<MissionPackage>();
                if (pkg != null)
                {
                    pkg.Respawn();
                    Debug.Log("[GravityReceipt] F7 respawn paquete");
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.F3) && IsPlaying)
            {
                DebugUnstuckPlayers();
                return;
            }

            if (Input.GetKeyDown(KeyCode.P) && IsPlaying)
            {
                TogglePause();
                return;
            }

            if (_phase == MatchPhase.Playing)
            {
                if (_splashLeft > 0f)
                {
                    _splashLeft -= Time.deltaTime;
                }
                else
                {
                    if (!_splashEnded)
                    {
                        _splashEnded = true;
                        MissionSfx.PlayObjective();
                    }

                    if (!_ruleNudgeSent && matchSeconds - _remaining >= 8f)
                    {
                        _ruleNudgeSent = true;
                        if (ObjectivesDone == 0)
                        {
                            Hint?.Invoke("Agarrá la taza $15 o la caja $80 · a una PARED");
                        }
                    }

                    _remaining -= Time.deltaTime;
                    if (_remaining <= 0f)
                    {
                        End(MatchPhase.Lost, "Tiempo agotado");
                    }
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

        private void SkipSplash()
        {
            if (_splashLeft <= 0f)
            {
                return;
            }

            _splashLeft = 0f;
            if (!_splashEnded)
            {
                _splashEnded = true;
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
            else
            {
                MissionSfx.PlayObjective();
            }
        }

        public void NotifyPackageDestroyed()
        {
            End(MatchPhase.Lost, "El paquete se destruyó 3 veces");
        }

        public void NotifyPackageDented()
        {
            PackageDented?.Invoke();
        }

        public void NotifyPlayerRespawned()
        {
            PlayerRespawned?.Invoke();
        }

        public void Rematch()
        {
            Time.timeScale = 1f;
            _paused = false;
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        private void TogglePause()
        {
            _paused = !_paused;
            Time.timeScale = _paused ? 0f : 1f;
            if (_paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            Debug.Log(_paused ? "[GravityReceipt] Pausa" : "[GravityReceipt] Reanuda");
        }

        private void DebugUnstuckPlayers()
        {
            var motors = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
            foreach (var motor in motors)
            {
                if (motor != null)
                {
                    motor.NudgeUnstuck();
                }
            }

            Debug.Log("[GravityReceipt] F3 unstuck jugadores");
        }

        /// <summary>
        /// Cheat de playtest: completa el siguiente objetivo y teleporta al checkpoint.
        /// </summary>
        private void DebugSkipObjective()
        {
            var next = -1;
            for (var i = 0; i < _objectives.Length; i++)
            {
                if (!_objectives[i])
                {
                    next = i;
                    break;
                }
            }

            if (next < 0)
            {
                return;
            }

            CompleteObjective(next, next switch
            {
                0 => "Enchufar",
                1 => "Entregar",
                2 => "Sellar",
                _ => "Objetivo"
            });

            if (!IsPlaying)
            {
                return;
            }

            WarpPlayersAndPackage();
            Debug.Log("[GravityReceipt] F6 skip → objetivo " + next);
        }

        private void DebugWarpCheckpoint()
        {
            WarpPlayersAndPackage();
            Debug.Log("[GravityReceipt] F4 warp checkpoint");
        }

        private void WarpPlayersAndPackage()
        {
            var checkpoints = CheckpointSystem.Instance;
            var motors = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
            foreach (var motor in motors)
            {
                if (motor == null)
                {
                    continue;
                }

                var input = motor.GetComponent<LocalPlayerInput>();
                var slot = input != null ? input.Slot : LocalPlayerSlot.One;
                var point = checkpoints != null
                    ? checkpoints.GetPlayerSpawn(slot)
                    : motor.transform.position;
                motor.Warp(point);
            }

            var pkg = FindAnyObjectByType<MissionPackage>();
            if (pkg != null)
            {
                pkg.Respawn();
            }
        }

        private void End(MatchPhase phase, string reason)
        {
            if (_phase != MatchPhase.Playing)
            {
                return;
            }

            _phase = phase;
            _endReason = reason;
            Time.timeScale = 0.22f;
            _paused = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            MatchEnded?.Invoke(phase, reason);
            MissionSfx.PlayEnd(phase == MatchPhase.Won);
            StartCoroutine(CaptureEndShot());
        }

        private IEnumerator CaptureEndShot()
        {
            yield return new WaitForSecondsRealtime(0.28f);
            var name = $"GravityReceipt_end_{System.DateTime.Now:HHmmss}.png";
            ScreenCapture.CaptureScreenshot(name);
            Debug.Log("[GravityReceipt] Fin de partida capturado: " + name);
        }
    }
}
