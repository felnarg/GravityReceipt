using UnityEngine;

namespace GravityReceipt.Mission
{
    /// <summary>
    /// Dings procedurales: objetivo, victoria, derrota.
    /// </summary>
    public static class MissionSfx
    {
        private static AudioClip _objective;
        private static AudioClip _win;
        private static AudioClip _lose;

        public static void PlayObjective() => Play(_objective ??= MakeClip("ObjectiveDing", 880f, 1320f, 0.22f));

        public static void PlayEnd(bool won) =>
            Play(won
                ? _win ??= MakeClip("WinFanfare", 523f, 784f, 0.45f)
                : _lose ??= MakeClip("LoseThud", 110f, 73f, 0.4f));

        private static void Play(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            var listener = Object.FindAnyObjectByType<AudioListener>();
            var pos = listener != null ? listener.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(clip, pos, 0.55f);
        }

        private static AudioClip MakeClip(string name, float freqA, float freqB, float duration)
        {
            const int hz = 22050;
            var samples = Mathf.Max(16, (int)(hz * duration));
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)(samples - 1);
                var env = Mathf.Exp(-t * 5.5f);
                var a = Mathf.Sin(2f * Mathf.PI * freqA * t * duration);
                var b = Mathf.Sin(2f * Mathf.PI * freqB * t * duration);
                data[i] = (a * 0.62f + b * 0.38f) * env * 0.4f;
            }

            var clip = AudioClip.Create(name, samples, 1, hz, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
