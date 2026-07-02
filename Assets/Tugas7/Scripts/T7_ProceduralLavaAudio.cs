using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Tugas7.EditModeTests")]

namespace Tugas7
{
    public sealed class T7_ProceduralLavaAudio : MonoBehaviour
    {
        private const int MaxCachedClips = 8;
        private static readonly Dictionary<ClipKey, AudioClip> ClipCache = new();
        private static readonly Queue<ClipKey> CacheOrder = new();
        private static readonly List<AudioClip> PendingRetirement = new();

        [SerializeField] private AudioClip ambienceClip;
        [SerializeField] private List<AudioSource> targets = new();
        private bool missingClipWarningLogged;

        public AudioClip AmbienceClip => ambienceClip;
        public int PlaybackRequestCount { get; private set; }
        internal static int CachedClipCount => ClipCache.Count;
        internal static int PendingRetirementCount => PendingRetirement.Count;

        public static AudioClip CreateClip(int sampleRate = 22050, float duration = 4f, int seed = 73421)
        {
            CleanupRetiredClips();
            sampleRate = Mathf.Clamp(sampleRate, 8000, 48000);
            if (float.IsNaN(duration) || float.IsInfinity(duration))
                duration = 4f;
            duration = Mathf.Clamp(duration, 0.1f, 30f);
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var key = new ClipKey(sampleRate, sampleCount, seed);
            if (ClipCache.TryGetValue(key, out AudioClip cached) && cached != null)
                return cached;

            var samples = new float[sampleCount];
            var random = new System.Random(seed);
            float filteredNoise = 0f;
            float crackle = 0f;
            float crackleDecay = Mathf.Exp(-1f / (sampleRate * 0.018f));
            for (int i = 0; i < sampleCount; i++)
            {
                float time = (float)i / sampleRate;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filteredNoise += 0.012f * (noise - filteredNoise);
                if (random.NextDouble() < 7.0 / sampleRate)
                    crackle += (float)(random.NextDouble() * 2.0 - 1.0) * 0.7f;
                crackle *= crackleDecay;
                samples[i] = filteredNoise * 0.5f +
                    Mathf.Sin(2f * Mathf.PI * 37f * time) * 0.18f +
                    Mathf.Sin(2f * Mathf.PI * 61f * time + 1.3f) * 0.1f +
                    crackle;
            }

            SmoothLoop(samples, Mathf.Min(sampleRate / 8, sampleCount / 4));
            float peak = 0f;
            for (int i = 0; i < samples.Length; i++)
                peak = Mathf.Max(peak, Mathf.Abs(samples[i]));
            float scale = peak > 0f ? 0.8f / peak : 1f;
            for (int i = 0; i < samples.Length; i++)
                samples[i] *= scale;

            AudioClip clip = AudioClip.Create(
                $"ProceduralLava_{sampleRate}_{sampleCount}_{seed}",
                sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            if (ClipCache.Count >= MaxCachedClips)
            {
                ClipKey oldest = CacheOrder.Dequeue();
                if (ClipCache.TryGetValue(oldest, out AudioClip oldClip))
                {
                    ClipCache.Remove(oldest);
                    RetireOrDestroy(oldClip);
                }
            }
            ClipCache[key] = clip;
            CacheOrder.Enqueue(key);
            return clip;
        }

        public void Configure(IReadOnlyList<AudioSource> newTargets)
        {
            Configure(CreateClip(), newTargets);
        }

        public void Configure(AudioClip clip, IReadOnlyList<AudioSource> newTargets)
        {
            PlaybackRequestCount = 0;
            ambienceClip = clip;
            missingClipWarningLogged = false;
            targets.Clear();
            if (newTargets != null)
                for (int i = 0; i < newTargets.Count; i++)
                    if (newTargets[i] != null)
                        targets.Add(newTargets[i]);
            ApplyConfiguration(true);
            CleanupRetiredClips();
        }

        private void OnEnable()
        {
            PlaybackRequestCount = 0;
            ApplyConfiguration(Application.isPlaying);
        }

        private void OnDisable() => CleanupRetiredClips();

        private void ApplyConfiguration(bool assignClip)
        {
            AudioClip clip = assignClip ? ambienceClip : null;
            if (assignClip && clip == null && targets.Count > 0 && !missingClipWarningLogged)
            {
                Debug.LogWarning("Lava ambience clip is unavailable.", this);
                missingClipWarningLogged = true;
            }
            for (int i = 0; i < targets.Count; i++)
            {
                AudioSource source = targets[i];
                if (source == null)
                    continue;
                if (assignClip)
                    source.clip = clip;
                source.loop = true;
                source.spatialBlend = 1f;
                source.playOnAwake = false;
                source.volume = 0.12f;
                source.minDistance = 4f;
                source.maxDistance = 22f;
                source.rolloffMode = AudioRolloffMode.Logarithmic;
                source.dopplerLevel = 0f;
                if (Application.isPlaying && source.isActiveAndEnabled &&
                    source.clip != null && !source.isPlaying)
                {
                    PlaybackRequestCount++;
                    source.Play();
                }
            }
        }

        internal static void ResetCacheForTests()
        {
            foreach (AudioClip clip in ClipCache.Values)
                RetireOrDestroy(clip);
            ClipCache.Clear();
            CacheOrder.Clear();
            CleanupRetiredClips();
        }

        private static void RetireOrDestroy(AudioClip clip)
        {
            if (clip == null)
                return;
            if (IsReferencedByAudioSource(clip))
            {
                if (!PendingRetirement.Contains(clip))
                    PendingRetirement.Add(clip);
                return;
            }
            DestroyClip(clip);
        }

        private static void CleanupRetiredClips()
        {
            for (int i = PendingRetirement.Count - 1; i >= 0; i--)
            {
                AudioClip clip = PendingRetirement[i];
                if (clip == null || !IsReferencedByAudioSource(clip))
                {
                    PendingRetirement.RemoveAt(i);
                    DestroyClip(clip);
                }
            }
        }

        private static bool IsReferencedByAudioSource(AudioClip clip)
        {
            AudioSource[] sources = Resources.FindObjectsOfTypeAll<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
                if (sources[i] != null && sources[i].clip == clip)
                    return true;
            return false;
        }

        private static void DestroyClip(AudioClip clip)
        {
            if (clip == null)
                return;
            if (Application.isPlaying)
                Destroy(clip);
            else
                DestroyImmediate(clip);
        }

        private static void SmoothLoop(float[] samples, int crossfadeLength)
        {
            if (crossfadeLength <= 0)
                return;
            crossfadeLength = Mathf.Min(crossfadeLength, (samples.Length - 2) / 2);
            if (crossfadeLength <= 0)
                return;

            int leftAnchor = samples.Length - crossfadeLength - 1;
            int rightAnchor = crossfadeLength;
            float from = samples[leftAnchor];
            float to = samples[rightAnchor];
            int bridgeSampleCount = crossfadeLength * 2;
            for (int step = 1; step <= bridgeSampleCount; step++)
            {
                float t = (float)step / (bridgeSampleCount + 1);
                float smoothT = t * t * (3f - 2f * t);
                float value = Mathf.LerpUnclamped(from, to, smoothT);
                int index = step <= crossfadeLength
                    ? samples.Length - crossfadeLength + step - 1
                    : step - crossfadeLength - 1;
                samples[index] = value;
            }
        }

        private readonly struct ClipKey : IEquatable<ClipKey>
        {
            public readonly int SampleRate;
            public readonly int SampleCount;
            public readonly int Seed;

            public ClipKey(int sampleRate, int sampleCount, int seed)
            {
                SampleRate = sampleRate;
                SampleCount = sampleCount;
                Seed = seed;
            }

            public bool Equals(ClipKey other) =>
                SampleRate == other.SampleRate &&
                SampleCount == other.SampleCount &&
                Seed == other.Seed;

            public override bool Equals(object obj) => obj is ClipKey other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(SampleRate, SampleCount, Seed);
        }
    }
}
