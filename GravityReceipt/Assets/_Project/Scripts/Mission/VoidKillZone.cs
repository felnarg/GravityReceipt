using GravityReceipt.Gravity;
using GravityReceipt.Player;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Si el jugador o un prop se aleja demasiado / cae al vacío, respawnea.
    /// Combina trigger + chequeo por distancia (CharacterController a veces falla triggers).
    /// </summary>
    public sealed class VoidKillZone : MonoBehaviour
    {
        [SerializeField] private float maxDistanceFromOrigin = 80f;
        [SerializeField] private float killY = -4f;

        private void Update()
        {
            var players = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
            foreach (var motor in players)
            {
                if (motor == null)
                {
                    continue;
                }

                var p = motor.transform.position;
                if (p.y < killY || p.magnitude > maxDistanceFromOrigin)
                {
                    RespawnPlayer(motor);
                }
            }

            var pkg = FindAnyObjectByType<MissionPackage>();
            if (pkg != null && (pkg.transform.position.y < killY || pkg.transform.position.magnitude > maxDistanceFromOrigin))
            {
                pkg.Respawn();
            }

            var valuables = FindObjectsByType<ValuableItem>(FindObjectsSortMode.None);
            foreach (var item in valuables)
            {
                if (item == null)
                {
                    continue;
                }

                var p = item.transform.position;
                if (p.y < killY || p.magnitude > maxDistanceFromOrigin)
                {
                    item.ResetToHome();
                }
            }
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
        }
    }
}
