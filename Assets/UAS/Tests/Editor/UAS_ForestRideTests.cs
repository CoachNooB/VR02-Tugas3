using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace UAS.Tests
{
    public class UAS_ForestRideTests
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
        public void CartRemainsIdleBeforeStartRide()
        {
            UAS_RideVehicleController vehicle = CreateVehicle(
                Waypoint("Start", Vector3.zero, 0f),
                Waypoint("Approach", new Vector3(4f, 0f, 0f), 4f));

            vehicle.Tick(1f);

            Assert.That(vehicle.State, Is.EqualTo(UAS_RideVehicleController.RideState.Idle));
            Assert.That(vehicle.transform.position, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void CartMovesWithoutOvershootingWaypoint()
        {
            UAS_RideVehicleController vehicle = CreateVehicle(
                Waypoint("Start", Vector3.zero, 0f),
                Waypoint("Approach", new Vector3(2f, 0f, 0f), 4f),
                Waypoint("Finish", new Vector3(10f, 0f, 0f), 1f, 0f, true));

            Assert.That(vehicle.StartRide(), Is.True);
            vehicle.Tick(1f);

            Assert.That(vehicle.transform.position.x, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(vehicle.transform.position.z, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void StopWaypointEntersStopping()
        {
            UAS_RideVehicleController vehicle = CreateVehicle(
                Waypoint("Start", Vector3.zero, 0f),
                Waypoint("Display Stop", new Vector3(1f, 0f, 0f), 4f, 2f),
                Waypoint("Finish", new Vector3(2f, 0f, 0f), 4f, 0f, true));

            vehicle.StartRide();
            vehicle.Tick(1f);

            Assert.That(vehicle.State, Is.EqualTo(UAS_RideVehicleController.RideState.Stopping));
            Assert.That(vehicle.IsDisplayStopWindow, Is.True);
        }

        [Test]
        public void StopTimerResumesMovement()
        {
            UAS_RideVehicleController vehicle = CreateVehicle(
                Waypoint("Start", Vector3.zero, 0f),
                Waypoint("Display Stop", new Vector3(1f, 0f, 0f), 4f, 2f),
                Waypoint("Finish", new Vector3(4f, 0f, 0f), 2f, 0f, true));
            vehicle.StartRide();
            vehicle.Tick(1f);

            vehicle.Tick(2.1f);

            Assert.That(vehicle.State, Is.EqualTo(UAS_RideVehicleController.RideState.Moving));
            Assert.That(vehicle.transform.position.x, Is.GreaterThan(1f));
        }

        [Test]
        public void FinalWaypointEntersComplete()
        {
            UAS_RideVehicleController vehicle = CreateVehicle(
                Waypoint("Start", Vector3.zero, 0f),
                Waypoint("Finish", new Vector3(1f, 0f, 0f), 4f, 0f, true));

            vehicle.StartRide();
            vehicle.Tick(1f);

            Assert.That(vehicle.State, Is.EqualTo(UAS_RideVehicleController.RideState.Complete));
        }

        [Test]
        public void RideCannotStartTwice()
        {
            UAS_RideVehicleController vehicle = CreateVehicle(
                Waypoint("Start", Vector3.zero, 0f),
                Waypoint("Finish", Vector3.right, 1f, 0f, true));

            Assert.That(vehicle.StartRide(), Is.True);
            Assert.That(vehicle.StartRide(), Is.False);
        }

        [Test]
        public void DisplayTriggerIsIdempotent()
        {
            GameObject sequenceObject = Track(new GameObject("Sequence"));
            UAS_ForestDisplaySequence sequence = sequenceObject.AddComponent<UAS_ForestDisplaySequence>();
            GameObject triggerObject = Track(new GameObject("DisplayTrigger"));
            UAS_RideZoneTrigger trigger = triggerObject.AddComponent<UAS_RideZoneTrigger>();
            trigger.Configure(UAS_RideZoneTrigger.ZoneMode.Display, null, sequence, null);
            GameObject cart = Track(new GameObject("Cart"));
            cart.AddComponent<UAS_RideVehicleController>();

            Assert.That(trigger.TryHandle(cart), Is.True);
            Assert.That(trigger.TryHandle(cart), Is.False);
            Assert.That(sequence.SequenceStartCount, Is.EqualTo(1));
        }

        [TestCase(UAS_TeddyAnimator.AnimationStyle.Wave)]
        [TestCase(UAS_TeddyAnimator.AnimationStyle.Clap)]
        [TestCase(UAS_TeddyAnimator.AnimationStyle.Bounce)]
        public void TeddyStylesAlterTheirConfiguredTransforms(UAS_TeddyAnimator.AnimationStyle style)
        {
            TeddyRig rig = CreateTeddy(style);
            rig.Animator.BeginAnimation();
            rig.Animator.TickAnimation(0.25f);

            switch (style)
            {
                case UAS_TeddyAnimator.AnimationStyle.Wave:
                    Assert.That(Quaternion.Angle(Quaternion.identity, rig.Left.localRotation), Is.GreaterThan(1f));
                    Assert.That(Quaternion.Angle(Quaternion.identity, rig.Head.localRotation), Is.GreaterThan(1f));
                    break;
                case UAS_TeddyAnimator.AnimationStyle.Clap:
                    Assert.That(Quaternion.Angle(Quaternion.identity, rig.Left.localRotation), Is.GreaterThan(1f));
                    Assert.That(Quaternion.Angle(Quaternion.identity, rig.Right.localRotation), Is.GreaterThan(1f));
                    break;
                case UAS_TeddyAnimator.AnimationStyle.Bounce:
                    Assert.That(rig.Root.localPosition.y, Is.GreaterThan(0f));
                    Assert.That(Quaternion.Angle(Quaternion.identity, rig.Head.localRotation), Is.GreaterThan(1f));
                    break;
            }
        }

        [Test]
        public void TeddyTransformsReturnToInitialValues()
        {
            TeddyRig rig = CreateTeddy(UAS_TeddyAnimator.AnimationStyle.Wave);
            Vector3 rootPosition = rig.Root.localPosition;
            Quaternion headRotation = rig.Head.localRotation;
            Quaternion leftRotation = rig.Left.localRotation;
            Quaternion rightRotation = rig.Right.localRotation;
            rig.Animator.BeginAnimation();
            rig.Animator.TickAnimation(0.37f);

            rig.Animator.StopAndRestore();

            Assert.That(rig.Root.localPosition, Is.EqualTo(rootPosition));
            Assert.That(rig.Head.localRotation, Is.EqualTo(headRotation));
            Assert.That(rig.Left.localRotation, Is.EqualTo(leftRotation));
            Assert.That(rig.Right.localRotation, Is.EqualTo(rightRotation));
        }

        [Test]
        public void StartInteractionRejectsUnboardedPlayer()
        {
            GameObject playerObject = Track(new GameObject("Player"));
            UAS_DemoPlayerController player = playerObject.AddComponent<UAS_DemoPlayerController>();
            GameObject vehicleObject = Track(new GameObject("Vehicle"));
            UAS_RideVehicleController vehicle = vehicleObject.AddComponent<UAS_RideVehicleController>();
            GameObject gateObject = Track(new GameObject("Gate"));
            UAS_GateLeverInteractable gate = gateObject.AddComponent<UAS_GateLeverInteractable>();
            gate.SetOpenForTests(true);
            GameObject startObject = Track(new GameObject("Start"));
            UAS_StartRideInteractable start = startObject.AddComponent<UAS_StartRideInteractable>();
            start.Configure(vehicle, gate);

            Assert.That(start.CanInteract(player), Is.False);
        }

        [Test]
        public void MusicBoxRejectsNonStoppingState()
        {
            GameObject playerObject = Track(new GameObject("Player"));
            UAS_DemoPlayerController player = playerObject.AddComponent<UAS_DemoPlayerController>();
            GameObject vehicleObject = Track(new GameObject("Vehicle"));
            UAS_RideVehicleController vehicle = vehicleObject.AddComponent<UAS_RideVehicleController>();
            GameObject sequenceObject = Track(new GameObject("Sequence"));
            UAS_ForestDisplaySequence sequence = sequenceObject.AddComponent<UAS_ForestDisplaySequence>();
            GameObject effectObject = Track(new GameObject("Effect"));
            UAS_ForestEffectInteractable effect = effectObject.AddComponent<UAS_ForestEffectInteractable>();
            effect.Configure(sequence, vehicle);

            Assert.That(effect.CanInteract(player), Is.False);
        }

        [Test]
        public void PlayerCannotDisembarkWhileVehicleIsMoving()
        {
            UAS_RideVehicleController vehicle = CreateVehicle(
                Waypoint("Start", Vector3.zero, 0f),
                Waypoint("Finish", new Vector3(10f, 0f, 0f), 1f, 0f, true));
            GameObject seatObject = Track(new GameObject("Seat"));
            seatObject.transform.SetParent(vehicle.transform, false);
            GameObject exitObject = Track(new GameObject("Exit"));
            GameObject playerObject = Track(new GameObject("Player"));
            UAS_DemoPlayerController player = playerObject.AddComponent<UAS_DemoPlayerController>();
            CharacterController controller = playerObject.GetComponent<CharacterController>();
            Assert.That(player.EnterRide(seatObject.transform), Is.True);
            Assert.That(controller.enabled, Is.False);
            vehicle.SetPassenger(player);
            vehicle.StartRide();

            Assert.That(player.ExitRide(exitObject.transform), Is.False);
            Assert.That(player.IsRiding, Is.True);
            Assert.That(controller.enabled, Is.False);
        }

        private UAS_RideVehicleController CreateVehicle(params UAS_RideVehicleController.WaypointSetting[] waypoints)
        {
            GameObject vehicleObject = Track(new GameObject("Vehicle"));
            UAS_RideVehicleController vehicle = vehicleObject.AddComponent<UAS_RideVehicleController>();
            vehicle.ConfigureWaypoints(waypoints);
            return vehicle;
        }

        private UAS_RideVehicleController.WaypointSetting Waypoint(
            string name,
            Vector3 position,
            float speed,
            float stopDuration = 0f,
            bool permanentStop = false)
        {
            GameObject point = Track(new GameObject(name));
            point.transform.position = position;
            return new UAS_RideVehicleController.WaypointSetting(point.transform, speed, stopDuration, permanentStop);
        }

        private TeddyRig CreateTeddy(UAS_TeddyAnimator.AnimationStyle style)
        {
            GameObject rootObject = Track(new GameObject("Teddy"));
            Transform head = Track(new GameObject("Head")).transform;
            Transform left = Track(new GameObject("LeftArm")).transform;
            Transform right = Track(new GameObject("RightArm")).transform;
            head.SetParent(rootObject.transform, false);
            left.SetParent(rootObject.transform, false);
            right.SetParent(rootObject.transform, false);
            UAS_TeddyAnimator animator = rootObject.AddComponent<UAS_TeddyAnimator>();
            animator.Configure(style, rootObject.transform, head, left, right);
            return new TeddyRig(animator, rootObject.transform, head, left, right);
        }

        private T Track<T>(T item) where T : Object
        {
            cleanup.Add(item);
            return item;
        }

        private readonly struct TeddyRig
        {
            public TeddyRig(UAS_TeddyAnimator animator, Transform root, Transform head, Transform left, Transform right)
            {
                Animator = animator;
                Root = root;
                Head = head;
                Left = left;
                Right = right;
            }

            public UAS_TeddyAnimator Animator { get; }
            public Transform Root { get; }
            public Transform Head { get; }
            public Transform Left { get; }
            public Transform Right { get; }
        }
    }
}
