using NUnit.Framework;
using UnityEngine;

namespace Tugas7.Tests
{
    public class T7_PuzzleTests
    {
        private GameObject root;

        [SetUp]
        public void SetUp() => root = new GameObject("Root");

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(root);

        [Test]
        public void PressurePlateOnlyAcceptsDesignatedCrate()
        {
            root.AddComponent<BoxCollider>().isTrigger = true;
            var plate = root.AddComponent<T7_PressurePlate>();
            var crate = new GameObject("Crate").AddComponent<Rigidbody>();
            var other = new GameObject("Other").AddComponent<Rigidbody>();
            crate.transform.SetParent(root.transform);
            other.transform.SetParent(root.transform);
            plate.SetDesignatedCrate(crate);
            plate.EvaluateBody(other, true);
            Assert.That(plate.IsPressed, Is.False);
            plate.EvaluateBody(crate, true);
            Assert.That(plate.IsPressed, Is.True);
            plate.EvaluateBody(crate, false);
            Assert.That(plate.IsPressed, Is.False);
        }

        [Test]
        public void GateTargetsOpenOnPressAndClosedOnRelease()
        {
            var gate = root.AddComponent<T7_Gate>();
            gate.Configure(Vector3.zero, new Vector3(0f, 4f, 0f), 4f);
            gate.SetOpen(true);
            Assert.That(gate.IsOpen, Is.True);
            Assert.That(gate.TargetLocalPosition, Is.EqualTo(new Vector3(0f, 4f, 0f)));
            gate.SetOpen(false);
            Assert.That(gate.IsOpen, Is.False);
            Assert.That(gate.TargetLocalPosition, Is.EqualTo(Vector3.zero));
        }
    }
}
