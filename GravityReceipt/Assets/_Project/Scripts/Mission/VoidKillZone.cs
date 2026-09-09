using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Si el jugador o un prop se aleja demasiado / cae al vacío, respawnea.
    /// Combina trigger + chequeo por distancia (CharacterController a veces falla triggers).
    /// </summary>
    public sealed class VoidKillZone : MonoBehaviour
    {
        [SerializeField] private Vector3 respawnPoint = new(0f, 0.5f, -3.5f);
        [SerializeField] private float maxDistanceFromOrigin = 14f;
        [SerializeField] private Transform player;

        private void Awake()
        {
            if (player is null)
            {
                var tagged = GameObject.FindWithTag("Player");
                if (tagged is not null)
                {
                    player = tagged.transform;
                }
            }
        }

        private void Update()
        {
            if (player is null)
            {
                return;
            }

            if (player.position.magnitude > maxDistanceFromOrigin || player.position.y < -4f)
            {
                RespawnPlayer(player.gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other is not { })
            {
                return;
            }

            if (other.CompareTag("Player") || other.GetComponent<CharacterController>() is not null)
            {
                RespawnPlayer(other.gameObject);
                return;
            }

            if (other.attachedRigidbody is { } rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = respawnPoint + Vector3.right * Random.Range(-1.5f, 1.5f);
            }
        }

        private void RespawnPlayer(GameObject go)
        {
            var cc = go.GetComponent<CharacterController>();
            if (cc is not null)
            {
                cc.enabled = false;
            }

            go.transform.SetPositionAndRotation(respawnPoint, Quaternion.identity);

            if (cc is not null)
            {
                cc.enabled = true;
            }
        }
    }
}
