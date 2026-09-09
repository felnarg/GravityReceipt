using GravityReceipt.Gravity;
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

        public float MoveMultiplier =>
            role == RoleKind.Runner && _input is { } && _input.SprintHeld()
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
            if (_input is not { })
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

            if (_motor is not { Gravity: { } gravity })
            {
                return;
            }

            gravity.AnchorFor(anchorSeconds);
            _anchorReadyAt = Time.time + anchorCooldown;
        }
    }
}
