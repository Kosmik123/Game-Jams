using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZeroHour.Audio
{
    // Sound effects and music for the whole game. Created on first use and kept across scenes;
    // clips come from Resources/Audio/<name>.
    public class AudioManager : MonoBehaviour
    {
        private const string MutedKey = "Muted";
        private const int SfxSourceCount = 6;

        private static AudioManager instance;

        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private readonly List<AudioSource> sfxSources = new List<AudioSource>();
        private AudioSource musicSource;
        private int nextSfxSource;
        private bool isMuted;

        // Every requested sound, also while muted (used to check the sound events).
        public event Action<string> ClipPlayed;

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject(nameof(AudioManager)).AddComponent<AudioManager>();
                    DontDestroyOnLoad(instance.gameObject);
                }
                return instance;
            }
        }

        public bool IsMuted
        {
            get => isMuted;
            set
            {
                isMuted = value;
                PlayerPrefs.SetInt(MutedKey, value ? 1 : 0);
                PlayerPrefs.Save();
                ApplyMute();
            }
        }

        private void Awake()
        {
            for (int i = 0; i < SfxSourceCount; i++)
                sfxSources.Add(CreateSource());
            musicSource = CreateSource();
            musicSource.loop = true;
            isMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
            ApplyMute();
        }

        // Statics survive play sessions when domain reload is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        public void Play(string clipName, float volume = 1f, float pitch = 1f)
        {
            ClipPlayed?.Invoke(clipName);
            AudioClip clip = GetClip(clipName);
            if (clip == null || isMuted)
                return;

            // A free source if there is one, otherwise the oldest sound is cut.
            AudioSource source = sfxSources.Find(s => !s.isPlaying);
            if (source == null)
            {
                source = sfxSources[nextSfxSource];
                nextSfxSource = (nextSfxSource + 1) % sfxSources.Count;
            }
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
        }

        // Starts the loop, or only changes its volume when it is already playing.
        public void PlayMusic(string clipName, float volume)
        {
            AudioClip clip = GetClip(clipName);
            if (clip == null)
                return;
            musicSource.volume = volume;
            if (musicSource.clip == clip && musicSource.isPlaying)
                return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void SetMusicVolume(float volume) => musicSource.volume = volume;

        public void StopMusic() => musicSource.Stop();

        private AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private AudioClip GetClip(string clipName)
        {
            if (!clips.TryGetValue(clipName, out AudioClip clip))
            {
                clip = Resources.Load<AudioClip>($"Audio/{clipName}");
                if (clip == null)
                    Debug.LogWarning($"[AudioManager] Missing clip Resources/Audio/{clipName}");
                clips[clipName] = clip;
            }
            return clip;
        }

        private void ApplyMute()
        {
            musicSource.mute = isMuted;
            foreach (AudioSource source in sfxSources)
                source.mute = isMuted;
        }
    }
}
