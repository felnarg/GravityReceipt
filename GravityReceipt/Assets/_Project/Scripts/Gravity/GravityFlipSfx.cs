using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Whoosh procedural al flippear g (sin assets de audio).
    /// </summary>
    public static class GravityFlipSfx
    {
        private static AudioClip _clip;
        private static float _lastPlay = -10f;

        public static void Play()
        {
            if (Time.unscaledTime - _lastPlay < 0.08f)
            {
                return;
            }

            _lastPlay = Time.unscaledTime;
            EnsureClip();
            if (_clip == null)
            {
                return;
            }

            var listener = Object.FindAnyObjectByType<AudioListener>();
            var pos = listener != null ? listener.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(_clip, pos, 0.65f);
        }

        private static void EnsureClip()
        {
            if (_clip != null)
            {
                return;
            }

            const int hz = 22050;
            const float duration = 0.38f;
            var samples = Mathf.Max(16, (int)(hz * duration));
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)(samples - 1);
                var freq = Mathf.Lerp(220f, 48f, t * t);
                var env = Mathf.Sin(t * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * env * 0.35f;
            }

            _clip = AudioClip.Create("GravityFlipWhoosh", samples, 1, hz, false);
            _clip.SetData(data, 0);
        }
    }
}
