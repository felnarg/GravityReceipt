using GravityReceipt.Interaction;
using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Paquete de misión: 3 vidas por impactos fuertes; a 0 se cuenta una destrucción.
    /// Tres destrucciones = derrota.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Grabbable))]
    public sealed class MissionPackage : MonoBehaviour
    {
        [SerializeField] private int livesPerLife = 3;
        [SerializeField] private int maxDestructions = 3;
        [SerializeField] private float dentImpactSpeed = 7.5f;
        [SerializeField] private float destroyImpactSpeed = 14f;

        private Rigidbody _body;
        private Renderer _renderer;
        private Color _baseColor;
        private int _lives;
        private int _destructions;
        private float _spawnGrace = 1.2f;

        public int Lives => _lives;
        public int LivesMax => livesPerLife;
        public int Destructions => _destructions;
        public int MaxDestructions => maxDestructions;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _renderer = GetComponent<Renderer>();
            if (_renderer is not null)
            {
                _baseColor = _renderer.material.color;
            }

            _lives = livesPerLife;
        }

        private void Update()
        {
            if (_spawnGrace > 0f)
            {
                _spawnGrace -= Time.deltaTime;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision is null || _spawnGrace > 0f)
            {
                return;
            }

            var speed = collision.relativeVelocity.magnitude;
            if (speed < dentImpactSpeed)
            {
                return;
            }

            var loss = speed >= destroyImpactSpeed ? _lives : 1;
            ApplyDamage(loss);
        }

        public void ApplyDamage(int amount)
        {
            if (MatchDirector.Instance == null || !MatchDirector.Instance.IsPlaying)
            {
                return;
            }

            _lives = Mathf.Max(0, _lives - Mathf.Max(1, amount));
            RefreshTint();
            if (_lives > 0)
            {
                return;
            }

            _destructions++;
            if (_destructions >= maxDestructions)
            {
                MatchDirector.Instance.NotifyPackageDestroyed();
                return;
            }

            Respawn();
        }

        public void Respawn()
        {
            var point = CheckpointSystem.Instance is { } cp
                ? cp.PackageSpawn
                : new Vector3(0f, 1f, 0f);

            if (_body is { })
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.position = point;
            }

            transform.SetPositionAndRotation(point, Quaternion.identity);
            _lives = livesPerLife;
            _spawnGrace = 1.2f;
            RefreshTint();
        }

        private void RefreshTint()
        {
            if (_renderer is null)
            {
                return;
            }

            var t = livesPerLife <= 0 ? 0f : _lives / (float)livesPerLife;
            _renderer.material.color = Color.Lerp(new Color(0.45f, 0.12f, 0.08f), _baseColor, t);
        }
    }
}
