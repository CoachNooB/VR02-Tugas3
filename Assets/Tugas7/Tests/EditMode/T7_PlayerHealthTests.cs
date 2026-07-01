using NUnit.Framework;
using UnityEngine;

namespace Tugas7.Tests
{
    public class T7_PlayerHealthTests
    {
        private GameObject player;
        private T7_PlayerHealth health;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Player");
            health = player.AddComponent<T7_PlayerHealth>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(player);

        [Test]
        public void DamageDecreasesHealthAndClampsAtZero()
        {
            health.ApplyDamage(150f, player);
            Assert.That(health.CurrentHealth, Is.Zero);
        }

        [Test]
        public void HealingIncreasesHealthAndClampsAtMaximum()
        {
            health.ApplyDamage(40f, player);
            health.Heal(100f);
            Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth));
        }

        [Test]
        public void DeathFiresOnlyOnce()
        {
            var deaths = 0;
            health.Died += () => deaths++;
            health.ApplyDamage(100f, player);
            health.ApplyDamage(10f, player);
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void RestoreForRespawnSetsExactHealth()
        {
            health.ApplyDamage(100f, player);
            health.RestoreForRespawn(50f);
            Assert.That(health.CurrentHealth, Is.EqualTo(50f));
            Assert.That(health.IsDead, Is.False);
        }
    }
}
