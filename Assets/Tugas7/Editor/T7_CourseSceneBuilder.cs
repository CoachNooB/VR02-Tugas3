using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Tugas7;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Tugas7.Editor
{
    public static class T7_CourseSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/T6_T7_MainScene.unity";
        private const string MaterialFolder = "Assets/Tugas7/Materials";
        private static readonly Dictionary<string, Material> Mats = new();
        private static Transform root;

        [MenuItem("Tools/Tugas 7/Rebuild Linear Lava Gauntlet")]
        public static void Rebuild()
        {
            EnsureFolder("Assets/Tugas7", "Materials");
            T7_UpgradeAssetBuilder.PrepareAll();
            Scene scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var existing = GameObject.Find("T7_GauntletRoot");
            if (existing != null) Object.DestroyImmediate(existing);
            CleanupBlankSceneDefaults(scene);
            CreateMaterials();
            root = new GameObject("T7_GauntletRoot").transform;

            Transform systems = Group("Systems", root);
            Transform playerGroup = Group("Player", root);
            Transform start = Group("StartArea", root);
            Transform section1 = Group("Section1_LavaTiming", root);
            Transform checkpoint1 = Group("Checkpoint1", root);
            Transform section2 = Group("Section2_PushPuzzle", root);
            Transform checkpoint2 = Group("Checkpoint2", root);
            Transform section3 = Group("Section3_MovingPlatforms", root);
            Transform checkpoint3 = Group("Checkpoint3", root);
            Transform section4 = Group("Section4_FinalChallenge", root);
            Transform finish = Group("FinishArea", root);
            Transform environment = Group("Environment", root);
            Transform lighting = Group("Lighting", root);

            var manager = systems.gameObject.AddComponent<T7_CourseManager>();
            CreateLighting(lighting);
            CreateEnvironment(environment);
            CreateReset(environment, new Vector3(0, -3f, 95f), new Vector3(30f, 2f, 210f));
            var player = CreatePlayer(playerGroup, manager, out Camera camera, out T7_PlayerHealth health,
                out T7_SpatialFeedbackUI hud, out Rigidbody playerBody, out Behaviour movement);

            Transform initialRespawn = Marker("InitialRespawn", start, new Vector3(0, 1.1f, 1));
            manager.Configure(hud, initialRespawn);
            CreateFloor("StartFloor", start, new Vector3(0, 0, 5), new Vector3(12, 1, 12));
            CreateSign(start, "Tutorial", new Vector3(-5.5f, 2.2f, 4),
                "LINEAR LAVA GAUNTLET\nWASD Move • Mouse Look • Space Jump\nE Interact • Left Click Push Crate");
            var startGate = CreateGate(start, "StartGate", new Vector3(0, 2, 11), new Vector3(0, 7, 11));
            var terminal = CreateInteractable(start, "StartTerminal", new Vector3(0, 1.2f, 7.5f),
                new Vector3(1.2f, 2.2f, 0.7f), Mats["Cyan"], "Start Terminal", "Gate opened — GO!", false);
            terminal.ConfigureAction(T7_CourseInteractable.CourseAction.StartCourseAndOpenGate, manager, startGate);
            BuildTutorialNPC(start, "StartGuide", new Vector3(-3.25f, 0.5f, 5.4f),
                player.transform, camera, T7_TutorialNPC.TutorialLines);

            BuildSection1(section1);
            BuildSectionGuide(section1, "Section1Guide", new Vector3(5.35f, 0.5f, 15f),
                player.transform, camera,
                "Time your jumps between platforms and avoid the rotating machinery.");
            CreateCheckpoint(checkpoint1, 1, 50f, manager);
            Rigidbody crate = BuildPuzzle(section2, manager);
            BuildSectionGuide(section2, "Section2Guide", new Vector3(5.2f, 0.5f, 58f),
                player.transform, camera,
                "Push the yellow crate onto the pressure plate to open the gate.");
            CreateCheckpoint(checkpoint2, 2, 92f, manager);
            BuildSection3(section3);
            BuildSectionGuide(section3, "Section3Guide", new Vector3(-5.25f, 0.5f, 97f),
                player.transform, camera,
                "Ride the moving platforms and keep clear of the sweeper.");
            CreateCheckpoint(checkpoint3, 3, 137f, manager);
            BuildFinal(section4);
            BuildSectionGuide(section4, "Section4Guide", new Vector3(5.25f, 0.5f, 143f),
                player.transform, camera,
                "Combine precise jumps and timing to reach the finish.");
            var beacon = BuildFinish(finish, manager);
            manager.SetFinishInteractable(beacon);
            BuildVisualDressing(environment, lighting);

            player.GetComponent<T7_RaycastInteractor>().Configure(camera, hud, 6f);
            player.GetComponent<T7_CratePusher>().Configure(camera, crate);
            player.GetComponent<T7_RespawnController>().Configure(health, playerBody, manager, 1f, 50f, movement);
            hud.Bind(health);

            AddSceneToBuildSettings();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"T7 gauntlet rebuilt and saved: {ScenePath}");
        }

        public static void RebuildBatch() => Rebuild();

        private static GameObject CreatePlayer(Transform parent, T7_CourseManager manager,
            out Camera camera, out T7_PlayerHealth health, out T7_SpatialFeedbackUI hud,
            out Rigidbody body, out Behaviour movement)
        {
            var player = parent.gameObject;
            player.tag = "Player";
            player.transform.position = new Vector3(0, 1.1f, 1);
            var capsule = player.AddComponent<CapsuleCollider>();
            capsule.height = 2f; capsule.radius = 0.45f;
            body = player.AddComponent<Rigidbody>();
            body.mass = 1f; body.freezeRotation = true; body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            health = player.AddComponent<T7_PlayerHealth>();
            player.AddComponent<T7_RaycastInteractor>();
            player.AddComponent<T7_CratePusher>();
            player.AddComponent<T7_NPCHeadHitInteractor>();
            player.AddComponent<T7_RespawnController>();

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraGo.transform.SetParent(player.transform);
            cameraGo.transform.localPosition = new Vector3(0, 0.65f, 0);
            camera = cameraGo.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            cameraGo.AddComponent<AudioListener>();

            movement = AddT6Controller(player, camera.transform, body);
            player.GetComponent<T7_NPCHeadHitInteractor>().Configure(camera, 3f);
            hud = CreateHud(camera.transform, health);
            return player;
        }

        private static Behaviour AddT6Controller(GameObject player, Transform camera, Rigidbody body)
        {
            Type type = Type.GetType("T6_FirstPersonController, Assembly-CSharp");
            if (type == null)
            {
                Debug.LogWarning("T6_FirstPersonController not found; player movement component was not added.");
                return null;
            }
            var component = player.AddComponent(type) as Behaviour;
            var serialized = new SerializedObject(component);
            serialized.FindProperty("kameraPlayer").objectReferenceValue = camera;
            serialized.FindProperty("rb").objectReferenceValue = body;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return component;
        }

        private static T7_SpatialFeedbackUI CreateHud(Transform camera, T7_PlayerHealth health)
        {
            var go = new GameObject("SpatialHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(T7_SpatialFeedbackUI));
            go.transform.SetParent(camera.parent);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(1600, 900);
            rect.localScale = Vector3.one * 0.00145f;
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 20;

            Image feedback = CreateImage("FullscreenFeedback", rect, Color.clear,
                Vector2.zero, Vector2.one);

            RectTransform healthPanel = CreateRect("HealthHUD_TopLeft", rect,
                new Vector2(0.025f, 0.84f), new Vector2(0.36f, 0.975f));
            Image healthBack = CreateImage("HealthBackground", healthPanel, Color.white,
                new Vector2(0.13f, 0.12f), new Vector2(0.98f, 0.62f),
                LoadSprite("Assets/Images/Background.png"));
            Image healthFill = CreateImage("HealthFill", healthBack.rectTransform, Color.white,
                Vector2.zero, Vector2.one);
            healthFill.sprite = LoadSprite("Assets/Images/Fill.png");
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            CreateImage("Heart", healthPanel, Color.white, new Vector2(0f, 0.08f), new Vector2(0.13f, 0.72f),
                LoadSprite("Assets/Images/Heart.png"));
            TMP_Text hp = CreateText("HPText", healthPanel, "HP 100/100", 30,
                new Vector2(0.13f, 0.62f), new Vector2(0.98f, 1f));
            hp.alignment = TextAlignmentOptions.Left;

            Image timerBack = CreateImage("TimerHUD_TopRight", rect, new Color(0.015f, 0.02f, 0.035f, 0.88f),
                new Vector2(0.72f, 0.885f), new Vector2(0.975f, 0.97f));
            TMP_Text timer = CreateText("TimerText", timerBack.rectTransform, "READY  00:00.0", 31,
                new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));

            Image checkpointBack = CreateImage("CheckpointHUD_TopCenter", rect,
                new Color(0.015f, 0.02f, 0.035f, 0.82f),
                new Vector2(0.39f, 0.91f), new Vector2(0.61f, 0.97f));
            TMP_Text cp = CreateText("CheckpointText", checkpointBack.rectTransform, "CHECKPOINT 0/3", 25,
                new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.95f));

            Image centerPanel = CreateImage("CenterMessageHUD", rect,
                new Color(0.015f, 0.02f, 0.035f, 0.86f),
                new Vector2(0.28f, 0.035f), new Vector2(0.72f, 0.17f));
            TMP_Text status = CreateText("StatusText", centerPanel.rectTransform,
                "Activate the cyan Start Terminal", 25,
                new Vector2(0.04f, 0.46f), new Vector2(0.96f, 0.96f));
            TMP_Text prompt = CreateText("InteractionPrompt", centerPanel.rectTransform, "", 27,
                new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.48f));
            var hud = go.GetComponent<T7_SpatialFeedbackUI>();
            hud.Configure(hp, healthFill, cp, prompt, status, timer, feedback, camera);
            hud.ConfigurePlacement(1.2f, Vector2.zero);
            return hud;
        }

        private static void BuildSection1(Transform parent)
        {
            CreateLava(parent, new Vector3(0, -1, 29), new Vector3(14, 1, 34));
            float[] z = { 15, 21, 27, 33, 39, 45 };
            foreach (float value in z) CreateFloor($"SafePlatform_{value}", parent, new Vector3(0, 0, value), new Vector3(6, 0.8f, 4));
            CreateSweeper(parent, "Sweeper_A", new Vector3(0, 1.2f, 24), 70f, 8f);
            CreateSweeper(parent, "Sweeper_B", new Vector3(0, 1.2f, 38), -85f, 8f);
            CreateSideWalls(parent, 12, 47, 7);
            CreateSign(parent, "Section1Sign", new Vector3(-5.5f, 2.2f, 15), "SECTION 1\nLAVA TIMING\nJump between safe platforms");
        }

        private static Rigidbody BuildPuzzle(Transform parent, T7_CourseManager manager)
        {
            CreateFloor("PuzzleFloor", parent, new Vector3(0, 0, 70), new Vector3(12, 1, 36));
            CreateSideWalls(parent, 52, 89, 7);
            CreateSign(parent, "PuzzleSign", new Vector3(-5.5f, 2.2f, 57), "SECTION 2\nPUSH PUZZLE\nLeft-click the yellow crate onto the plate");
            var crate = Primitive("PushCrate", PrimitiveType.Cube, parent, new Vector3(-3, 1, 61),
                new Vector3(1.8f, 1.8f, 1.8f), Mats["Crate"]);
            var body = crate.AddComponent<Rigidbody>();
            body.mass = 2f; body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var plateGo = Primitive("PressurePlate", PrimitiveType.Cube, parent, new Vector3(3, 0.6f, 72),
                new Vector3(3, 0.25f, 3), Mats["PlateOff"]);
            plateGo.GetComponent<BoxCollider>().isTrigger = true;
            var plate = plateGo.AddComponent<T7_PressurePlate>();
            plate.SetDesignatedCrate(body);
            plate.SetRenderer(plateGo.GetComponent<Renderer>());
            var gate = CreateGate(parent, "PuzzleGate", new Vector3(0, 2f, 84), new Vector3(0, 7f, 84));
            gate.Bind(plate);
            var console = CreateInteractable(parent, "GateConsole", new Vector3(-4.5f, 1.2f, 80),
                new Vector3(1.2f, 2.2f, 0.7f), Mats["Cyan"], "Gate Console", "", false);
            console.ConfigureAction(T7_CourseInteractable.CourseAction.ReportPressurePlate, manager, gate, plate);
            return body;
        }

        private static void BuildSection3(Transform parent)
        {
            CreateLava(parent, new Vector3(0, -1f, 112), new Vector3(14, 1, 38));
            CreateFloor("Entry", parent, new Vector3(0, 0, 96), new Vector3(7, 0.8f, 5));
            CreateMovingPlatform(parent, "MovingPlatform_A", new Vector3(-1.5f, 0, 102), new Vector3(3, 0, 0), 4.5f);
            CreateMovingPlatform(parent, "MovingPlatform_B", new Vector3(1.5f, 0.35f, 110), new Vector3(-3, 0, 0), 5f);
            CreateMovingPlatform(parent, "MovingPlatform_C", new Vector3(-1.5f, 0, 118), new Vector3(3, 0, 0), 4.5f);
            CreateFloor("Exit", parent, new Vector3(0, 0, 128), new Vector3(7, 0.8f, 14));
            CreateSweeper(parent, "PrecisionBar_A", new Vector3(0, 1.2f, 113), 55, 6);
            CreateSideWalls(parent, 94, 134, 7);
            CreateSign(parent, "Section3Sign", new Vector3(-5.5f, 2.2f, 97), "SECTION 3\nPRECISION MOVEMENT\nRide all three moving platforms");
        }

        private static void BuildFinal(Transform parent)
        {
            CreateLava(parent, new Vector3(0, -1f, 160), new Vector3(14, 1, 42));
            for (int i = 0; i < 6; i++)
            {
                float x = i % 2 == 0 ? -1.5f : 1.5f;
                CreateFloor($"NarrowPath_{i}", parent, new Vector3(x, 0, 144 + i * 5),
                    new Vector3(3.3f, 0.7f, 4.2f));
            }
            CreateMovingPlatform(parent, "FinalMovingPlatform", new Vector3(0, 0, 176), new Vector3(4, 0, 0), 3f);
            CreateSweeper(parent, "FinalSweeper_A", new Vector3(0, 1.1f, 151), 105, 7);
            CreateSweeper(parent, "FinalSweeper_B", new Vector3(0, 1.1f, 166), -110, 7);
            CreateSideWalls(parent, 141, 181, 7);
            CreateSign(parent, "FinalSign", new Vector3(-5.5f, 2.2f, 143), "FINAL CHALLENGE\nCombine timing, balance, and movement");
        }

        private static T7_CourseInteractable BuildFinish(Transform parent, T7_CourseManager manager)
        {
            CreateFloor("FinishFloor", parent, new Vector3(0, 0, 187), new Vector3(12, 1, 14));
            var beacon = CreateInteractable(parent, "FinishBeacon", new Vector3(0, 2, 187),
                new Vector3(1.8f, 4f, 1.8f), Mats["Gold"], "Finish Beacon", "Run complete", true);
            var interactionTarget = beacon.gameObject.AddComponent<BoxCollider>();
            interactionTarget.isTrigger = true;
            interactionTarget.center = new Vector3(0f, 0f, -0.7f);
            interactionTarget.size = new Vector3(2.2f, 1.2f, 2.5f);
            beacon.ConfigureAction(T7_CourseInteractable.CourseAction.FinishCourse, manager);
            CreateSign(parent, "FinishSign", new Vector3(-5.5f, 2.2f, 185), "FINISH\nCheckpoint 3 unlocks this beacon\nLook at it and press E");
            return beacon;
        }

        private static void CreateCheckpoint(Transform parent, int index, float z, T7_CourseManager manager)
        {
            CreateFloor("CheckpointFloor", parent, new Vector3(0, 0, z), new Vector3(12, 1, 6));
            var zone = Primitive("HealingZone", PrimitiveType.Cylinder, parent, new Vector3(0, 0.6f, z),
                new Vector3(3.5f, 0.15f, 3.5f), Mats["Checkpoint"]);
            zone.GetComponent<Collider>().isTrigger = true;
            Transform point = Marker("RespawnPoint", parent, new Vector3(0, 1.1f, z));
            var checkpoint = zone.AddComponent<T7_Checkpoint>();
            checkpoint.Configure(index, manager, point, zone.GetComponent<Renderer>());
            CreateSign(parent, $"Checkpoint{index}Sign", new Vector3(-5.5f, 2.2f, z),
                $"CHECKPOINT {index}\nBlue activates to green\nHealing: 10 HP/second");
        }

        private static T7_Gate CreateGate(Transform parent, string name, Vector3 closed, Vector3 open)
        {
            var go = Primitive(name, PrimitiveType.Cube, parent, closed, new Vector3(11, 4, 0.7f), Mats["WeatheredMetal"]);
            var gate = go.AddComponent<T7_Gate>();
            gate.Configure(closed, open, 4f);
            return gate;
        }

        private static T7_CourseInteractable CreateInteractable(Transform parent, string name, Vector3 position,
            Vector3 scale, Material material, string displayName, string message, bool locked)
        {
            var go = Primitive(name, PrimitiveType.Cube, parent, position, scale, material);
            var item = go.AddComponent<T7_CourseInteractable>();
            item.Configure(displayName, message, locked, go.GetComponent<Renderer>());
            return item;
        }

        private static void CreateMovingPlatform(Transform parent, string name, Vector3 position, Vector3 delta, float duration)
        {
            var go = Primitive(name, PrimitiveType.Cube, parent, position, new Vector3(5.5f, 0.6f, 5.5f), Mats["Safe"]);
            go.AddComponent<T7_MovingPlatform>().Configure(Vector3.zero, delta, duration);
        }

        private static void CreateSweeper(Transform parent, string name, Vector3 position, float speed, float length)
        {
            var pivot = new GameObject(name);
            pivot.transform.SetParent(parent);
            pivot.transform.position = position;
            pivot.AddComponent<T7_RotatingHazard>().Configure(Vector3.up, speed);
            var bar = Primitive("DamageBar", PrimitiveType.Cube, pivot.transform, position, new Vector3(length, 0.45f, 0.45f), Mats["Obstacle"]);
            bar.transform.localPosition = Vector3.zero;
            bar.AddComponent<T7_DamageObstacle>().Configure(15f, 0.75f);
        }

        private static void CreateEnvironment(Transform parent)
        {
            for (int z = 0; z <= 190; z += 10)
            {
                Primitive($"LeftWall_{z}", PrimitiveType.Cube, parent, new Vector3(-8, 2, z),
                    new Vector3(1, 4, 10), Mats["Wall"]);
                Primitive($"RightWall_{z}", PrimitiveType.Cube, parent, new Vector3(8, 2, z),
                    new Vector3(1, 4, 10), Mats["Wall"]);
            }
        }

        private static void BuildSectionGuide(Transform parent, string name, Vector3 position,
            Transform player, Camera camera, string line)
        {
            Primitive($"{name}Pedestal", PrimitiveType.Cube, parent,
                new Vector3(position.x, 0f, position.z), new Vector3(2.2f, 0.5f, 2.2f),
                Mats["ReinforcedConcrete"]);
            BuildTutorialNPC(parent, name, position, player, camera, new[] { line });
        }

        private static void BuildTutorialNPC(Transform parent, string name, Vector3 position,
            Transform player, Camera camera, IReadOnlyList<string> lines)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(T7_UpgradeAssetBuilder.NpcPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("Tutorial NPC prefab is unavailable.");
                return;
            }
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            instance.name = name;
            instance.transform.position = position;
            Vector3 direction = player.position - instance.transform.position;
            direction.y = 0f;
            instance.transform.rotation = Quaternion.LookRotation(direction);
            T7_TutorialNPC npc = instance.GetComponent<T7_TutorialNPC>();
            T7_WorldSpaceDialogue dialogue = instance.GetComponentInChildren<T7_WorldSpaceDialogue>(true);
            npc.Configure(instance.GetComponent<Animator>(), dialogue, player);
            npc.ConfigureDialogue(lines);
            dialogue.Configure(
                dialogue.GetComponent<Canvas>(),
                instance.transform.Find("WorldSpaceDialogue/PromptPanel")?.gameObject,
                instance.transform.Find("WorldSpaceDialogue/PromptPanel/Prompt")?.GetComponent<TMP_Text>(),
                instance.transform.Find("WorldSpaceDialogue/DialoguePanel")?.gameObject,
                instance.transform.Find("WorldSpaceDialogue/DialoguePanel/Speaker")?.GetComponent<TMP_Text>(),
                instance.transform.Find("WorldSpaceDialogue/DialoguePanel/Line")?.GetComponent<TMP_Text>(),
                camera.transform);
        }

        private static void BuildVisualDressing(Transform parent, Transform lighting)
        {
            Material metal = Mats["WeatheredMetal"];
            Material rock = Mats["VolcanicRock"];
            Material stripe = Mats["HazardStripe"];
            Material props = Mats["IndustrialProps"];
            Material crate = Mats["IndustrialCrate"];

            for (int z = 10; z <= 180; z += 20)
            {
                CreatePipe(parent, new Vector3(-7.35f, 2.7f, z), metal);
                CreatePipe(parent, new Vector3(7.35f, 1.4f, z + 8), metal);
                CreateRouteLight(lighting, new Vector3(0f, 3.7f, z), z % 40 == 10);
            }

            float[] lavaEdges = { 16f, 27f, 40f, 100f, 112f, 126f, 144f, 160f, 178f };
            foreach (float z in lavaEdges)
            {
                CreateRock(parent, new Vector3(-6.25f, 0.2f, z), rock, z);
                CreateRock(parent, new Vector3(6.25f, 0.2f, z + 1.5f), rock, z + 1f);
            }
            foreach (float z in new[] { 29f, 112f, 160f })
                CreateLavaParticles(parent, new Vector3(0f, 0.1f, z));

            foreach (float z in new[] { 24f, 38f, 113f, 151f, 166f })
            {
                Primitive($"HazardStripe_Left_{z}", PrimitiveType.Cube, parent, new Vector3(-5.8f, 0.15f, z),
                    new Vector3(1.7f, 0.12f, 0.5f), stripe);
                Primitive($"HazardStripe_Right_{z}", PrimitiveType.Cube, parent, new Vector3(5.8f, 0.15f, z),
                    new Vector3(1.7f, 0.12f, 0.5f), stripe);
            }

            PlaceImportedProp(parent, "Prop_Barrel1", new Vector3(-6.3f, 0.5f, 65f), Vector3.one * 1.1f, props);
            PlaceImportedProp(parent, "Prop_Locker", new Vector3(6.45f, 0.5f, 76f), Vector3.one * 1.2f, props);
            PlaceImportedProp(parent, "Prop_SatelliteDish", new Vector3(-6.4f, 0.6f, 132f), Vector3.one, props);
            PlaceImportedProp(parent, "Prop_Shelves_WideTall", new Vector3(6.35f, 0.5f, 183f), Vector3.one, props);
            PlaceImportedProp(parent, "Prop_Crate_Large", new Vector3(-6.2f, 0.65f, 184f), Vector3.one, crate);
        }

        private static void CreatePipe(Transform parent, Vector3 position, Material material)
        {
            GameObject pipe = Primitive($"WallPipe_{position.z}", PrimitiveType.Cylinder, parent, position,
                new Vector3(0.22f, 3.5f, 0.22f), material);
            pipe.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Object.DestroyImmediate(pipe.GetComponent<Collider>());
        }

        private static void CreateRock(Transform parent, Vector3 position, Material material, float seed)
        {
            GameObject rock = Primitive($"VolcanicRock_{seed}", PrimitiveType.Sphere, parent, position,
                new Vector3(1.1f + seed % 3f * 0.15f, 0.65f, 0.8f), material);
            rock.transform.rotation = Quaternion.Euler(seed * 7f % 25f, seed * 13f % 180f, seed * 3f % 20f);
            Object.DestroyImmediate(rock.GetComponent<Collider>());
        }

        private static void CreateRouteLight(Transform parent, Vector3 position, bool warm)
        {
            var go = new GameObject(warm ? "WarmLavaLight" : "CoolRouteLight");
            go.transform.SetParent(parent);
            go.transform.position = position;
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = warm ? new Color(1f, 0.22f, 0.04f) : new Color(0.25f, 0.65f, 1f);
            light.intensity = warm ? 4f : 2.5f;
            light.range = warm ? 7f : 8f;
            light.shadows = LightShadows.None;
        }

        private static void CreateLavaParticles(Transform parent, Vector3 position)
        {
            var go = new GameObject($"LavaEmbers_{position.z}", typeof(ParticleSystem));
            go.transform.SetParent(parent);
            go.transform.position = position;
            ParticleSystem particles = go.GetComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.09f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.18f, 0.01f, 0.9f), new Color(1f, 0.72f, 0.08f, 1f));
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.rateOverTime = 12f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(11f, 0.1f, 18f);
            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.25f;
            noise.frequency = 0.35f;
        }

        private static void PlaceImportedProp(Transform parent, string assetName, Vector3 position,
            Vector3 scale, Material material)
        {
            string path = $"Assets/Tugas7/ThirdParty/Industrial/Models/{assetName}.fbx";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return;
            GameObject instance = Object.Instantiate(asset, parent);
            instance.name = assetName;
            instance.transform.position = position;
            instance.transform.localScale = scale;
            instance.transform.rotation = Quaternion.Euler(0f, position.x < 0f ? 90f : -90f, 0f);
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = material;
        }

        private static void CreateSideWalls(Transform parent, float fromZ, float toZ, float x)
        {
            float length = toZ - fromZ;
            Primitive("LeftBoundary", PrimitiveType.Cube, parent, new Vector3(-x, 2, fromZ + length / 2),
                new Vector3(0.5f, 4, length), Mats["Wall"]);
            Primitive("RightBoundary", PrimitiveType.Cube, parent, new Vector3(x, 2, fromZ + length / 2),
                new Vector3(0.5f, 4, length), Mats["Wall"]);
        }

        private static void CreateLighting(Transform parent)
        {
            var sun = new GameObject("Directional Light");
            sun.transform.SetParent(parent);
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.2f; light.shadows = LightShadows.Soft;
            RenderSettings.ambientIntensity = 0.7f;
            CreatePostProcessing(parent);
        }

        private static void CreatePostProcessing(Transform parent)
        {
            const string profilePath = "Assets/Tugas7/T7_VolcanicFacilityVolume.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
            }
            Bloom bloom = GetOrAddVolumeComponent<Bloom>(profile);
            bloom.intensity.Override(0.45f);
            bloom.threshold.Override(1.15f);
            bloom.scatter.Override(0.55f);
            ColorAdjustments color = GetOrAddVolumeComponent<ColorAdjustments>(profile);
            color.postExposure.Override(-0.05f);
            color.contrast.Override(8f);
            color.saturation.Override(-4f);
            Vignette vignette = GetOrAddVolumeComponent<Vignette>(profile);
            vignette.intensity.Override(0.18f);
            vignette.smoothness.Override(0.45f);
            Tonemapping tonemapping = GetOrAddVolumeComponent<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);
            EditorUtility.SetDirty(profile);

            var volumeObject = new GameObject("Global Volcanic Facility Volume");
            volumeObject.transform.SetParent(parent);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = profile;
        }

        private static T GetOrAddVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent =>
            profile.TryGet(out T component) ? component : profile.Add<T>();

        private static void CreateFloor(string name, Transform parent, Vector3 pos, Vector3 scale) =>
            Primitive(name, PrimitiveType.Cube, parent, pos, scale, Mats["Safe"]);
        private static void CreateLava(Transform parent, Vector3 pos, Vector3 scale)
        {
            var lava = Primitive("Lava", PrimitiveType.Cube, parent, pos, scale, Mats["Lava"]);
            lava.GetComponent<Collider>().isTrigger = true;
            lava.AddComponent<T7_DamageVolume>().DamagePerSecond = 20f;
            lava.AddComponent<T7_LavaMaterialController>().Configure(Mats["Lava"]);
        }
        private static void CreateReset(Transform parent, Vector3 pos, Vector3 scale)
        {
            var reset = Primitive("ResetVolume", PrimitiveType.Cube, parent, pos, scale, Mats["Invisible"]);
            reset.GetComponent<Collider>().isTrigger = true;
            reset.GetComponent<Renderer>().enabled = false;
            reset.AddComponent<T7_ResetVolume>();
        }

        private static void CreateSign(Transform parent, string name, Vector3 position, string text)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0, -90, 0);
            go.transform.localScale = Vector3.one * 0.01f;
            var rect = (RectTransform)go.transform; rect.sizeDelta = new Vector2(440, 220);
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            Image bg = CreateImage("Background", rect, new Color(0.02f, 0.04f, 0.07f, 0.94f), Vector2.zero, Vector2.one);
            CreateText("Objective", bg.rectTransform, text, 30, new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.95f));
        }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static Image CreateImage(string name, RectTransform parent, Color color, Vector2 min, Vector2 max,
            Sprite sprite = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>(); image.color = color; image.sprite = sprite; return image;
        }

        private static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        private static TMP_Text CreateText(string name, RectTransform parent, string value, float size, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value; text.fontSize = size; text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center; text.enableWordWrapping = true;
            return text;
        }

        private static GameObject Primitive(string name, PrimitiveType type, Transform parent,
            Vector3 worldPosition, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.SetParent(parent); go.transform.position = worldPosition; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static Transform Marker(string name, Transform parent, Vector3 position)
        {
            Transform marker = Group(name, parent);
            marker.position = position;
            return marker;
        }
        private static Transform Group(string name, Transform parent)
        {
            var go = new GameObject(name); go.transform.SetParent(parent); return go.transform;
        }

        private static void CreateMaterials()
        {
            Mats.Clear();
            Mats["Lava"] = AssetDatabase.LoadAssetAtPath<Material>(T7_UpgradeAssetBuilder.LavaMaterialPath);
            Mats["Safe"] = LoadEnvironmentMaterial("ReinforcedConcrete");
            MakeMaterial("Checkpoint", new Color(0.05f, 0.25f, 1f), new Color(0.05f, 0.25f, 2f));
            Mats["Obstacle"] = LoadEnvironmentMaterial("DangerMetal");
            MakeMaterial("Crate", new Color(1f, 0.65f, 0.03f));
            MakeMaterial("PlateOff", new Color(0.5f, 0.08f, 0.72f), new Color(0.3f, 0.02f, 0.5f));
            Mats["Cyan"] = LoadEnvironmentMaterial("InteractableMetal");
            Mats["Gold"] = LoadEnvironmentMaterial("GoldMetal");
            Mats["Wall"] = LoadEnvironmentMaterial("DarkConcreteWall");
            MakeMaterial("Invisible", Color.clear);
            Mats["WeatheredMetal"] = LoadEnvironmentMaterial("WeatheredMetal");
            Mats["ReinforcedConcrete"] = LoadEnvironmentMaterial("ReinforcedConcrete");
            Mats["VolcanicRock"] = LoadEnvironmentMaterial("VolcanicRock");
            Mats["HazardStripe"] = LoadEnvironmentMaterial("HazardStripe");
            Mats["IndustrialProps"] = LoadEnvironmentMaterial("IndustrialProps");
            Mats["IndustrialCrate"] = LoadEnvironmentMaterial("IndustrialCrate");
        }

        private static Material LoadEnvironmentMaterial(string name) =>
            AssetDatabase.LoadAssetAtPath<Material>($"Assets/Tugas7/Materials/Environment/T7_{name}.mat");

        private static void MakeMaterial(string name, Color baseColor, Color? emission = null)
        {
            string path = $"{MaterialFolder}/T7_{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else mat.shader = shader;
            mat.color = baseColor;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(mat);
            Mats[name] = mat;
        }

        private static void AddSceneToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int index = scenes.FindIndex(s => s.path == ScenePath);
            if (index >= 0) scenes[index] = new EditorBuildSettingsScene(ScenePath, true);
            else scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void CleanupBlankSceneDefaults(Scene scene)
        {
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (go.name == "Main Camera" || go.name == "Directional Light")
                    Object.DestroyImmediate(go);
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
