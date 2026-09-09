using GravityReceipt.Mission;
using GravityReceipt.UI;
using UnityEngine;

namespace GravityReceipt.Player
{
    /// <summary>
    /// 4 emotes locales (sem 5.7): OK / NO / ? / ¡AQUÍ!
    /// </summary>
    public sealed class PlayerEmote : MonoBehaviour
    {
        private static readonly string[] Labels = { "OK", "NO", "?", "¡AQUÍ!" };
        private static readonly Color[] Colors =
        {
            new(0.45f, 1f, 0.55f),
            new(1f, 0.4f, 0.38f),
            new(1f, 0.92f, 0.35f),
            new(0.55f, 0.85f, 1f)
        };

        [SerializeField] private float cooldown = 1.15f;
        [SerializeField] private float lifetime = 2.1f;

        private LocalPlayerInput _input;
        private float _readyAt;
        private GameObject _bubble;

        private void Awake()
        {
            _input = GetComponent<LocalPlayerInput>();
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            var match = MatchDirector.Instance;
            if (match != null && !match.IsPlaying)
            {
                return;
            }

            var index = _input.EmotePressed();
            if (index < 0 || Time.unscaledTime < _readyAt)
            {
                return;
            }

            Show(index);
            _readyAt = Time.unscaledTime + cooldown;
        }

        private void Show(int index)
        {
            if (index < 0 || index >= Labels.Length)
            {
                return;
            }

            Clear();
            _bubble = new GameObject("EmoteBubble");
            var floor = GameObject.Find("OfficeFloor");
            if (floor != null)
            {
                _bubble.transform.SetParent(floor.transform, true);
            }

            var follow = _bubble.AddComponent<FollowBillboard>();
            follow.Configure(transform, Vector3.up * 2.35f);
            WorldLabel.Create(_bubble.transform, "Text", Labels[index], Vector3.zero, Colors[index], 0.14f);
            Destroy(_bubble, lifetime);
            MissionSfx.PlayEmote(index);
        }

        private void Clear()
        {
            if (_bubble != null)
            {
                Destroy(_bubble);
                _bubble = null;
            }
        }

        private void OnDisable()
        {
            Clear();
        }
    }
}
