using System.Collections;
using UnityEngine;

namespace MoodieLand
{
    /// <summary>
    /// Plays the imported background music and synthesises sound effects at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        public const string MusicVolumeKey = "MoodieLand.musicVolume";
        public const string SfxVolumeKey = "MoodieLand.sfxVolume";

        [Range(0f, 1f)] public float musicVolume = 0.35f;
        [Range(0f, 1f)] public float sfxVolume = 0.7f;

        AudioSource music;
        AudioSource sfx;
        AudioClip click, pop, select, success, softError, whoosh, badge;
        bool ready;

        void Awake()
        {
            if (I != null && I != this) { Destroy(this); return; }
            I = this;
            DontDestroyOnLoad(gameObject);

            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.35f);
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.7f);

            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            music.volume = musicVolume;
            music.spatialBlend = 0f;

            sfx = gameObject.AddComponent<AudioSource>();
            sfx.loop = false;
            sfx.playOnAwake = false;
            sfx.volume = sfxVolume;
            sfx.spatialBlend = 0f;

            StartCoroutine(BuildAndPlay());
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }

        IEnumerator BuildAndPlay()
        {
            yield return null;   // avoid a hitch in the first frame

            click = Synth.Trim(Synth.Mix(
                Synth.Sine(760f, 0.07f, 0.5f, 0.003f, 0.06f),
                Synth.Sine(1180f, 0.04f, 0.18f, 0.001f, 0.03f)), 0.5f);

            pop = Synth.Sweep(430f, 900f, 0.13f, 0.4f);

            select = Synth.Trim(Synth.Mix(
                Synth.Sine(660f, 0.09f, 0.35f, 0.004f, 0.08f),
                Synth.Sine(990f, 0.12f, 0.25f, 0.01f, 0.1f)), 0.6f);

            success = Synth.Arpeggio(new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.1f, 0.55f, 0.45f);
            badge = Synth.Arpeggio(new[] { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f }, 0.12f, 0.9f, 0.6f);
            softError = Synth.Sine(300f, 0.16f, 0.3f, 0.005f, 0.14f);
            whoosh = Synth.Noise(0.22f, 0.12f);

            music.clip = Resources.Load<AudioClip>("Audio/calm bgm");
            music.volume = musicVolume;
            music.Play();
            ready = true;
        }

        void Update()
        {
            if (music != null) music.volume = musicVolume;
            if (sfx != null) sfx.volume = sfxVolume;
        }

        // ------------------------------------------------------------------ api

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
            if (music != null) music.volume = musicVolume;
        }

        public void SetSfxVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            if (sfx != null) sfx.volume = sfxVolume;
        }

        public void PlayClick() => Play(click);
        public void PlayPop() => Play(pop);
        public void PlaySelect() => Play(select);
        public void PlaySuccess() => Play(success);
        public void PlayBadge() => Play(badge);
        public void PlaySoftError() => Play(softError);
        public void PlayWhoosh() => Play(whoosh);

        void Play(AudioClip clip)
        {
            if (!ready || clip == null || sfx == null) return;
            sfx.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>Tiny synthesiser helpers for the generated soundtrack and effects.</summary>
    public static class Synth
    {
        public const int Rate = 44100;

        public static AudioClip Sine(float freq, float dur, float amp, float attack, float release)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(Rate * dur));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float env = Envelope(t, dur, attack, release);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * amp * env
                        + Mathf.Sin(4f * Mathf.PI * freq * t) * amp * 0.12f * env;
            }
            return Make("sine", data);
        }

        public static AudioClip Sweep(float f0, float f1, float dur, float amp)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(Rate * dur));
            var data = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float k = t / dur;
                float f = Mathf.Lerp(f0, f1, k * k);
                phase += 2f * Mathf.PI * f / Rate;
                data[i] = Mathf.Sin(phase) * amp * Envelope(t, dur, 0.004f, dur * 0.85f);
            }
            return Make("sweep", data);
        }

        public static AudioClip Noise(float dur, float amp)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(Rate * dur));
            var data = new float[n];
            float last = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float white = Random.Range(-1f, 1f);
                last = Mathf.Lerp(white, last, 0.72f);          // gentle low pass
                data[i] = last * amp * Envelope(t, dur, 0.02f, dur * 0.6f);
            }
            return Make("noise", data);
        }

        public static AudioClip Arpeggio(float[] notes, float step, float dur, float amp)
        {
            float total = step * notes.Length + dur;
            int n = Mathf.RoundToInt(Rate * total);
            var data = new float[n];
            for (int k = 0; k < notes.Length; k++)
            {
                int offset = Mathf.RoundToInt(Rate * step * k);
                int len = Mathf.RoundToInt(Rate * dur);
                for (int i = 0; i < len && offset + i < n; i++)
                {
                    float t = i / (float)Rate;
                    float env = Envelope(t, dur, 0.008f, dur * 0.9f);
                    data[offset + i] += Mathf.Sin(2f * Mathf.PI * notes[k] * t) * amp * env * 0.7f;
                    data[offset + i] += Mathf.Sin(2f * Mathf.PI * notes[k] * 2f * t) * amp * env * 0.12f;
                }
            }
            return Make("arp", data);
        }

        public static AudioClip Mix(params AudioClip[] clips)
        {
            int n = 0;
            foreach (var c in clips) n = Mathf.Max(n, c.samples);
            var data = new float[n];
            foreach (var c in clips)
            {
                var src = new float[c.samples];
                c.GetData(src, 0);
                for (int i = 0; i < src.Length; i++) data[i] += src[i];
            }
            return Make("mix", data);
        }

        public static AudioClip Trim(AudioClip clip, float amp) => Scale(clip, amp);

        public static AudioClip Scale(AudioClip clip, float amp)
        {
            int n = clip.samples;
            var data = new float[n];
            clip.GetData(data, 0);
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(data[i] * amp, -1f, 1f);
            return Make(clip.name, data);
        }

        /// <summary>A slow music-box loop in C major, one bar per chord, seamless.</summary>
        public static AudioClip MusicBoxLoop()
        {
            const float bpm = 72f;
            float beat = 60f / bpm;
            float bar = beat * 4f;
            int bars = 8;
            float total = bar * bars;
            int n = Mathf.RoundToInt(Rate * total);
            var data = new float[n];

            float[][] chords =
            {
                new[] { 261.63f, 329.63f, 392.00f },  // C
                new[] { 220.00f, 261.63f, 329.63f },  // Am
                new[] { 174.61f, 220.00f, 261.63f },  // F
                new[] { 196.00f, 246.94f, 293.66f },  // G
            };

            var rng = new System.Random(7);

            for (int b = 0; b < bars; b++)
            {
                var chord = chords[b % chords.Length];
                float barStart = b * bar;

                // soft bass
                AddTone(data, chord[0] * 0.5f, barStart, bar * 0.95f, 0.10f, 0.25f);

                // arpeggio: 4 notes per bar (one per beat)
                for (int e = 0; e < 4; e++)
                {
                    float note = chord[rng.Next(chord.Length)] * (e == 3 ? 2f : 1f);
                    float t = barStart + e * beat;
                    AddTone(data, note, t, beat * 1.6f, 0.085f, 0.02f);
                }

                // occasional sparkle an octave up
                if (b % 2 == 1)
                    AddTone(data, chord[rng.Next(chord.Length)] * 4f, barStart + bar * 0.75f, 0.5f, 0.035f, 0.01f);
            }

            // fade the seam so the loop restarts smoothly
            int fade = Mathf.RoundToInt(Rate * 0.08f);
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                data[i] *= k;
                data[n - 1 - i] *= k;
            }

            return Make("bgm_musicbox", data);
        }

        static void AddTone(float[] buffer, float freq, float start, float dur, float amp, float attack)
        {
            int offset = Mathf.RoundToInt(Rate * start);
            int len = Mathf.RoundToInt(Rate * dur);
            for (int i = 0; i < len && offset + i < buffer.Length; i++)
            {
                float t = i / (float)Rate;
                float env = Envelope(t, dur, attack, dur * 0.9f);
                float v = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.75f
                        + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.15f
                        + Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.06f;
                buffer[offset + i] += v * amp * env;
            }
        }

        static float Envelope(float t, float dur, float attack, float release)
        {
            if (attack > 0f && t < attack) return t / attack;
            float rel = Mathf.Max(0.001f, release);
            float tail = (dur - t) / rel;
            if (tail < 1f) return Mathf.Clamp01(tail) * Mathf.Clamp01(tail);
            return 1f;
        }

        static AudioClip Make(string name, float[] data)
        {
            for (int i = 0; i < data.Length; i++) data[i] = Mathf.Clamp(data[i], -1f, 1f);
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
