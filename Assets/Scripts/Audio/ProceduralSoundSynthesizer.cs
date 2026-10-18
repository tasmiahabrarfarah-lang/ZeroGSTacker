using System.Collections.Generic;
using UnityEngine;

namespace ZeroGStacker.Audio
{
    public class ProceduralSoundSynthesizer : MonoBehaviour
    {
        private static ProceduralSoundSynthesizer instance;
        public static ProceduralSoundSynthesizer Instance => instance;

        [SerializeField] private AudioSource sfxSource;

        private const int SampleRate = 44100;

        // Pentatonic frequencies in Hz (A4 major pentatonic: A, B, C#, E, F#)
        private readonly float[] pentatonicScale = new float[]
        {
            440.0f,  // A4
            493.88f, // B4
            554.37f, // C#5
            659.25f, // E5
            739.99f, // F#5
            880.0f,  // A5
            987.77f, // B5
            1108.73f,// C#6
            1318.51f,// E6
            1479.98f // F#6
        };

        private AudioClip pulseClip;
        private AudioClip normalPlaceClip;
        private AudioClip stabilizerClip;
        private AudioClip collapseClip;
        private AudioClip buttonClickClip;
        private Dictionary<int, AudioClip> chimeClips = new Dictionary<int, AudioClip>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f; // 2D sound
            }

            GenerateAllClips();
        }

        private void GenerateAllClips()
        {
            pulseClip = SynthesizePulseSound(0.2f, 220f, 660f);
            normalPlaceClip = SynthesizePluckSound(0.25f, 330f);
            stabilizerClip = SynthesizeStabilizerWave(0.5f);
            collapseClip = SynthesizeNoiseRumble(0.6f);
            buttonClickClip = SynthesizeClickSound(0.05f);

            for (int i = 0; i < 10; i++)
            {
                chimeClips[i] = SynthesizeChimeSound(0.4f, pentatonicScale[i]);
            }
        }

        public void PlayPulseFire()
        {
            if (pulseClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(pulseClip, 0.6f);
            }
        }

        public void PlayNormalPlacement()
        {
            if (normalPlaceClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(normalPlaceClip, 0.75f);
            }
        }

        public void PlayPerfectPlacement(int comboCount)
        {
            if (sfxSource == null) return;
            int idx = Mathf.Clamp(comboCount - 1, 0, pentatonicScale.Length - 1);
            if (chimeClips.TryGetValue(idx, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip, 0.9f);
            }
        }

        public void PlayStabilizer()
        {
            if (stabilizerClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(stabilizerClip, 0.9f);
            }
        }

        public void PlayCollapse()
        {
            if (collapseClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(collapseClip, 1.0f);
            }
        }

        public void PlayButtonClick()
        {
            if (buttonClickClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(buttonClickClip, 0.5f);
            }
        }

        #region Procedural Audio Generators

        private AudioClip SynthesizeChimeSound(float duration, float frequency)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-t * 8.0f); // Fast decay bell envelope

                // Fundamental + bell overtone harmonic
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.7f
                           + Mathf.Sin(2f * Mathf.PI * (frequency * 2.76f) * t) * 0.25f
                           + Mathf.Sin(2f * Mathf.PI * (frequency * 5.4f) * t) * 0.05f;

                data[i] = wave * envelope;
            }

            var clip = AudioClip.Create($"Chime_{frequency}Hz", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip SynthesizePulseSound(float duration, float startFreq, float endFreq)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / duration;
                float curFreq = Mathf.Lerp(startFreq, endFreq, t * t);
                float envelope = Mathf.Sin(t * Mathf.PI);

                float wave = Mathf.Sin(2f * Mathf.PI * curFreq * ((float)i / SampleRate));
                data[i] = wave * envelope * 0.5f;
            }

            var clip = AudioClip.Create("PulseFire", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip SynthesizePluckSound(float duration, float frequency)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-t * 12.0f);
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
                data[i] = wave * envelope * 0.6f;
            }

            var clip = AudioClip.Create("PluckImpact", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip SynthesizeStabilizerWave(float duration)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            // Harmonic chord sweep
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Sin(((float)i / samples) * Mathf.PI);

                float wave = Mathf.Sin(2f * Mathf.PI * 528f * t) * 0.4f
                           + Mathf.Sin(2f * Mathf.PI * 660f * t) * 0.3f
                           + Mathf.Sin(2f * Mathf.PI * 792f * t) * 0.3f;

                data[i] = wave * envelope * 0.7f;
            }

            var clip = AudioClip.Create("StabilizerWave", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip SynthesizeNoiseRumble(float duration)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            float lastVal = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = (1f - t);

                // Low-pass filtered noise for deep rumble
                float white = (Random.value * 2f - 1f);
                lastVal = (lastVal + (0.05f * white)) / 1.05f;

                data[i] = lastVal * envelope * 2.5f;
            }

            var clip = AudioClip.Create("CollapseRumble", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip SynthesizeClickSound(float duration)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / duration;
                float envelope = 1f - t;
                data[i] = Mathf.Sin(2f * Mathf.PI * 1200f * ((float)i / SampleRate)) * envelope * 0.3f;
            }

            var clip = AudioClip.Create("ClickSound", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        #endregion
    }
}
