using GravityReceipt.Player;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Respawn de jugadores y paquete. Avanza al completar objetivos.
    /// </summary>
    public sealed class CheckpointSystem : MonoBehaviour
    {
        public static CheckpointSystem Instance { get; private set; }

        [SerializeField] private Vector3[] playerSpawns = { new( -1.2f, 1f, -2f), new(1.2f, 1f, -2f) };
        [SerializeField] private Vector3 packageSpawn = new(0f, 0.7f, 0f);

        private readonly Vector3[][] _stages = new Vector3[4][];
        private readonly Vector3[] _packageStages = new Vector3[4];
        private int _stage;

        public Vector3 PackageSpawn => _packageStages[Mathf.Clamp(_stage, 0, _packageStages.Length - 1)];
        public int Stage => _stage;

        public void Configure(Vector3[] hubPlayers, Vector3 hubPackage)
        {
            playerSpawns = hubPlayers;
            packageSpawn = hubPackage;
            _stages[0] = hubPlayers;
            _packageStages[0] = hubPackage;
        }

        public void SetStageSpawns(int stage, Vector3[] players, Vector3 package)
        {
            if (stage < 0 || stage >= _stages.Length)
            {
                return;
            }

            _stages[stage] = players;
            _packageStages[stage] = package;
        }

        private void Awake()
        {
            Instance = this;
            if (_stages[0] is null)
            {
                _stages[0] = playerSpawns;
                _packageStages[0] = packageSpawn;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Advance(int objectiveIndex)
        {
            _stage = Mathf.Max(_stage, objectiveIndex + 1);
        }

        public Vector3 GetPlayerSpawn(LocalPlayerSlot slot)
        {
            var list = _stages[Mathf.Clamp(_stage, 0, _stages.Length - 1)] ?? playerSpawns;
            if (list is not { Length: > 0 })
            {
                return new Vector3(0f, 1f, 0f);
            }

            var i = Mathf.Clamp((int)slot, 0, list.Length - 1);
            return list[i];
        }
    }
}
