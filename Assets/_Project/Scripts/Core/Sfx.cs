using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    public enum Sound { Shoot, Hit, Kill, Coin, Buy, Hurt, Death, Boss, Dash, Win }

    /// <summary>Dzwiek generowany w kodzie (bez assetow): bipy 8-bit i petla z fali kwadratowej. M = wyciszenie.</summary>
    public static class Sfx
    {
        private const int Rate = 22050;
        private static readonly Dictionary<Sound, AudioClip> clips = new Dictionary<Sound, AudioClip>();
        private static AudioSource sfxSource, musicSource;
        private static bool muted;

        public static bool Muted => muted;

        public static void Init(Transform parent)
        {
            clips.Clear();
            var go = new GameObject("Audio");
            go.transform.SetParent(parent, false);
            sfxSource = go.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            musicSource = go.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = 0.35f;
            musicSource.clip = null;
            zoneMusic.Clear();
            musicZone = -1;
            SetMusic(1);

            clips[Sound.Shoot] = Sweep("shoot", 900, 300, 0.08f, Wave.Square, 0.25f);
            clips[Sound.Hit] = Sweep("hit", 300, 150, 0.06f, Wave.Noise, 0.3f);
            clips[Sound.Kill] = Sweep("kill", 400, 80, 0.18f, Wave.Noise, 0.4f);
            clips[Sound.Coin] = Join("coin", Sweep("c1", 988, 988, 0.05f, Wave.Square, 0.25f), Sweep("c2", 1319, 1319, 0.12f, Wave.Square, 0.25f));
            clips[Sound.Buy] = Join("buy", Sweep("b1", 523, 523, 0.07f, Wave.Square, 0.3f), Sweep("b2", 659, 659, 0.07f, Wave.Square, 0.3f), Sweep("b3", 784, 784, 0.14f, Wave.Square, 0.3f));
            clips[Sound.Hurt] = Sweep("hurt", 220, 60, 0.25f, Wave.Square, 0.45f);
            clips[Sound.Death] = Sweep("death", 440, 40, 0.8f, Wave.Square, 0.5f);
            clips[Sound.Boss] = Sweep("boss", 90, 50, 0.9f, Wave.Square, 0.5f);
            clips[Sound.Dash] = Sweep("dash", 200, 700, 0.12f, Wave.Noise, 0.25f);
            clips[Sound.Win] = Join("win", Sweep("w1", 523, 523, 0.12f, Wave.Square, 0.3f), Sweep("w2", 659, 659, 0.12f, Wave.Square, 0.3f),
                Sweep("w3", 784, 784, 0.12f, Wave.Square, 0.3f), Sweep("w4", 1047, 1047, 0.4f, Wave.Square, 0.3f));
            Apply();
        }

        public static void Play(Sound s)
        {
            if (sfxSource == null || muted) return;
            if (clips.TryGetValue(s, out var c)) sfxSource.PlayOneShot(c);
        }

        public static void StartMusic()
        {
            if (musicSource != null && !musicSource.isPlaying) musicSource.Play();
        }

        public static void ToggleMute()
        {
            muted = !muted;
            Apply();
        }

        private static void Apply()
        {
            if (musicSource != null) musicSource.mute = muted;
        }

        // ---------- synteza ----------

        private enum Wave { Square, Noise }

        private static AudioClip Sweep(string name, float f0, float f1, float dur, Wave wave, float volume)
        {
            int n = Mathf.Max(1, (int)(Rate * dur));
            var data = new float[n];
            float phase = 0;
            var rng = new System.Random(name.GetHashCode());
            float noise = 0;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float f = Mathf.Lerp(f0, f1, t);
                phase += f / Rate;
                if (phase > 1f) phase -= 1f;
                float v;
                if (wave == Wave.Square) v = phase < 0.5f ? 1f : -1f;
                else
                {
                    if (i % Mathf.Max(1, (int)(Rate / (f * 2f))) == 0) noise = (float)rng.NextDouble() * 2f - 1f;
                    v = noise;
                }
                data[i] = v * volume * (1f - t);
            }
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip Join(string name, params AudioClip[] parts)
        {
            int total = 0;
            foreach (var p in parts) total += p.samples;
            var data = new float[total];
            int offset = 0;
            foreach (var p in parts)
            {
                var d = new float[p.samples];
                p.GetData(d, 0);
                d.CopyTo(data, offset);
                offset += d.Length;
            }
            var clip = AudioClip.Create(name, total, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static readonly Dictionary<int, AudioClip> zoneMusic = new Dictionary<int, AudioClip>();
        private static int musicZone = -1;

        /// <summary>Petla muzyczna per strefa (1-3), generowana leniwie.</summary>
        public static void SetMusic(int zone)
        {
            if (musicSource == null || zone == musicZone) return;
            musicZone = zone;
            if (!zoneMusic.TryGetValue(zone, out var clip)) zoneMusic[zone] = clip = BuildMusic(zone);
            bool wasPlaying = musicSource.isPlaying;
            musicSource.clip = clip;
            if (wasPlaying) musicSource.Play();
        }

        private static AudioClip BuildMusic(int zone)
        {
            float step;
            float[] bass;
            float[][] chords;
            float duty, bassVol, arpVol;
            switch (zone)
            {
                case 2:   // The Bank: d-moll, szybciej, waska fala w arpeggio
                    step = 0.16f; duty = 0.125f; bassVol = 0.33f; arpVol = 0.2f;
                    bass = new[] { 73.4f, 73.4f, 146.8f, 73.4f, 58.3f, 58.3f, 116.5f, 58.3f, 65.4f, 65.4f, 130.8f, 65.4f, 55f, 55f, 110f, 55f };
                    chords = new[]
                    {
                        new[] { 293.7f, 349.2f, 440f }, new[] { 233.1f, 293.7f, 349.2f },
                        new[] { 261.6f, 329.6f, 392f }, new[] { 220f, 277.2f, 329.6f }
                    };
                    break;
                case 3:   // Auction House: napiecie, chromatyka, bardzo szybko
                    step = 0.13f; duty = 0.25f; bassVol = 0.34f; arpVol = 0.2f;
                    bass = new[] { 110f, 116.5f, 110f, 103.8f, 110f, 116.5f, 123.5f, 116.5f, 87.3f, 92.5f, 87.3f, 82.4f, 87.3f, 92.5f, 98f, 92.5f };
                    chords = new[]
                    {
                        new[] { 220f, 261.6f, 311.1f }, new[] { 220f, 277.2f, 329.6f },
                        new[] { 174.6f, 207.7f, 261.6f }, new[] { 185f, 220f, 277.2f }
                    };
                    break;
                default:  // The Toll Road: a-moll
                    step = 0.2f; duty = 0.25f; bassVol = 0.35f; arpVol = 0.18f;
                    bass = new[] { 110, 110, 165, 110, 87.3f, 87.3f, 130.8f, 87.3f, 98, 98, 146.8f, 98, 82.4f, 82.4f, 123.5f, 82.4f };
                    chords = new[]
                    {
                        new[] { 220f, 261.6f, 329.6f }, new[] { 174.6f, 220f, 261.6f },
                        new[] { 196f, 246.9f, 293.7f }, new[] { 164.8f, 207.7f, 246.9f }
                    };
                    break;
            }

            int stepSamples = (int)(Rate * step);
            var data = new float[stepSamples * 16];
            for (int st = 0; st < 16; st++)
            {
                var chord = chords[st / 4];
                float arp = chord[st % 3];
                for (int i = 0; i < stepSamples; i++)
                {
                    float tt = (st * stepSamples + i) / (float)Rate;
                    float env = 1f - i / (float)stepSamples;
                    float b = Mathf.Repeat(tt * bass[st], 1f) < 0.5f ? 1f : -1f;
                    float a = Mathf.Repeat(tt * arp, 1f) < duty ? 1f : -1f;
                    data[st * stepSamples + i] = (b * bassVol + a * arpVol) * (0.4f + 0.6f * env);
                }
            }
            var clip = AudioClip.Create("music" + zone, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
