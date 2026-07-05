using NUnit.Framework;
using UnityEngine;

namespace Tugas7.Tests
{
    public class T7_GroundProbeTests
    {
        private GameObject floor;
        private GameObject player;

        [SetUp]
        public void SetUp()
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2f, 1f, 2f);
            player = new GameObject("Player");
            player.transform.position = new Vector3(1.25f, 1.5f, 0f);
            var capsule = player.AddComponent<CapsuleCollider>();
            capsule.radius = 0.45f;
            capsule.height = 2f;
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(floor);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void GroundProbeDetectsSupportNearPlatformEdge()
        {
            Assert.That(Physics.Raycast(player.transform.position, Vector3.down, 1.2f), Is.False);
            Assert.That(T7_GroundProbe.IsGrounded(player.transform,
                player.GetComponent<CapsuleCollider>(), 1.2f, ~0), Is.True);
        }
    }
}
