using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UAS.Tests
{
    public class UAS_ForestAnimalAnimatorTests
    {
        private readonly List<Object> cleanup = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object item in cleanup)
            {
                if (item != null)
                {
                    Object.DestroyImmediate(item);
                }
            }

            cleanup.Clear();
        }

        [Test]
        public void HopMovesAnimalVertically()
        {
            Rig rig = CreateRig(UAS_ForestAnimalAnimator.AnimationStyle.Hop);
            rig.Animator.BeginAnimation();
            rig.Animator.TickAnimation(0.2f);

            Assert.That(rig.Root.localPosition.y, Is.GreaterThan(0f));
        }

        [Test]
        public void FlapRotatesBothWingsInOppositeDirections()
        {
            Rig rig = CreateRig(UAS_ForestAnimalAnimator.AnimationStyle.Flap);
            rig.Animator.BeginAnimation();
            rig.Animator.TickAnimation(0.15f);

            Assert.That(Quaternion.Angle(Quaternion.identity, rig.LeftWing.localRotation), Is.GreaterThan(1f));
            Assert.That(Quaternion.Angle(Quaternion.identity, rig.RightWing.localRotation), Is.GreaterThan(1f));
            Assert.That(Mathf.Sign(rig.LeftWing.localEulerAngles.z - 180f),
                Is.Not.EqualTo(Mathf.Sign(rig.RightWing.localEulerAngles.z - 180f)));
        }

        [Test]
        public void OrbitMovesAnimalAroundCenter()
        {
            Rig rig = CreateRig(UAS_ForestAnimalAnimator.AnimationStyle.Orbit);
            Vector3 initialPosition = rig.Root.position;
            rig.Animator.BeginAnimation();
            rig.Animator.TickAnimation(0.25f);

            Assert.That(Vector3.Distance(initialPosition, rig.Root.position), Is.GreaterThan(0.1f));
            Assert.That(Vector3.Distance(
                new Vector3(rig.Root.position.x, 0f, rig.Root.position.z),
                Vector3.zero), Is.EqualTo(2f).Within(0.01f));
        }

        [TestCase(UAS_ForestAnimalAnimator.AnimationStyle.Hop)]
        [TestCase(UAS_ForestAnimalAnimator.AnimationStyle.Flap)]
        [TestCase(UAS_ForestAnimalAnimator.AnimationStyle.Orbit)]
        public void StopRestoresExactInitialTransforms(UAS_ForestAnimalAnimator.AnimationStyle style)
        {
            Rig rig = CreateRig(style);
            Vector3 initialPosition = rig.Root.localPosition;
            Quaternion initialRotation = rig.Root.localRotation;
            Quaternion initialLeft = rig.LeftWing.localRotation;
            Quaternion initialRight = rig.RightWing.localRotation;
            rig.Animator.BeginAnimation();
            rig.Animator.TickAnimation(0.27f);

            rig.Animator.StopAndRestore();

            Assert.That(rig.Root.localPosition, Is.EqualTo(initialPosition));
            Assert.That(rig.Root.localRotation, Is.EqualTo(initialRotation));
            Assert.That(rig.LeftWing.localRotation, Is.EqualTo(initialLeft));
            Assert.That(rig.RightWing.localRotation, Is.EqualTo(initialRight));
        }

        private Rig CreateRig(UAS_ForestAnimalAnimator.AnimationStyle style)
        {
            GameObject centerObject = Track(new GameObject("Center"));
            GameObject rootObject = Track(new GameObject("Animal"));
            Transform left = Track(new GameObject("LeftWing")).transform;
            Transform right = Track(new GameObject("RightWing")).transform;
            left.SetParent(rootObject.transform, false);
            right.SetParent(rootObject.transform, false);
            UAS_ForestAnimalAnimator animator = rootObject.AddComponent<UAS_ForestAnimalAnimator>();
            animator.Configure(style, rootObject.transform, left, right, centerObject.transform, 2f, 1f, 0f);
            return new Rig(animator, rootObject.transform, left, right);
        }

        private T Track<T>(T item) where T : Object
        {
            cleanup.Add(item);
            return item;
        }

        private readonly struct Rig
        {
            public Rig(
                UAS_ForestAnimalAnimator animator,
                Transform root,
                Transform leftWing,
                Transform rightWing)
            {
                Animator = animator;
                Root = root;
                LeftWing = leftWing;
                RightWing = rightWing;
            }

            public UAS_ForestAnimalAnimator Animator { get; }
            public Transform Root { get; }
            public Transform LeftWing { get; }
            public Transform RightWing { get; }
        }
    }
}
