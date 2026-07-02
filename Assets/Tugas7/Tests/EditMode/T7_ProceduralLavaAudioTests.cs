using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tugas7.Tests
{
    public class T7_ProceduralLavaAudioTests
    {
        [SetUp]
        public void SetUp() => T7_ProceduralLavaAudio.ResetCacheForTests();

        [TearDown]
        public void TearDown() => T7_ProceduralLavaAudio.ResetCacheForTests();

        [Test]
        public void ImportedLavaAmbienceAndAttributionArePresent()
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/Tugas7/Audio/T7_LavaAmbience.ogg");
            Assert.That(clip, Is.Not.Null);
            Assert.That(clip.length, Is.InRange(20f, 30f));

            TextAsset attribution = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/Tugas7/ThirdParty/ATTRIBUTION.md");
            Assert.That(attribution, Is.Not.Null);
            string text = attribution.text;
            string[] required =
            {
                "Kilauea Lava Sounds.wav",
                "e__",
                "https://freesound.org/people/e__/sounds/172630/",
                "Heavy Bubbles",
                "casiba842",
                "https://freesound.org/people/casiba842/sounds/577880/",
                "CC0",
                "T7_LavaAmbience.ogg"
            };
            foreach (string value in required)
                StringAssert.Contains(value, text);
        }

        [Test]
        public void CreateClipIsDeterministicBoundedMonoAndLoopable()
        {
            AudioClip first = T7_ProceduralLavaAudio.CreateClip(22050, 4f, 73421);
            AudioClip second = T7_ProceduralLavaAudio.CreateClip(22050, 4f, 73421);
            var firstSamples = new float[first.samples];
            var secondSamples = new float[second.samples];
            Assert.That(first.GetData(firstSamples, 0), Is.True);
            Assert.That(second.GetData(secondSamples, 0), Is.True);

            Assert.That(first.channels, Is.EqualTo(1));
            Assert.That(first.frequency, Is.EqualTo(22050));
            Assert.That(first.samples, Is.EqualTo(88200));
            Assert.That(secondSamples, Is.EqualTo(firstSamples));
            Assert.That(firstSamples.Max(value => Mathf.Abs(value)), Is.GreaterThan(0.05f).And.LessThanOrEqualTo(0.8f));
            Assert.That(firstSamples.All(value => !float.IsNaN(value) && !float.IsInfinity(value) &&
                Mathf.Abs(value) <= 1f), Is.True);
            float boundaryDelta = Mathf.Abs(firstSamples[0] - firstSamples[firstSamples.Length - 1]);
            Assert.That(boundaryDelta, Is.GreaterThan(0.000001f),
                "Loop smoothing must not fake continuity by copying the first sample to the endpoint.");
            const int seamWindow = 128;
            float seamSquareSum = boundaryDelta * boundaryDelta;
            float seamMax = boundaryDelta;
            for (int offset = 1; offset < seamWindow; offset++)
            {
                float tailDelta = Mathf.Abs(firstSamples[firstSamples.Length - offset] -
                    firstSamples[firstSamples.Length - offset - 1]);
                float headDelta = Mathf.Abs(firstSamples[offset] - firstSamples[offset - 1]);
                seamSquareSum += tailDelta * tailDelta + headDelta * headDelta;
                seamMax = Mathf.Max(seamMax, tailDelta, headDelta);
            }
            float seamRms = Mathf.Sqrt(seamSquareSum / (seamWindow * 2f - 1f));
            float internalSquareSum = 0f;
            for (int i = seamWindow + 1; i < firstSamples.Length - seamWindow; i++)
            {
                float delta = firstSamples[i] - firstSamples[i - 1];
                internalSquareSum += delta * delta;
            }
            float internalRms = Mathf.Sqrt(internalSquareSum /
                (firstSamples.Length - seamWindow * 2 - 1f));
            Assert.That(seamMax, Is.LessThan(0.05f));
            Assert.That(seamRms, Is.LessThanOrEqualTo(internalRms * 1.5f + 0.001f));
        }

        [Test]
        public void CreateClipClampsInvalidArgumentsPredictably()
        {
            AudioClip negative = T7_ProceduralLavaAudio.CreateClip(0, -2f, 1);
            AudioClip zero = T7_ProceduralLavaAudio.CreateClip(0, 0f, 2);
            AudioClip notANumber = T7_ProceduralLavaAudio.CreateClip(22050, float.NaN, 3);
            AudioClip positiveInfinity = T7_ProceduralLavaAudio.CreateClip(22050, float.PositiveInfinity, 4);

            Assert.That(negative.frequency, Is.EqualTo(8000));
            Assert.That(negative.samples, Is.EqualTo(800));
            Assert.That(zero.samples, Is.EqualTo(800));
            Assert.That(notANumber.samples, Is.EqualTo(88200));
            Assert.That(positiveInfinity.samples, Is.EqualTo(88200));
        }

        [Test]
        public void CacheDistinguishesDurationsWithDifferentSampleCounts()
        {
            AudioClip shorter = T7_ProceduralLavaAudio.CreateClip(22050, 1.0001f, 9123);
            AudioClip longer = T7_ProceduralLavaAudio.CreateClip(22050, 1.0004f, 9123);

            Assert.That(longer.samples, Is.Not.EqualTo(shorter.samples));
            Assert.That(longer, Is.Not.SameAs(shorter));
        }

        [Test]
        public void CacheSharesExactKeysAndRegeneratesDeterministicallyAfterFifoEviction()
        {
            const int sampleRate = 8001;
            const float duration = 0.2f;
            const int firstSeed = 41000;
            AudioClip first = T7_ProceduralLavaAudio.CreateClip(sampleRate, duration, firstSeed);
            AudioClip shared = T7_ProceduralLavaAudio.CreateClip(sampleRate, duration, firstSeed);
            var originalSamples = new float[first.samples];
            Assert.That(first.GetData(originalSamples, 0), Is.True);
            Assert.That(shared, Is.SameAs(first));

            for (int seed = firstSeed + 1; seed <= firstSeed + 8; seed++)
                T7_ProceduralLavaAudio.CreateClip(sampleRate, duration, seed);

            AudioClip regenerated = T7_ProceduralLavaAudio.CreateClip(sampleRate, duration, firstSeed);
            var regeneratedSamples = new float[regenerated.samples];
            Assert.That(regenerated.GetData(regeneratedSamples, 0), Is.True);
            Assert.That(ReferenceEquals(regenerated, first), Is.False);
            Assert.That(regeneratedSamples, Is.EqualTo(originalSamples));
        }

        [Test]
        public void CacheRetiresEvictedClipOnlyAfterAudioSourceReleasesIt()
        {
            const int sampleRate = 8002;
            const float duration = 0.2f;
            const int firstSeed = 51000;
            var sourceObject = new GameObject("LiveLavaSource");
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            AudioClip first = T7_ProceduralLavaAudio.CreateClip(sampleRate, duration, firstSeed);
            source.clip = first;

            for (int seed = firstSeed + 1; seed <= firstSeed + 8; seed++)
                T7_ProceduralLavaAudio.CreateClip(sampleRate, duration, seed);

            Assert.That(T7_ProceduralLavaAudio.CachedClipCount, Is.EqualTo(8));
            Assert.That(T7_ProceduralLavaAudio.PendingRetirementCount, Is.EqualTo(1));
            Assert.That(source.clip, Is.SameAs(first));
            var samples = new float[first.samples];
            Assert.That(first.GetData(samples, 0), Is.True);

            Object.DestroyImmediate(sourceObject);
            T7_ProceduralLavaAudio.CreateClip(sampleRate, duration, firstSeed + 8);

            Assert.That(T7_ProceduralLavaAudio.PendingRetirementCount, Is.Zero);
        }

        [Test]
        public void ConfigureAppliesSharedSpatialClipToCopiedTargets()
        {
            var root = new GameObject("AudioController");
            var controller = root.AddComponent<T7_ProceduralLavaAudio>();
            var a = new GameObject("A").AddComponent<AudioSource>();
            var b = new GameObject("B").AddComponent<AudioSource>();
            var targets = new[] { a, b };
            controller.Configure(targets);
            targets[0] = null;

            foreach (AudioSource source in new[] { a, b })
            {
                Assert.That(source.clip, Is.Not.Null);
                Assert.That(source.clip, Is.SameAs(a.clip));
                Assert.That(source.loop, Is.True);
                Assert.That(source.playOnAwake, Is.False);
                Assert.That(source.spatialBlend, Is.EqualTo(1f));
                Assert.That(source.volume, Is.EqualTo(0.12f).Within(0.001f));
                Assert.That(source.minDistance, Is.EqualTo(4f));
                Assert.That(source.maxDistance, Is.EqualTo(22f));
                Assert.That(source.rolloffMode, Is.EqualTo(AudioRolloffMode.Logarithmic));
                Assert.That(source.dopplerLevel, Is.Zero);
                Assert.That(source.isPlaying, Is.False);
            }

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(a.gameObject);
            Object.DestroyImmediate(b.gameObject);
        }

        [Test]
        public void BuiltSceneContainsOneConfiguredLavaAudioGroup()
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/T6_T7_MainScene.unity", OpenSceneMode.Additive);
            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                T7_ProceduralLavaAudio[] controllers = roots
                    .SelectMany(root => root.GetComponentsInChildren<T7_ProceduralLavaAudio>(true))
                    .ToArray();
                Transform[] namedGroups = roots
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Where(item => item.name == "AmbientLavaAudio")
                    .ToArray();
                Assert.That(controllers, Has.Length.EqualTo(1));
                Assert.That(namedGroups, Has.Length.EqualTo(1));
                Assert.That(namedGroups[0], Is.SameAs(controllers[0].transform));
                Assert.That(controllers[0].name, Is.EqualTo("AmbientLavaAudio"));
                AudioSource[] sources = controllers[0].GetComponentsInChildren<AudioSource>(true)
                    .OrderBy(source => source.name)
                    .ToArray();
                Assert.That(sources, Has.Length.EqualTo(4));
                Vector3[] expectedPoolCenters =
                {
                    new(0f, 0.2f, 29f),
                    new(0f, 0.2f, 112f),
                    new(0f, 0.2f, 160f),
                    new(0f, 0.2f, 187f)
                };
                Assert.That(sources.Select(source => source.transform.localPosition),
                    Is.EqualTo(expectedPoolCenters));
                Assert.That(sources.Select(source => source.transform.position),
                    Is.EqualTo(expectedPoolCenters));
                Assert.That(sources.All(source => source.clip == null && source.loop &&
                    !source.playOnAwake && source.spatialBlend == 1f &&
                    Mathf.Approximately(source.minDistance, 4f) &&
                    Mathf.Approximately(source.maxDistance, 22f) &&
                    Mathf.Approximately(source.volume, 0.12f) &&
                    source.rolloffMode == AudioRolloffMode.Logarithmic &&
                    Mathf.Approximately(source.dopplerLevel, 0f)), Is.True);
                Assert.That(sources.All(source => source.GetComponent<Collider>() == null &&
                    source.GetComponent<Renderer>() == null), Is.True);
                var serialized = new SerializedObject(controllers[0]);
                Assert.That(serialized.FindProperty("targets").arraySize, Is.EqualTo(4));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
