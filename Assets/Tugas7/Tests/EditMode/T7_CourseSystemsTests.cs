using NUnit.Framework;
using UnityEngine;

namespace Tugas7.Tests
{
    public class T7_CourseSystemsTests
    {
        private GameObject root;
        private T7_CourseManager manager;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Course");
            manager = root.AddComponent<T7_CourseManager>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(root);

        [Test]
        public void CheckpointsRejectSkippedIndices()
        {
            Assert.That(manager.TryActivateCheckpoint(2, root.transform), Is.False);
            Assert.That(manager.CurrentCheckpointIndex, Is.Zero);
        }

        [Test]
        public void ActivatedCheckpointBecomesRespawnPoint()
        {
            var point = new GameObject("Respawn").transform;
            point.SetParent(root.transform);
            Assert.That(manager.TryActivateCheckpoint(1, point), Is.True);
            Assert.That(manager.CurrentRespawnPoint, Is.SameAs(point));
        }

        [Test]
        public void FinishRequiresAllCheckpointsAndRunningCourse()
        {
            manager.StartCourse();
            Assert.That(manager.TryFinishCourse(), Is.False);
            manager.TryActivateCheckpoint(1, root.transform);
            manager.TryActivateCheckpoint(2, root.transform);
            manager.TryActivateCheckpoint(3, root.transform);
            Assert.That(manager.TryFinishCourse(), Is.True);
        }

        [Test]
        public void ThirdCheckpointUnlocksFinishBeacon()
        {
            var beacon = root.AddComponent<T7_CourseInteractable>();
            beacon.Configure("Finish Beacon", "Finish locked", true);
            manager.SetFinishInteractable(beacon);
            manager.TryActivateCheckpoint(1, root.transform);
            manager.TryActivateCheckpoint(2, root.transform);
            manager.TryActivateCheckpoint(3, root.transform);
            Assert.That(beacon.IsUnlocked, Is.True);
        }

        [Test]
        public void FinishUsesCheckpointStateIfUnlockNotificationWasMissed()
        {
            var beacon = root.AddComponent<T7_CourseInteractable>();
            beacon.Configure("Finish Beacon", "Run complete", true);
            beacon.ConfigureAction(T7_CourseInteractable.CourseAction.FinishCourse, manager);
            manager.StartCourse();
            manager.TryActivateCheckpoint(1, root.transform);
            manager.TryActivateCheckpoint(2, root.transform);
            manager.TryActivateCheckpoint(3, root.transform);

            Assert.That(beacon.IsUnlocked, Is.True);
            Assert.That(beacon.Interact(), Does.StartWith("Run complete"));
            Assert.That(manager.IsComplete, Is.True);
        }

        [Test]
        public void FinishSucceedsAfterCheckpointThreeEvenIfTimerIsNotRunning()
        {
            manager.TryActivateCheckpoint(1, root.transform);
            manager.TryActivateCheckpoint(2, root.transform);
            manager.TryActivateCheckpoint(3, root.transform);

            Assert.That(manager.IsRunning, Is.False);
            Assert.That(manager.TryFinishCourse(), Is.True);
            Assert.That(manager.IsComplete, Is.True);
        }

        [Test]
        public void RepeatedFinishInteractionReturnsRecordedCompletion()
        {
            var beacon = root.AddComponent<T7_CourseInteractable>();
            beacon.Configure("Finish Beacon", "Run complete", true);
            beacon.ConfigureAction(T7_CourseInteractable.CourseAction.FinishCourse, manager);
            manager.TryActivateCheckpoint(1, root.transform);
            manager.TryActivateCheckpoint(2, root.transform);
            manager.TryActivateCheckpoint(3, root.transform);

            string first = beacon.Interact();
            string second = beacon.Interact();

            Assert.That(second, Is.EqualTo(first));
            Assert.That(second, Does.StartWith("Run complete"));
            Assert.That(manager.IsComplete, Is.True);
        }

        [Test]
        public void CourseCompletionIsSignalledExactlyOnce()
        {
            int completedCount = 0;
            manager.CourseCompleted += () => completedCount++;
            manager.TryActivateCheckpoint(1, root.transform);
            manager.TryActivateCheckpoint(2, root.transform);
            manager.TryActivateCheckpoint(3, root.transform);

            Assert.That(manager.TryFinishCourse(), Is.True);
            Assert.That(manager.TryFinishCourse(), Is.False);
            Assert.That(completedCount, Is.EqualTo(1));
        }

        [Test]
        public void HighlightUsesConfiguredIntensityAndRestoresOriginalEmission()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Assert.That(shader, Is.Not.Null);
            var material = new Material(shader);
            var originalEmission = new Color(0.1f, 0.2f, 0.3f, 1f);
            material.SetColor("_EmissionColor", originalEmission);
            var rendererObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rendererObject.transform.SetParent(root.transform);
            var renderer = rendererObject.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var beacon = root.AddComponent<T7_CourseInteractable>();
            beacon.Configure("Finish Beacon", "Run complete", false, renderer);
            var goldGlow = new Color(1f, 0.5f, 0.1f, 1f);
            beacon.ConfigureHighlight(goldGlow, 4f);
            var properties = new MaterialPropertyBlock();

