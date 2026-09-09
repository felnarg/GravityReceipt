using GravityReceipt.Gravity;
using GravityReceipt.Player;
using GravityReceipt.World;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Si el jugador o un prop se aleja demasiado / cae al vacío, respawnea.
    /// Combina trigger + chequeo por distancia (CharacterController a veces falla triggers).
    /// </summary>
    [DefaultExecutionOrder(50)]
    public sealed class VoidKillZone : MonoBehaviour
    {
        [SerializeField] private float maxDistanceFromOrigin = 80f;
        [SerializeField] private float killY = -4f;

        private float _scanAt;
        private PlayerMotor[] _players;
        private ValuableItem[] _valuables;
        private SpawnHome[] _homes;

        private void Update()
        {
            if (Time.unscaledTime >= _scanAt)
            {
                _scanAt = Time.unscaledTime + 0.12f;
                _players = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
                _valuables = FindObjectsByType<ValuableItem>(FindObjectsSortMode.None);
                _homes = FindObjectsByType<SpawnHome>(FindObjectsSortMode.None);
            }

            var players = _players;
            if (players != null)
            {
                foreach (var motor in players)
                {
                    if (motor == null)
                    {
                        continue;
                    }

                    var p = motor.transform.position;
                    if (IsLost(p))
                    {
                        RespawnPlayer(motor);
                    }
                }
            }

            var pkg = FindAnyObjectByType<MissionPackage>();
            if (pkg != null && IsLost(pkg.transform.position))
            {
                pkg.Respawn();
            }

            var valuables = _valuables;
            if (valuables != null)
            {
                foreach (var item in valuables)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var p = item.transform.position;
                    if (IsLost(p))
                    {
                        item.ResetToHome();
                    }
                }
            }

            var homes = _homes;
            if (homes != null)
            {
                foreach (var home in homes)
                {
                    if (home == null)
                    {
                        continue;
                    }

                    var p = home.transform.position;
                    if (IsLost(p))
                    {
                        home.ReturnHome();
                    }
                }
            }
        }

        private bool IsLost(Vector3 p)
        {
            if (p.y < killY || p.y > 16f || p.magnitude > maxDistanceFromOrigin)
            {
                return true;
            }

            if (Mathf.Abs(p.x) > 9.5f || p.z < -12f || p.z > 68f)
            {
                return true;
            }

            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
            {
                return;
            }

            var motor = other.GetComponentInParent<PlayerMotor>();
            if (motor != null)
            {
                RespawnPlayer(motor);
                return;
            }

            var pkg = other.GetComponentInParent<MissionPackage>();
            if (pkg != null)
            {
                pkg.Respawn();
                return;
            }

            var valuable = other.GetComponentInParent<ValuableItem>();
            if (valuable != null)
            {
                valuable.ResetToHome();
                return;
            }

            var home = other.GetComponentInParent<SpawnHome>();
            if (home != null)
            {
                home.ReturnHome();
                return;
            }

            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                var checkpoints = CheckpointSystem.Instance;
                var spawn = checkpoints != null
                    ? checkpoints.PackageSpawn
                    : new Vector3(0f, 1f, 0f);
                rb.position = spawn + Vector3.right * Random.Range(-1.2f, 1.2f);
            }
        }

        private void RespawnPlayer(PlayerMotor motor)
        {
            var input = motor.GetComponent<LocalPlayerInput>();
            var slot = input != null ? input.Slot : LocalPlayerSlot.One;
            var checkpoints = CheckpointSystem.Instance;
            var point = checkpoints != null
                ? checkpoints.GetPlayerSpawn(slot)
                : new Vector3(0f, 1f, 0f);
            motor.Warp(point);
            if (MatchDirector.Instance != null)
            {
                MatchDirector.Instance.NotifyPlayerRespawned();
            }
        }
    }
}
