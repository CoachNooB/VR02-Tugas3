using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UAS.Tests
{
    public class UAS_ForestSceneValidationTests
    {
        private const string ScenePath = "Assets/UAS/Scenes/UAS_Harry_Forrest.unity";
        private const string SectionPrefabPath = "Assets/UAS/Prefabs/UAS_ForestTeddySection.prefab";

        private Scene scene;

        [SetUp]
        public void SetUp()
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [Test]
        public void TrackLengthAndDemoAreaMeetMinimumDimensions()
        {
            Transform entry = Find("Track_Entry");
            Transform exit = Find("Track_Exit");
            Assert.That(Vector3.Distance(entry.position, exit.position), Is.GreaterThanOrEqualTo(40f));

            Renderer[] renderers = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Renderer>(true))
                .ToArray();
            Assert.That(renderers.Length, Is.GreaterThan(0));
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers.Skip(1))
            {
                bounds.Encapsulate(renderer.bounds);
            }

            Assert.That(bounds.size.x, Is.GreaterThanOrEqualTo(40f));
            Assert.That(bounds.size.z, Is.GreaterThanOrEqualTo(10f));
        }

        [Test]
        public void SceneHasThreeTriggersAndFourInteractables()
        {
            UAS_RideZoneTrigger[] triggers = FindComponents<UAS_RideZoneTrigger>();
            UAS_InteractableBase[] interactables = FindComponents<UAS_InteractableBase>();

            Assert.That(triggers.Select(item => item.name), Is.EquivalentTo(new[]
            {
                "Boarding_Trigger",
                "Forest_Display_Trigger",
                "Finish_Trigger"
            }));
            Assert.That(interactables.Length, Is.EqualTo(4));
        }

        [Test]
        public void SceneHasThreeDistinctTeddyStyles()
        {
            UAS_TeddyAnimator[] teddies = FindComponents<UAS_TeddyAnimator>();
            Assert.That(teddies.Length, Is.EqualTo(3));
            Assert.That(teddies.Select(item => item.Style).Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void SceneHasFiveAnimatedForestAnimals()
        {
            UAS_ForestAnimalAnimator[] animals = FindComponents<UAS_ForestAnimalAnimator>();
            Assert.That(animals.Length, Is.EqualTo(5));
            Assert.That(animals.Count(item => item.Style == UAS_ForestAnimalAnimator.AnimationStyle.Hop),
                Is.EqualTo(2));
            Assert.That(animals.Count(item => item.Style == UAS_ForestAnimalAnimator.AnimationStyle.Flap),
                Is.EqualTo(2));
            Assert.That(animals.Count(item => item.Style == UAS_ForestAnimalAnimator.AnimationStyle.Orbit),
                Is.EqualTo(1));
            Assert.That(Find("Forest_Animals"), Is.Not.Null);
        }

        [Test]
        public void SceneHasWorldSpaceCanvas()
        {
            Canvas[] canvases = FindComponents<Canvas>();
            Assert.That(canvases.Any(item => item.renderMode == RenderMode.WorldSpace), Is.True);
        }

        [Test]
        public void VehicleHasSixOrderedWaypoints()
        {
            UAS_RideVehicleController vehicle = FindComponents<UAS_RideVehicleController>().Single();
            Assert.That(vehicle.Waypoints.Count, Is.EqualTo(6));
            Assert.That(vehicle.Waypoints.Select(item => item.point.name), Is.EqualTo(new[]
            {
                "WP_00_Start",
                "WP_01_Approach",
                "WP_02_Slow",
                "WP_03_Display_Stop",
                "WP_04_Depart",
                "WP_05_Finish"
            }));
            Assert.That(vehicle.Waypoints.Select(item => item.point.position.x), Is.EqualTo(new[]
            {
                -18f, -6f, -2f, 0f, 7f, 18f
            }).Within(0.001f));
        }

        [Test]
        public void DisplayPrefabExposesIntegrationPointsAndExcludesDemoDependencies()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SectionPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.transform.Find("Integration_Points/Track_Entry"), Is.Not.Null);
            Assert.That(prefab.transform.Find("Integration_Points/Display_Stop"), Is.Not.Null);
            Assert.That(prefab.transform.Find("Integration_Points/Track_Exit"), Is.Not.Null);
            Assert.That(prefab.GetComponentInChildren<UAS_ForestDisplaySequence>(true), Is.Not.Null);
            Assert.That(prefab.GetComponentInChildren<Camera>(true), Is.Null);
            Assert.That(prefab.GetComponentInChildren<UAS_DemoPlayerController>(true), Is.Null);
            Assert.That(prefab.GetComponentInChildren<UAS_RideVehicleController>(true), Is.Null);
        }

        [Test]
        public void SceneHasNoMissingScriptsMaterialsOrErrorShaders()
        {
            List<string> errors = new List<string>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                {
                    int missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject);
                    if (missingScripts > 0)
                    {
                        errors.Add($"{GetPath(item)} has {missingScripts} missing script(s)");
                    }
                }

                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material == null)
                        {
                            errors.Add($"{GetPath(renderer.transform)} has a missing material");
                        }
                        else if (material.shader == null || material.shader.name == "Hidden/InternalErrorShader")
                        {
                            errors.Add($"{material.name} has an invalid shader");
                        }
                    }
                }
            }

            Assert.That(errors, Is.Empty, string.Join("\n", errors));
        }

        private Transform Find(string objectName)
        {
            Transform result = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(item => item.name == objectName);
            Assert.That(result, Is.Not.Null, $"Missing object: {objectName}");
            return result;
        }

        private T[] FindComponents<T>() where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static string GetPath(Transform item)
        {
            string path = item.name;
            while (item.parent != null)
            {
                item = item.parent;
                path = $"{item.name}/{path}";
            }

            return path;
        }
    }
}
