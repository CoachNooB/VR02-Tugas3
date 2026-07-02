using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tugas7
{
    public sealed class T7_ProceduralLavaAudio : MonoBehaviour
    {
        private const int MaxCachedClips = 8;
        private static readonly Dictionary<ClipKey, AudioClip> ClipCache = new();
        private static readonly Queue<ClipKey> CacheOrder = new();

        [SerializeField] private List<AudioSource> targets = new();

        public static AudioClip CreateClip(int sampleRate = 22050, float duration = 4f, int seed = 73421)
        {
            sampleRate = Mathf.Clamp(sampleRate, 8000, 48000);
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
                    if (oldClip != null)
                    {
                        if (Application.isPlaying) Destroy(oldClip);
                        else DestroyImmediate(oldClip);
                    }
                }
            }
            ClipCache[key] = clip;
            CacheOrder.Enqueue(key);
            return clip;
        }

        public void Configure(IReadOnlyList<AudioSource> newTargets)
        {
            targets.Clear();
            if (newTargets != null)
                for (int i = 0; i < newTargets.Count; i++)
                    if (newTargets[i] != null)
                        targets.Add(newTargets[i]);
            ApplyConfiguration(true);
        }

        private void OnEnable() => ApplyConfiguration(Application.isPlaying);

        private void ApplyConfiguration(bool assignClip)
        {
            AudioClip clip = assignClip ? CreateClip() : null;
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
                    source.Play();
            }
        }

        private static void SmoothLoop(float[] samples, int crossfadeLength)
        {
            if (crossfadeLength <= 0)
                return;
            int start = samples.Length - crossfadeLength;
            for (int i = 0; i < crossfadeLength; i++)
            {
                float t = (float)i / (crossfadeLength - 1);
                float fromEnd = Mathf.Cos(t * Mathf.PI * 0.5f);
                float fromStart = Mathf.Sin(t * Mathf.PI * 0.5f);
                samples[start + i] = samples[start + i] * fromEnd + samples[i] * fromStart;
            }
            samples[samples.Length - 1] = samples[0];
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
