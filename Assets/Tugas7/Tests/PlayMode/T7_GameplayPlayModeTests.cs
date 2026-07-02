using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tugas7.Tests
{
    public class T7_GameplayPlayModeTests
    {
        [UnityTest]
        public IEnumerator DamageVolumeAppliesDamageOverElapsedTime()
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            var health = player.AddComponent<T7_PlayerHealth>();
            var lava = new GameObject("Lava");
            lava.AddComponent<BoxCollider>().isTrigger = true;
            var volume = lava.AddComponent<T7_DamageVolume>();
            volume.DamagePerSecond = 20f;
            volume.ApplyTo(health, 0.5f);
            yield return null;
            Assert.That(health.CurrentHealth, Is.EqualTo(90f).Within(0.01f));
            Object.Destroy(player);
            Object.Destroy(volume.gameObject);
        }

        [UnityTest]
        public IEnumerator ObstacleRespectsPerSourceCooldown()
        {
            var player = new GameObject("Player");
            var health = player.AddComponent<T7_PlayerHealth>();
            var obstacle = new GameObject("Bar").AddComponent<T7_DamageObstacle>();
            obstacle.Configure(15f, 0.75f);
            Assert.That(obstacle.TryHit(health), Is.True);
            Assert.That(obstacle.TryHit(health), Is.False);
            yield return null;
            Assert.That(health.CurrentHealth, Is.EqualTo(85f));
            Object.Destroy(player);
            Object.Destroy(obstacle.gameObject);
        }

        [UnityTest]
        public IEnumerator RespawnRestoresFiftyHealthAndClearsVelocity()
        {
            var player = new GameObject("Player");
            var body = player.AddComponent<Rigidbody>();
            body.useGravity = false;
            var health = player.AddComponent<T7_PlayerHealth>();
            var respawn = player.AddComponent<T7_RespawnController>();
            respawn.Configure(health, body, null, 0f, 50f);
            body.linearVelocity = Vector3.one * 5f;
            health.ApplyDamage(100f, player);
            yield return null;
            Assert.That(health.CurrentHealth, Is.EqualTo(50f));
            Assert.That(body.linearVelocity, Is.EqualTo(Vector3.zero));
            Object.Destroy(player);
        }

        [UnityTest]
        public IEnumerator FinishPresentationTriggersOnlyOnceInPlayMode()
        {
            var root = new GameObject("FinishPresentationTest");
            var manager = root.AddComponent<T7_CourseManager>();
            var leftNpc = new GameObject("LeftNPC").AddComponent<T7_TutorialNPC>();
            var rightNpc = new GameObject("RightNPC").AddComponent<T7_TutorialNPC>();
            leftNpc.transform.SetParent(root.transform);
            rightNpc.transform.SetParent(root.transform);
            var presentation = root.AddComponent<T7_FinishPresentation>();
            presentation.Configure(manager, null, new[] { leftNpc, rightNpc });

            manager.TryActivateCheckpoint(1, root.transform);
            manager.TryActivateCheckpoint(2, root.transform);
            manager.TryActivateCheckpoint(3, root.transform);
            Assert.That(manager.TryFinishCourse(), Is.True);
            Assert.That(manager.TryFinishCourse(), Is.False);
            yield return null;

            Assert.That(leftNpc.IsVictorious, Is.True);
            Assert.That(rightNpc.IsVictorious, Is.True);
            Assert.That(presentation.PlayCount, Is.EqualTo(1));
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator LavaAudioDefersPlaybackUntilInactiveControllerIsEnabled()
        {
            var root = new GameObject("LavaAudioTest");
            root.AddComponent<AudioListener>();
            var first = new GameObject("LavaEmitterA").AddComponent<AudioSource>();
            var second = new GameObject("LavaEmitterB").AddComponent<AudioSource>();
            first.transform.SetParent(root.transform);
            second.transform.SetParent(root.transform);
            var controller = root.AddComponent<T7_ProceduralLavaAudio>();
            AudioClip imported = AudioClip.Create("ImportedLava", 8000, 1, 8000, false);
            root.SetActive(false);

            controller.Configure(imported, new[] { first, second });

            Assert.That(controller.AmbienceClip, Is.SameAs(imported));
            Assert.That(first.clip, Is.SameAs(imported));
            Assert.That(second.clip, Is.SameAs(first.clip));
            Assert.That(controller.PlaybackRequestCount, Is.Zero);

            root.SetActive(true);
            yield return null;

            foreach (AudioSource source in new[] { first, second })
            {
                Assert.That(source.clip, Is.SameAs(imported));
                Assert.That(source.loop, Is.True);
                Assert.That(source.spatialBlend, Is.EqualTo(1f));
                Assert.That(source.playOnAwake, Is.False);
                Assert.That(source.volume, Is.EqualTo(0.12f).Within(0.001f));
                Assert.That(source.minDistance, Is.EqualTo(4f));
                Assert.That(source.maxDistance, Is.EqualTo(22f));
                Assert.That(source.rolloffMode, Is.EqualTo(AudioRolloffMode.Logarithmic));
                Assert.That(source.dopplerLevel, Is.Zero);
            }
            Assert.That(controller.PlaybackRequestCount, Is.EqualTo(2));
            LogAssert.NoUnexpectedReceived();
            Object.Destroy(root);
            yield return null;
            Object.Destroy(imported);
            yield return null;
        }
    }
}