            beacon.SetHighlighted(true);
            renderer.GetPropertyBlock(properties);
            Assert.That(Vector4.Distance(properties.GetColor("_EmissionColor"), goldGlow * 4f),
                Is.LessThan(0.0001f));

            beacon.SetHighlighted(false);
            renderer.GetPropertyBlock(properties);
            Assert.That(properties.HasProperty(Shader.PropertyToID("_EmissionColor")), Is.False);
            Assert.That(renderer.sharedMaterial.GetColor("_EmissionColor"), Is.EqualTo(originalEmission));
            Object.DestroyImmediate(material);
        }

        [Test]
        public void HighlightRestoresPreExistingEmissionOverride()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader);
            var rendererObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rendererObject.transform.SetParent(root.transform);
            var renderer = rendererObject.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var overrideEmission = new Color(0.25f, 0.5f, 0.75f, 1f);
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_EmissionColor", overrideEmission);
            renderer.SetPropertyBlock(properties);
            var beacon = root.AddComponent<T7_CourseInteractable>();
            beacon.Configure("Beacon", "Done", false, renderer);

            beacon.SetHighlighted(true);
            beacon.SetHighlighted(false);

            renderer.GetPropertyBlock(properties);
            Assert.That(Vector4.Distance(properties.GetColor("_EmissionColor"), overrideEmission),
                Is.LessThan(0.0001f));
            Object.DestroyImmediate(material);
        }

        [Test]
        public void HighlightPreservesUnrelatedPropertyBlockValues()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader);
            var rendererObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rendererObject.transform.SetParent(root.transform);
            var renderer = rendererObject.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var properties = new MaterialPropertyBlock();
            properties.SetFloat("_Smoothness", 0.37f);
            renderer.SetPropertyBlock(properties);
            var beacon = root.AddComponent<T7_CourseInteractable>();
            beacon.Configure("Beacon", "Done", false, renderer);

            beacon.SetHighlighted(true);
            renderer.GetPropertyBlock(properties);
            Assert.That(properties.GetFloat("_Smoothness"), Is.EqualTo(0.37f));
            beacon.SetHighlighted(false);
            renderer.GetPropertyBlock(properties);
            Assert.That(properties.GetFloat("_Smoothness"), Is.EqualTo(0.37f));
            Object.DestroyImmediate(material);
        }

        [Test]
        public void DisablingHighlightedBeaconRestoresPropertyBlock()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader);
            var rendererObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rendererObject.transform.SetParent(root.transform);
            var renderer = rendererObject.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var originalEmission = new Color(0.2f, 0.3f, 0.4f, 1f);
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_EmissionColor", originalEmission);
            renderer.SetPropertyBlock(properties);
            var beacon = root.AddComponent<T7_CourseInteractable>();
            beacon.Configure("Beacon", "Done", false, renderer);

            beacon.SetHighlighted(true);
            InvokePrivate(beacon, "OnDisable");

            renderer.GetPropertyBlock(properties);
            Assert.That(Vector4.Distance(properties.GetColor("_EmissionColor"), originalEmission),
                Is.LessThan(0.0001f));
            Object.DestroyImmediate(material);
        }

        [Test]
        public void DisablingInteractorClearsHighlightAndAllowsReacquisition()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader);
            var cameraObject = new GameObject("Camera");
            cameraObject.transform.SetParent(root.transform);
            var camera = cameraObject.AddComponent<Camera>();
            var rendererObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rendererObject.transform.SetParent(root.transform);
            rendererObject.transform.position = Vector3.forward * 2f;
            var renderer = rendererObject.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            var beacon = rendererObject.AddComponent<T7_CourseInteractable>();
            beacon.Configure("Beacon", "Done", false, renderer);
            beacon.ConfigureHighlight(Color.yellow, 4f);
            var interactor = root.AddComponent<T7_RaycastInteractor>();
            interactor.Configure(camera, null);
            var properties = new MaterialPropertyBlock();
            Physics.SyncTransforms();

            InvokePrivate(interactor, "Update");
            InvokePrivate(interactor, "OnDisable");
            renderer.GetPropertyBlock(properties);
            Assert.That(properties.HasProperty(Shader.PropertyToID("_EmissionColor")), Is.False);

            InvokePrivate(interactor, "Update");
            renderer.GetPropertyBlock(properties);
            Assert.That(properties.HasProperty(Shader.PropertyToID("_EmissionColor")), Is.True);
            Object.DestroyImmediate(material);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            target.GetType().GetMethod(methodName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(target, null);
        }
    }
}
