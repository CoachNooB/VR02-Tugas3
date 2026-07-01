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
    }
}
