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
            Assert.That(Mathf.Abs(firstSamples[0] - firstSamples[firstSamples.Length - 1]), Is.LessThan(0.15f));
        }

        [Test]
        public void CreateClipClampsInvalidArgumentsPredictably()
        {
            AudioClip clip = T7_ProceduralLavaAudio.CreateClip(0, -2f, 1);
            Assert.That(clip.frequency, Is.EqualTo(8000));
            Assert.That(clip.samples, Is.EqualTo(800));
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
                GameObject gauntlet = scene.GetRootGameObjects().Single(root => root.name == "T7_GauntletRoot");
                T7_ProceduralLavaAudio[] controllers =
                    gauntlet.GetComponentsInChildren<T7_ProceduralLavaAudio>(true);
                Assert.That(controllers, Has.Length.EqualTo(1));
                Assert.That(controllers[0].name, Is.EqualTo("AmbientLavaAudio"));
                AudioSource[] sources = controllers[0].GetComponentsInChildren<AudioSource>(true);
                Assert.That(sources, Has.Length.EqualTo(4));
                Assert.That(sources.Select(source => source.transform.position.z),
                    Is.EquivalentTo(new[] { 29f, 112f, 160f, 187f }));
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
