using GravityReceipt.Interaction;
using GravityReceipt.Player;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Objetivo genérico: el paquete (y opcionalmente un jugador) debe permanecer en zona.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ObjectiveTrigger : MonoBehaviour
    {
        [SerializeField] private int objectiveIndex;
        [SerializeField] private string objectiveLabel = "Objetivo";
        [SerializeField] private float requiredSeconds = 0.5f;
        [SerializeField] private bool requirePackage = true;
        [SerializeField] private bool requirePlayer = false;
        [SerializeField] private bool requireHoldInteract = false;

        private BoxCollider _box;
        private float _progress;
        private bool _done;

        public int Index => objectiveIndex;
        public string Label => objectiveLabel;
        public float ProgressNormalized => requiredSeconds <= 0f ? 1f : Mathf.Clamp01(_progress / requiredSeconds);
        public bool IsDone => _done;

        public void Configure(int index, string label, float seconds, bool package, bool player, bool holdInteract)
        {
            objectiveIndex = index;
            objectiveLabel = label;
            requiredSeconds = seconds;
            requirePackage = package;
            requirePlayer = player;
            requireHoldInteract = holdInteract;
        }

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;
        }

        private void Update()
        {
            if (_done || MatchDirector.Instance == null || !MatchDirector.Instance.IsPlaying)
            {
                return;
            }

            if (objectiveIndex > 0 && !MatchDirector.Instance.IsObjectiveComplete(objectiveIndex - 1))
            {
                _progress = 0f;
                return;
            }

            if (!ConditionsMet())
            {
                _progress = 0f;
                return;
            }

            _progress += Time.deltaTime;
            if (_progress < requiredSeconds)
            {
                return;
            }

            _done = true;
            MatchDirector.Instance.CompleteObjective(objectiveIndex, objectiveLabel);
        }

        private bool ConditionsMet()
        {
            if (requirePackage)
            {
                var pkg = FindAnyObjectByType<MissionPackage>();
                if (pkg == null || !pkg.isActiveAndEnabled || !Contains(pkg.transform.position))
                {
                    return false;
                }
            }

            if (requirePlayer || requireHoldInteract)
            {
                var players = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
                var any = false;
                foreach (var p in players)
                {
                    if (p == null || !Contains(p.transform.position))
                    {
                        continue;
                    }

                    if (requireHoldInteract)
                    {
                        var input = p.GetComponent<LocalPlayerInput>();
                        var inter = p.GetComponent<PlayerInteractor>();
                        var holdingPackage = inter != null && inter.IsHoldingPackage;
                        if (!holdingPackage && (input == null || !input.GrabHeld()))
                        {
                            continue;
                        }
                    }

                    any = true;
                    break;
                }

                if (!any)
                {
                    return false;
                }
            }

            return true;
        }

        private bool Contains(Vector3 worldPos)
        {
            if (_box == null)
            {
                _box = GetComponent<BoxCollider>();
            }

            if (_box == null)
            {
                return false;
            }

            var local = transform.InverseTransformPoint(worldPos);
            var e = _box.size * 0.5f;
            var c = _box.center;
            local -= c;
            return Mathf.Abs(local.x) <= e.x
                   && Mathf.Abs(local.y) <= e.y
                   && Mathf.Abs(local.z) <= e.z;
        }
    }
}
