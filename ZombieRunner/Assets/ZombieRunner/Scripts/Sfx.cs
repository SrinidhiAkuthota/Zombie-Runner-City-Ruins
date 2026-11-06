using System.Collections.Generic;
using UnityEngine;

namespace ZombieRunner
{
    /// <summary>Tiny procedural sound effects - no audio files needed.</summary>
    public static class Sfx
    {
        static AudioSource src;
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { src = null; clips.Clear(); }

        public static void Init(GameObject host)
        {
            src = host.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.volume = 0.6f;
            clips["jump"] = Tone(260f, 620f, 0.16f, 0.35f, false);
            clips["slide"] = Tone(0f, 0f, 0.22f, 0.15f, true);
            clips["coin"] = Tone(900f, 1500f, 0.12f, 0.30f, false);
            clips["med"] = Tone(500f, 1100f, 0.30f, 0.35f, false);
            clips["hit"] = Tone(150f, 40f, 0.35f, 0.90f, false);
            clips["crunch"] = Tone(0f, 0f, 0.20f, 0.45f, true);
            clips["over"] = Tone(320f, 50f, 1.10f, 0.60f, false);
        }

        public static void Play(string name, float vol = 1f)
        {
            AudioClip c;
            if (src != null && clips.TryGetValue(name, out c)) src.PlayOneShot(c, vol);
        }

        static AudioClip Tone(float f0, float f1, float dur, float vol, bool noise)
        {
            const int rate = 22050;
            int n = Mathf.Max(1, (int)(rate * dur));
            float[] d = new float[n];
            float ph = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float f = Mathf.Lerp(f0, f1, t);
                ph += 2f * Mathf.PI * f / rate;
                float env = (1f - t) * (1f - t);
                float s = noise ? (Random.value * 2f - 1f) : (Mathf.Sin(ph) * 0.6f + Mathf.Sign(Mathf.Sin(ph)) * 0.25f);
                d[i] = s * env * vol;
            }
            AudioClip clip = AudioClip.Create("sfx", n, 1, rate, false);
            clip.SetData(d, 0);
            return clip;
        }
    }
}
