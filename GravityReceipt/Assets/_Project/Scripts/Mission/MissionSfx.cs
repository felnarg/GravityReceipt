using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Ding corto al completar un objetivo (procedural, sin assets).
    /// </summary>
    public static class MissionSfx
    {
        private static AudioClip _clip;

        public static void PlayObjective()
        {
            EnsureClip();
            if (_clip == null)
            {
                return;
            }

            var listener = Object.FindAnyObjectByType<AudioListener>();
            var pos = listener != null ? listener.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(_clip, pos, 0.55f);
        }

        private static void EnsureClip()
        {
            if (_clip != null)
            {
                return;
            }

            const int hz = 22050;
            const float duration = 0.22f;
            var samples = Mathf.Max(16, (int)(hz * duration));
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)(samples - 1);
                var env = Mathf.Exp(-t * 6f);
                var a = Mathf.Sin(2f * Mathf.PI * 880f * t * duration);
                var b = Mathf.Sin(2f * Mathf.PI * 1320f * t * duration);
                data[i] = (a * 0.65f + b * 0.35f) * env * 0.4f;
            }

            _clip = AudioClip.Create("ObjectiveDing", samples, 1, hz, false);
            _clip.SetData(data, 0);
        }
    }
}
