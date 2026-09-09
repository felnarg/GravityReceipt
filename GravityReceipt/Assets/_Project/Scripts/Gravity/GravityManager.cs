using System;
using System.Collections;
using System.Collections.Generic;
using GravityReceipt.Mission;
using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Por sala: la gravedad se alinea al eje dominante hacia el valuable más caro.
    /// Empate de precio → gana el último movido.
    /// </summary>
    [DefaultExecutionOrder(-20)]
    public sealed class GravityManager : MonoBehaviour
    {
        public event Action<Vector3, ValuableItem> GravityChanged;

        [SerializeField] private float telegraphSeconds = 1f;
        [SerializeField] private float gravityMagnitude = 9.81f;
        [SerializeField] private Vector3 defaultDown = Vector3.down;
        [SerializeField] private Transform roomCenter;

        private readonly List<ValuableItem> _valuables = new();
        private ValuableItem _dominant;
        private Vector3 _currentGravityDirection = Vector3.down;
        private Vector3 _pendingDirection;
        private float _telegraphRemaining;
        private bool _isTelegraphing;
        private float _anchorUntil;
        private Coroutine _hitStop;
        private float _flipBannerUntil;

        public Vector3 CurrentGravity => _currentGravityDirection * gravityMagnitude;
        public Vector3 CurrentDirection => _currentGravityDirection;
        public ValuableItem Dominant => _dominant;
        public bool IsTelegraphing => _isTelegraphing && !IsAnchored;
        public bool IsAnchored => Time.time < _anchorUntil;
        public bool ShowFlipBanner => IsTelegraphing || Time.unscaledTime < _flipBannerUntil;
        public Vector3 PendingDirection => _pendingDirection;
        public Vector3 BannerDirection => IsTelegraphing ? _pendingDirection : _currentGravityDirection;
        public float TelegraphNormalized =>
            telegraphSeconds <= 0f ? 0f : 1f - Mathf.Clamp01(_telegraphRemaining / telegraphSeconds);

        public void Configure(Transform center, float telegraph = 1f)
        {
            roomCenter = center;
            telegraphSeconds = telegraph;
        }

        public void AnchorFor(float seconds)
        {
            _anchorUntil = Time.time + Mathf.Max(0.1f, seconds);
            _isTelegraphing = false;
        }

        public void Register(ValuableItem item)
        {
            if (item == null || _valuables.Contains(item))
            {
                return;
            }

            _valuables.Add(item);
            RecalculateDominant(immediate: true);
        }

        public void Unregister(ValuableItem item)
        {
            if (!_valuables.Remove(item))
            {
                return;
            }

            if (_dominant == item)
            {
                _dominant = null;
            }

            RecalculateDominant(immediate: true);
        }

        public void NotifyValuableMoved(ValuableItem item)
        {
            if (item == null)
            {
                return;
            }

            RecalculateDominant(immediate: false, movedHint: item);
        }

        private void Update()
        {
            if (IsAnchored || !_isTelegraphing)
            {
                return;
            }

            _telegraphRemaining -= Time.deltaTime;
            if (_telegraphRemaining > 0f)
            {
                return;
            }

            ApplyGravity(_pendingDirection, _dominant);
            _isTelegraphing = false;
        }

        private void RecalculateDominant(bool immediate, ValuableItem movedHint = null)
        {
            if (IsAnchored && !immediate)
            {
                return;
            }

            ValuableItem best = null;
            var bestPrice = int.MinValue;

            for (var i = _valuables.Count - 1; i >= 0; i--)
            {
                var v = _valuables[i];
                if (v == null)
                {
                    _valuables.RemoveAt(i);
                    continue;
                }

                if (!v.IsActiveValuable)
                {
                    continue;
                }

                if (v.Price > bestPrice)
                {
                    bestPrice = v.Price;
                    best = v;
                }
                else if (v.Price == bestPrice && movedHint == v)
                {
                    best = v;
                }
            }

            if (best == null)
            {
                ScheduleOrApply(defaultDown, null, immediate);
                return;
            }

            var direction = DirectionTowardValuable(best);
            ScheduleOrApply(direction, best, immediate);
        }

        private Vector3 DirectionTowardValuable(ValuableItem valuable)
        {
            var center = roomCenter != null ? roomCenter.position : transform.position;
            var toItem = valuable.transform.position - center;
            if (toItem.sqrMagnitude < 0.25f)
            {
                return defaultDown;
            }

            // Bias vertical: suelo/techo no voltean a una pared por un offset X/Z pequeño.
            // En las manos el objeto está a altura de pecho: hace falta más bias o el flip dispara en el centro de la sala.
            toItem.y *= valuable.IsHeld ? 4.4f : 1.75f;

            var ax = Mathf.Abs(toItem.x);
            var ay = Mathf.Abs(toItem.y);
            var az = Mathf.Abs(toItem.z);

            if (ay >= ax && ay >= az)
            {
                return toItem.y >= 0f ? Vector3.up : Vector3.down;
            }

            if (ax >= az)
            {
                return toItem.x >= 0f ? Vector3.right : Vector3.left;
            }

            return toItem.z >= 0f ? Vector3.forward : Vector3.back;
        }

        private void ScheduleOrApply(Vector3 direction, ValuableItem dominant, bool immediate)
        {
            direction = direction.normalized;
            if (immediate || Vector3.Dot(_currentGravityDirection, direction) > 0.99f)
            {
                ApplyGravity(direction, dominant);
                _isTelegraphing = false;
                return;
            }

            if (_isTelegraphing && Vector3.Dot(_pendingDirection, direction) > 0.99f)
            {
                _dominant = dominant;
                return;
            }

            _dominant = dominant;
            _pendingDirection = direction;
            _telegraphRemaining = telegraphSeconds;
            _isTelegraphing = true;
        }

        private void ApplyGravity(Vector3 direction, ValuableItem dominant)
        {
            var previous = _currentGravityDirection;
            _currentGravityDirection = direction.normalized;
            _dominant = dominant;
            if (Vector3.Dot(previous, _currentGravityDirection) < 0.99f)
            {
                _flipBannerUntil = Time.unscaledTime + 0.85f;
                GravityChanged?.Invoke(CurrentGravity, dominant);
                if (isActiveAndEnabled && Time.timeSinceLevelLoad > 1f)
                {
                    if (_hitStop != null)
                    {
                        StopCoroutine(_hitStop);
                    }

                    _hitStop = StartCoroutine(HitStop());
                    StartCoroutine(CaptureFirstFlipDelayed());
                }
            }
        }

        private IEnumerator HitStop()
        {
            var match = MatchDirector.Instance;
            if (match == null || !match.IsPlaying)
            {
                _hitStop = null;
                yield break;
            }

            Time.timeScale = 0.18f;
            yield return new WaitForSecondsRealtime(0.08f);
            if (match != null && match.IsPlaying)
            {
                Time.timeScale = match.IsPaused ? 0f : 1f;
            }

            _hitStop = null;
        }

        public static void ResetFlipScreenshotFlag()
        {
            _flipShotTaken = false;
        }

        private static bool _flipShotTaken;

        private IEnumerator CaptureFirstFlipDelayed()
        {
            if (_flipShotTaken)
            {
                yield break;
            }

            _flipShotTaken = true;
            yield return new WaitForSecondsRealtime(0.16f);
            var name = $"GravityReceipt_flip_{System.DateTime.Now:HHmmss}.png";
            ScreenCapture.CaptureScreenshot(name);
            Debug.Log("[GravityReceipt] Primer flip capturado: " + name);
        }
    }
}
