using UnityEngine;

namespace GravityReceipt.Player
{
    public enum RoleKind
    {
        Runner = 0,
        Anchor = 1
    }

    /// <summary>
    /// Stub de roles: Runner sprint, Anchor fija g 3 s.
    /// </summary>
    public sealed class PlayerRole : MonoBehaviour
    {
        [SerializeField] private RoleKind role = RoleKind.Runner;
        [SerializeField] private float runnerSprintMultiplier = 1.55f;
        [SerializeField] private float anchorSeconds = 3f;
        [SerializeField] private float anchorCooldown = 8f;

        private LocalPlayerInput _input;
        private PlayerMotor _motor;
        private float _anchorReadyAt;

        public RoleKind Role => role;
        public string RoleLabel => role == RoleKind.Runner ? "Runner" : "Anchor";
        public float AnchorCooldownLeft => Mathf.Max(0f, _anchorReadyAt - Time.time);

        public float MoveMultiplier =>
            role == RoleKind.Runner && _input != null && _input.SprintHeld()
                ? runnerSprintMultiplier
                : 1f;

        public void Configure(RoleKind kind)
        {
            role = kind;
        }

        private void Awake()
        {
            _input = GetComponent<LocalPlayerInput>();
            _motor = GetComponent<PlayerMotor>();
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            if (_input.CycleRolePressed())
            {
                role = role == RoleKind.Runner ? RoleKind.Anchor : RoleKind.Runner;
            }

            if (role != RoleKind.Anchor || !_input.AbilityPressed())
            {
                return;
            }

            if (Time.time < _anchorReadyAt)
            {
                return;
            }

            if (_motor == null || _motor.Gravity == null)
            {
                return;
            }

            _motor.Gravity.AnchorFor(anchorSeconds);
            _anchorReadyAt = Time.time + anchorCooldown;
            SpawnAnchorRing();
        }

        private void SpawnAnchorRing()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "AnchorRing";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            go.transform.localScale = new Vector3(1.7f, 0.04f, 1.7f);
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            var rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    rend.sharedMaterial = new Material(shader) { color = new Color(0.35f, 0.85f, 1f) };
                }
            }

            Destroy(go, anchorSeconds);
        }
    }
}
