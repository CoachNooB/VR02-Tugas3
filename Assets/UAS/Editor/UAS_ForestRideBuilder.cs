using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class UAS_ForestRideBuilder
{
    private const string ScenePath = "Assets/UAS/Scenes/UAS_Harry_Forrest.unity";
    private const string MaterialFolder = "Assets/UAS/Materials/TeddyPicnic";
    private const string PrefabFolder = "Assets/UAS/Prefabs";

    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    [MenuItem("UAS/Build Forest Teddy Picnic Ride")]
    public static void Build()
    {
        EnsureFolder("Assets/UAS", "Editor");
        EnsureFolder("Assets/UAS", "Prefabs");
        EnsureFolder("Assets/UAS", "Materials");
        EnsureFolder("Assets/UAS/Materials", "TeddyPicnic");
        CreateMaterials();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (FindRoot(scene, "UAS_ForestTeddySection") != null)
        {
            EnsureAnimatedAnimalsInPrefab();
            ApplyVisualFixesToPrefabs();
            ApplyVisualFixesToScene(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("UAS Forest Teddy Picnic ride section already exists; visual settings refreshed safely.");
            return;
        }

        RemoveRoot(scene, "UAS_ForestTeddySection");
        RemoveRoot(scene, "UAS_DemoRide");
        RemoveRoot(scene, "Demo_Sun");

        GameObject section = new GameObject("UAS_ForestTeddySection");
        SceneManager.MoveGameObjectToScene(section, scene);
        Transform environment = AdoptOrCreateRoot(scene, "Environment", section.transform);
        Transform picnic = AdoptOrCreateRoot(scene, "Picnic_Set", section.transform);
        Transform teddies = AdoptOrCreateRoot(scene, "Teddy_Family", section.transform);
        Transform oldLighting = FindRoot(scene, "Lighting");
        if (oldLighting != null)
        {
            Object.DestroyImmediate(oldLighting.gameObject);
        }

        Transform oldCamera = FindRoot(scene, "Cinematic_Camera");
        if (oldCamera != null)
        {
            Object.DestroyImmediate(oldCamera.gameObject);
        }

        Transform displayLighting = Child(section.transform, "Display_Lighting");
        Transform displayInteractions = Child(section.transform, "Display_Interactions");
        Transform worldUi = Child(section.transform, "WorldSpace_UI");
        Transform integration = Child(section.transform, "Integration_Points");

        Transform trackEntry = Marker(integration, "Track_Entry", new Vector3(-22f, 0.5f, -8.5f));
        Transform displayStop = Marker(integration, "Display_Stop", new Vector3(0f, 0.5f, -8.5f));
        Transform trackExit = Marker(integration, "Track_Exit", new Vector3(22f, 0.5f, -8.5f));
        Transform displayTriggerMarker = Child(integration, "Display_Trigger");
        displayTriggerMarker.position = displayStop.position;

        UAS_TeddyAnimator[] teddyAnimators = ConfigureTeddies(teddies);
        Light[] warmLights = CreateDisplayLights(displayLighting);
        Renderer[] fairyLights = CreateFairyLights(displayLighting);
        Transform centerpiece = FindDeepChild(picnic, "Flower_Centerpiece");
        if (centerpiece == null)
        {
            centerpiece = Primitive("Flower_Centerpiece", PrimitiveType.Cylinder, picnic,
                new Vector3(0f, 0.45f, 0f), new Vector3(0.45f, 0.35f, 0.45f),
                Materials["Cart_Gold"], false).transform;
        }

        GameObject sequenceObject = new GameObject("Forest_Display_Sequence");
        sequenceObject.transform.SetParent(displayInteractions, false);
        AudioSource audioSource = sequenceObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        UAS_ProceduralChime chime = sequenceObject.AddComponent<UAS_ProceduralChime>();
        UAS_ForestDisplaySequence sequence = sequenceObject.AddComponent<UAS_ForestDisplaySequence>();
        sequence.Configure(warmLights, fairyLights, teddyAnimators, centerpiece, chime);

        GameObject musicBox = Primitive("Picnic_MusicBox_Panel", PrimitiveType.Cube, displayInteractions,
            new Vector3(0f, 1.1f, -5.9f), new Vector3(1.1f, 0.8f, 0.25f),
            Materials["Cart_Gold"], true);
        UAS_ForestEffectInteractable effectInteractable = musicBox.AddComponent<UAS_ForestEffectInteractable>();
        GameObject musicPrompt = CreatePrompt(musicBox.transform, "E  Replay Finale", new Vector3(0f, 0.8f, 0f));
        effectInteractable.ConfigureFeedback("Replay Picnic Finale", "Available only while stopped",
            musicPrompt, new[] { musicBox.GetComponent<Renderer>() });
        effectInteractable.Configure(sequence, null);

        UAS_RideZoneTrigger displayTrigger = CreateTrigger(
            "Forest_Display_Trigger",
            displayTriggerMarker,
            Vector3.zero,
            new Vector3(3.5f, 3f, 5f),
            UAS_RideZoneTrigger.ZoneMode.Display);
        displayTrigger.Configure(UAS_RideZoneTrigger.ZoneMode.Display, null, sequence, null);

        UAS_RideStatusUIH statusUi = CreateWorldSpaceUi(worldUi);

        PrefabUtility.SaveAsPrefabAssetAndConnect(
            section,
            $"{PrefabFolder}/UAS_ForestTeddySection.prefab",
            InteractionMode.AutomatedAction);

        GameObject demo = new GameObject("UAS_DemoRide");
        SceneManager.MoveGameObjectToScene(demo, scene);
        Transform track = Child(demo.transform, "Track_44m");
        BuildTrack(track);
        Transform waypointsRoot = Child(demo.transform, "Waypoints");
        List<UAS_RideVehicleController.WaypointSetting> waypoints = BuildWaypoints(waypointsRoot);

        Transform boarding = Child(demo.transform, "Boarding_Area");
        UAS_GateLeverInteractable gate = BuildBoardingArea(boarding);
        Transform finish = Child(demo.transform, "Finish_Area");
        Transform exitAnchor = BuildFinishArea(finish);
        Transform cartRoot = Child(demo.transform, "Mini_Cart");
        CartParts cart = BuildCart(cartRoot);

        cart.Vehicle.ConfigureWaypoints(waypoints);
        cart.Seat.Configure(cart.SeatAnchor, cart.Vehicle, null);
        cart.Start.Configure(cart.Vehicle, null);
        PrefabUtility.SaveAsPrefabAssetAndConnect(
            cartRoot.gameObject,
            $"{PrefabFolder}/UAS_DemoMiniCart.prefab",
            InteractionMode.AutomatedAction);
        cart.Seat.Configure(cart.SeatAnchor, cart.Vehicle, gate);
        cart.Start.Configure(cart.Vehicle, gate);
        effectInteractable.Configure(sequence, cart.Vehicle);

        UAS_RideZoneTrigger boardingTrigger = CreateTrigger(
            "Boarding_Trigger",
            boarding,
            new Vector3(0f, 1f, 0f),
            new Vector3(7f, 2f, 6f),
            UAS_RideZoneTrigger.ZoneMode.Boarding);
        boardingTrigger.Configure(UAS_RideZoneTrigger.ZoneMode.Boarding, statusUi, null, null);
        UAS_RideZoneTrigger finishTrigger = CreateTrigger(
            "Finish_Trigger",
            finish,
            new Vector3(0f, 1f, 2.5f),
            new Vector3(5f, 2f, 5f),
            UAS_RideZoneTrigger.ZoneMode.Finish);
        finishTrigger.Configure(UAS_RideZoneTrigger.ZoneMode.Finish, statusUi, null, exitAnchor);

        UAS_DemoPlayerController player = BuildPlayer(demo.transform);
        statusUi.Configure(
            FindText(worldUi, "Boarding_Instructions"),
            FindText(worldUi, "Forest_Title"),
            FindText(worldUi, "Ride_State"),
            FindText(worldUi, "Sequence_Stage"),
            cart.Vehicle,
            sequence);

        CreateSun(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("UAS Forest Teddy Picnic ride section built successfully.");
    }

    [MenuItem("UAS/Add Animated Forest Animals")]
    public static void AddAnimatedAnimals()
    {
        EnsureAnimatedAnimalsInPrefab();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Added two hopping bunnies, two flapping pigeons, and one orbiting dragonfly.");
    }

    public static void ApplyVisualFixes()
    {
        ApplyVisualFixesToPrefabs();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ApplyVisualFixesToScene(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("UAS Forest Teddy Picnic visual settings updated.");
    }

    public static void CapturePreview()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (Canvas canvas in scene.GetRootGameObjects()
                     .SelectMany(root => root.GetComponentsInChildren<Canvas>(true)))
        {
            canvas.gameObject.SetActive(false);
        }

        GameObject cameraObject = new GameObject("UAS_Preview_Camera");
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        cameraObject.transform.position = new Vector3(10f, 6f, -11f);
        cameraObject.transform.LookAt(new Vector3(0f, 1.2f, 0f));
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        camera.fieldOfView = 52f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 120f;
        RenderTexture target = new RenderTexture(1280, 720, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
        Color[] pixels = image.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i].r = Mathf.LinearToGammaSpace(pixels[i].r);
            pixels[i].g = Mathf.LinearToGammaSpace(pixels[i].g);
            pixels[i].b = Mathf.LinearToGammaSpace(pixels[i].b);
        }
        image.SetPixels(pixels);
        image.Apply();
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? ".";
        File.WriteAllBytes(Path.Combine(projectRoot, "UAS_ForestTeddyRide_Preview.png"), image.EncodeToPNG());
        RenderTexture.active = null;
        camera.targetTexture = null;
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(image);
        Object.DestroyImmediate(cameraObject);
        Debug.Log("UAS Forest Teddy Picnic preview captured.");
    }

    private static void CreateMaterials()
    {
        Materials.Clear();
        CreateMaterial("Track_Rail", new Color(0.07f, 0.08f, 0.09f), 0.8f, 0.65f);
        CreateMaterial("Track_Wood", new Color(0.28f, 0.12f, 0.045f), 0f, 0.25f);
        CreateMaterial("Track_Ballast", new Color(0.25f, 0.27f, 0.24f), 0f, 0.15f);
        CreateMaterial("Cart_Red", new Color(0.58f, 0.035f, 0.025f), 0.25f, 0.5f);
        CreateMaterial("Cart_Gold", new Color(0.9f, 0.52f, 0.06f), 0.55f, 0.65f);
        CreateMaterial("FairyLight_Off", new Color(0.24f, 0.16f, 0.08f), 0f, 0.2f, Color.black);
        CreateMaterial("FairyLight_On", new Color(1f, 0.72f, 0.25f), 0f, 0.35f,
            new Color(1f, 0.45f, 0.08f) * 3f);
    }

    private static void ApplyVisualFixesToPrefabs()
    {
        FixPrefabCanvasRotations($"{PrefabFolder}/UAS_ForestTeddySection.prefab");
        FixPrefabCanvasRotations($"{PrefabFolder}/UAS_DemoMiniCart.prefab");
    }

    private static void EnsureAnimatedAnimalsInPrefab()
    {
        string sectionPath = $"{PrefabFolder}/UAS_ForestTeddySection.prefab";
        GameObject section = PrefabUtility.LoadPrefabContents(sectionPath);
        try
        {
            Transform environment = FindDeepChild(section.transform, "Environment");
            if (environment == null)
            {
                throw new InvalidOperationException("Forest section prefab has no Environment root.");
            }

            Transform existing = environment.Find("Forest_Animals");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            Transform animals = Child(environment, "Forest_Animals");
            Transform orbitCenter = Child(animals, "Animal_Orbit_Center");
            orbitCenter.localPosition = new Vector3(0f, 2.6f, 0.5f);

            CreateAnimatedAnimal(
                animals,
                "Bunny_Hopper_A",
                "Assets/Monsters/Prefabs/Bunny.prefab",
                new Vector3(-4.2f, 0.15f, 2.7f),
                new Vector3(0f, 125f, 0f),
                0.72f,
                UAS_ForestAnimalAnimator.AnimationStyle.Hop,
                null,
                0f,
                0.8f,
                0f);
            CreateAnimatedAnimal(
                animals,
                "Bunny_Hopper_B",
                "Assets/Monsters/Prefabs/Bunny.prefab",
                new Vector3(4.4f, 0.15f, 2.2f),
                new Vector3(0f, -110f, 0f),
                0.62f,
                UAS_ForestAnimalAnimator.AnimationStyle.Hop,
                null,
                0f,
                0.68f,
                Mathf.PI * 0.65f);
            CreateAnimatedAnimal(
                animals,
                "Pigeon_Flap_A",
                "Assets/Monsters/Prefabs/Pigeon.prefab",
                new Vector3(-3.5f, 3.2f, -0.8f),
                new Vector3(0f, 35f, 0f),
                0.58f,
                UAS_ForestAnimalAnimator.AnimationStyle.Flap,
                null,
                0f,
                1.15f,
                0f);
            CreateAnimatedAnimal(
                animals,
                "Pigeon_Flap_B",
                "Assets/Monsters/Prefabs/Pigeon.prefab",
                new Vector3(3.2f, 3.65f, 1.1f),
                new Vector3(0f, -145f, 0f),
                0.52f,
                UAS_ForestAnimalAnimator.AnimationStyle.Flap,
                null,
                0f,
                1.32f,
                Mathf.PI * 0.4f);
            CreateAnimatedAnimal(
                animals,
                "Dragonfly_Orbit",
                "Assets/Monsters/Prefabs/Dragon Fly.prefab",
                new Vector3(2.4f, 2.8f, 0.5f),
                Vector3.zero,
                0.32f,
                UAS_ForestAnimalAnimator.AnimationStyle.Orbit,
                orbitCenter,
                2.6f,
                0.22f,
                0f);

            PrefabUtility.SaveAsPrefabAsset(section, sectionPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(section);
        }
    }

    private static void CreateAnimatedAnimal(
        Transform parent,
        string name,
        string prefabPath,
        Vector3 localPosition,
        Vector3 localEulerAngles,
        float modelScale,
        UAS_ForestAnimalAnimator.AnimationStyle style,
        Transform orbitCenter,
        float orbitRadius,
        float speed,
        float phase)
    {
        GameObject wrapper = new GameObject(name);
        wrapper.transform.SetParent(parent, false);
        wrapper.transform.localPosition = localPosition;
        wrapper.transform.localEulerAngles = localEulerAngles;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            throw new InvalidOperationException($"Missing animal prefab: {prefabPath}");
        }

        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, wrapper.transform);
        model.name = $"{name}_Model";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one * modelScale;
        foreach (Animator importedAnimator in model.GetComponentsInChildren<Animator>(true))
        {
            importedAnimator.enabled = false;
        }

        Transform leftWing = model.GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(item => item.name == "Wing1.L");
        Transform rightWing = model.GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(item => item.name == "Wing1.R");
        UAS_ForestAnimalAnimator animator = wrapper.AddComponent<UAS_ForestAnimalAnimator>();
        animator.Configure(style, wrapper.transform, leftWing, rightWing, orbitCenter,
            Mathf.Max(0.1f, orbitRadius), speed, phase);
    }

    private static void FixPrefabCanvasRotations(string path)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            FixCanvasRotations(root.transform);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ApplyVisualFixesToScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            FixCanvasRotations(root.transform);
        }

        Camera demoCamera = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
            .FirstOrDefault(camera => camera.name == "Demo_Camera");
        if (demoCamera != null
            && demoCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() == null)
        {
            demoCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        }

        Light sun = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Light>(true))
            .FirstOrDefault(light => light.name == "Demo_Sun");
        if (sun != null)
        {
            sun.intensity = 0.7f;
            RenderSettings.sun = sun;
        }

        RenderSettings.ambientIntensity = 0.7f;
        RenderSettings.reflectionIntensity = 0.6f;
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void FixCanvasRotations(Transform root)
    {
        foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
        {
            if (item.name == "Forest_Status_Panel" || item.name == "Interaction_Prompt")
            {
                item.localRotation = Quaternion.identity;
            }
        }
    }

    private static Material CreateMaterial(
        string name,
        Color color,
        float metallic,
        float smoothness,
        Color? emission = null)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            throw new InvalidOperationException("Universal Render Pipeline/Lit shader is unavailable.");
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        material.name = name;
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        if (emission.HasValue)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission.Value);
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(material);
        Materials[name] = material;
        return material;
    }

    private static UAS_TeddyAnimator[] ConfigureTeddies(Transform teddyRoot)
    {
        string[] names = { "Teddy_Brown", "Teddy_Honey", "Teddy_Cream" };
        UAS_TeddyAnimator.AnimationStyle[] styles =
        {
            UAS_TeddyAnimator.AnimationStyle.Wave,
            UAS_TeddyAnimator.AnimationStyle.Clap,
            UAS_TeddyAnimator.AnimationStyle.Bounce
        };
        UAS_TeddyAnimator[] result = new UAS_TeddyAnimator[3];
        for (int i = 0; i < names.Length; i++)
        {
            Transform teddy = FindDeepChild(teddyRoot, names[i]);
            if (teddy == null)
            {
                throw new InvalidOperationException($"Existing picnic teddy is missing: {names[i]}");
            }

            UAS_TeddyAnimator animator = teddy.GetComponent<UAS_TeddyAnimator>();
            if (animator == null)
            {
                animator = teddy.gameObject.AddComponent<UAS_TeddyAnimator>();
            }

            animator.Configure(
                styles[i],
                teddy,
                FindDeepChild(teddy, "Head"),
                FindDeepChild(teddy, "Arm_Left"),
                FindDeepChild(teddy, "Arm_Right"));
            result[i] = animator;
        }

        return result;
    }

    private static Light[] CreateDisplayLights(Transform parent)
    {
        Vector3[] positions =
        {
            new Vector3(-4f, 5f, -3f),
            new Vector3(4f, 5f, -3f)
        };
        List<Light> lights = new List<Light>();
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject lightObject = new GameObject($"Warm_Spotlight_{i + 1}");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = positions[i];
            lightObject.transform.LookAt(new Vector3(0f, 0.5f, 0f));
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1f, 0.58f, 0.3f);
            light.range = 14f;
            light.spotAngle = 55f;
            light.intensity = 0f;
            light.enabled = false;
            lights.Add(light);
        }

        return lights.ToArray();
    }

    private static Renderer[] CreateFairyLights(Transform parent)
    {
        List<Renderer> renderers = new List<Renderer>();
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2f / 12f;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 4.8f, 2.7f + (i % 2) * 0.35f,
                Mathf.Sin(angle) * 3.5f);
            GameObject light = Primitive($"Fairy_Light_{i + 1:00}", PrimitiveType.Sphere, parent,
                position, Vector3.one * 0.16f, Materials["FairyLight_Off"], false);
            renderers.Add(light.GetComponent<Renderer>());
        }

        return renderers.ToArray();
    }

    private static void BuildTrack(Transform parent)
    {
        Primitive("Ballast_Ground_Strip", PrimitiveType.Cube, parent, new Vector3(0f, -0.05f, -8.5f),
            new Vector3(44f, 0.15f, 3.5f), Materials["Track_Ballast"], true);
        Primitive("Rail_North", PrimitiveType.Cube, parent, new Vector3(0f, 0.12f, -7.3f),
            new Vector3(44f, 0.16f, 0.14f), Materials["Track_Rail"], true);
        Primitive("Rail_South", PrimitiveType.Cube, parent, new Vector3(0f, 0.12f, -9.7f),
            new Vector3(44f, 0.16f, 0.14f), Materials["Track_Rail"], true);
        int sleeperIndex = 0;
        for (float x = -21.75f; x <= 21.75f; x += 1.5f)
        {
            Primitive($"Sleeper_{sleeperIndex++:00}", PrimitiveType.Cube, parent,
                new Vector3(x, 0.02f, -8.5f), new Vector3(0.24f, 0.12f, 3.25f),
                Materials["Track_Wood"], true);
        }
    }

    private static List<UAS_RideVehicleController.WaypointSetting> BuildWaypoints(Transform parent)
    {
        string[] names =
        {
            "WP_00_Start", "WP_01_Approach", "WP_02_Slow",
            "WP_03_Display_Stop", "WP_04_Depart", "WP_05_Finish"
        };
        float[] x = { -18f, -6f, -2f, 0f, 7f, 18f };
        float[] speeds = { 0f, 4f, 1.5f, 1f, 2.5f, 3.5f };
        List<UAS_RideVehicleController.WaypointSetting> settings = new List<UAS_RideVehicleController.WaypointSetting>();
        for (int i = 0; i < names.Length; i++)
        {
            Transform point = Marker(parent, names[i], new Vector3(x[i], 0.5f, -8.5f));
            settings.Add(new UAS_RideVehicleController.WaypointSetting(
                point,
                speeds[i],
                i == 3 ? 7f : 0f,
                i == names.Length - 1));
        }

        return settings;
    }

    private static UAS_GateLeverInteractable BuildBoardingArea(Transform parent)
    {
        parent.position = new Vector3(-18f, 0f, -11f);
        Primitive("Boarding_Platform", PrimitiveType.Cube, parent, Vector3.zero,
            new Vector3(7f, 0.35f, 4.5f), Materials["Track_Wood"], true);
        GameObject gate = Primitive("Wooden_Gate", PrimitiveType.Cube, parent,
            new Vector3(-0.2f, 1.15f, 1.7f), new Vector3(4.4f, 1.8f, 0.18f),
            Materials["Track_Wood"], true);
        GameObject leverBase = Primitive("Gate_Lever_Base", PrimitiveType.Cylinder, parent,
            new Vector3(-2.7f, 0.9f, 1.35f), new Vector3(0.38f, 0.45f, 0.38f),
            Materials["Cart_Gold"], true);
        GameObject lever = Primitive("Gate_Lever_Handle", PrimitiveType.Cube, leverBase.transform,
            new Vector3(0f, 0.65f, 0f), new Vector3(0.16f, 1.2f, 0.16f),
            Materials["Cart_Red"], true);
        UAS_GateLeverInteractable interactable = leverBase.AddComponent<UAS_GateLeverInteractable>();
        interactable.Configure(lever.transform, gate.transform, new Vector3(0f, 2.3f, 0f));
        GameObject prompt = CreatePrompt(leverBase.transform, "E  Open Gate", new Vector3(0f, 1.6f, 0f));
        interactable.ConfigureFeedback("Open Gate", "Gate already open", prompt,
            new[] { lever.GetComponent<Renderer>(), leverBase.GetComponent<Renderer>() });
        return interactable;
    }

    private static Transform BuildFinishArea(Transform parent)
    {
        parent.position = new Vector3(18f, 0f, -11f);
        Primitive("Finish_Platform", PrimitiveType.Cube, parent, Vector3.zero,
            new Vector3(7f, 0.35f, 4.5f), Materials["Track_Wood"], true);
        Transform exit = Marker(parent, "Exit_Anchor", new Vector3(18f, 1f, -12f));
        exit.rotation = Quaternion.Euler(0f, -90f, 0f);
        return exit;
    }

    private static CartParts BuildCart(Transform cartRoot)
    {
        cartRoot.position = new Vector3(-18f, 0.5f, -8.5f);
        Rigidbody body = cartRoot.gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        BoxCollider interactionCollider = cartRoot.gameObject.AddComponent<BoxCollider>();
        interactionCollider.size = new Vector3(3.2f, 1.8f, 2.5f);
        interactionCollider.center = new Vector3(0f, 0.5f, 0f);
        UAS_RideVehicleController vehicle = cartRoot.gameObject.AddComponent<UAS_RideVehicleController>();

        Primitive("Cart_Body", PrimitiveType.Cube, cartRoot, new Vector3(0f, 0.35f, 0f),
            new Vector3(3.1f, 0.75f, 2.35f), Materials["Cart_Red"], false);
        Primitive("Cart_Gold_Trim", PrimitiveType.Cube, cartRoot, new Vector3(0f, 0.78f, 0f),
            new Vector3(3.25f, 0.16f, 2.48f), Materials["Cart_Gold"], false);
        Primitive("Seat_Back", PrimitiveType.Cube, cartRoot, new Vector3(0.85f, 1.15f, 0f),
            new Vector3(0.2f, 1.2f, 1.9f), Materials["Track_Wood"], false);
        Primitive("Dashboard", PrimitiveType.Cube, cartRoot, new Vector3(-0.95f, 1.05f, 0f),
            new Vector3(0.28f, 0.8f, 1.9f), Materials["Track_Wood"], false);

        Vector3[] wheels =
        {
            new Vector3(-1f, -0.15f, -1.05f), new Vector3(1f, -0.15f, -1.05f),
            new Vector3(-1f, -0.15f, 1.05f), new Vector3(1f, -0.15f, 1.05f)
        };
        for (int i = 0; i < wheels.Length; i++)
        {
            GameObject wheel = Primitive($"Wheel_{i + 1}", PrimitiveType.Cylinder, cartRoot, wheels[i],
                new Vector3(0.6f, 0.18f, 0.6f), Materials["Track_Rail"], false);
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        Transform seatAnchor = Marker(cartRoot, "Seat_Anchor", new Vector3(-18f, 1.1f, -8.5f));
        seatAnchor.localPosition = new Vector3(0.45f, 0.65f, 0f);
        GameObject seatInteraction = Primitive("Cart_Seat", PrimitiveType.Cube, cartRoot,
            new Vector3(0.3f, 0.8f, 0f), new Vector3(1.2f, 0.22f, 1.8f),
            Materials["Track_Wood"], true);
        UAS_RideSeatInteractable seat = seatInteraction.AddComponent<UAS_RideSeatInteractable>();
        GameObject seatPrompt = CreatePrompt(seatInteraction.transform, "E  Board Cart", new Vector3(0f, 1f, 0f));
        seat.ConfigureFeedback("Board Cart", "Open gate before boarding", seatPrompt,
            new[] { seatInteraction.GetComponent<Renderer>() });

        GameObject startButton = Primitive("Dashboard_Start_Button", PrimitiveType.Cylinder, cartRoot,
            new Vector3(-1.15f, 1.5f, 0f), new Vector3(0.25f, 0.12f, 0.25f),
            Materials["Cart_Gold"], true);
        startButton.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        UAS_StartRideInteractable start = startButton.AddComponent<UAS_StartRideInteractable>();
        start.ConfigureVisuals(startButton.transform, startButton.GetComponent<Renderer>());
        GameObject startPrompt = CreatePrompt(startButton.transform, "E  Start Ride", new Vector3(0f, 1f, 0f));
        start.ConfigureFeedback("Press E to Start", "Board cart and open gate first", startPrompt,
            new[] { startButton.GetComponent<Renderer>() });
        return new CartParts(vehicle, seat, start, seatAnchor);
    }

    private static UAS_DemoPlayerController BuildPlayer(Transform demo)
    {
        GameObject playerObject = new GameObject("Demo_Player");
        playerObject.transform.SetParent(demo, false);
        playerObject.transform.position = new Vector3(-20f, 1f, -13f);
        CharacterController controller = playerObject.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        UAS_DemoPlayerController player = playerObject.AddComponent<UAS_DemoPlayerController>();
        Transform pivot = Child(playerObject.transform, "View_Pivot");
        pivot.localPosition = new Vector3(0f, 1.6f, 0f);
        GameObject cameraObject = new GameObject("Demo_Camera");
        cameraObject.transform.SetParent(pivot, false);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.nearClipPlane = 0.05f;
        cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        cameraObject.AddComponent<AudioListener>();
        UAS_RayCastInteractorH interactor = playerObject.AddComponent<UAS_RayCastInteractorH>();
        player.Configure(pivot);
        interactor.Configure(camera, player, 5f);
        return player;
    }

    private static UAS_RideStatusUIH CreateWorldSpaceUi(Transform parent)
    {
        Transform boardingCanvas = CreateCanvas(parent, "Boarding_Panel",
            new Vector3(-18f, 2.7f, -12.8f), Quaternion.Euler(0f, 0f, 0f),
            new Vector2(600f, 220f));
        CreateText(boardingCanvas, "Boarding_Instructions",
            "Open Gate\nBoard Cart\nPress E to Start", 44f, TextAlignmentOptions.Center);

        Transform infoCanvas = CreateCanvas(parent, "Forest_Status_Panel",
            new Vector3(0f, 3.2f, -5.5f), Quaternion.identity,
            new Vector2(700f, 310f));
        CreateText(infoCanvas, "Forest_Title", "Forest Teddy Picnic", 52f,
            TextAlignmentOptions.Top);
        TMP_Text state = CreateText(infoCanvas, "Ride_State", "State: Ready", 38f,
            TextAlignmentOptions.Center);
        state.rectTransform.anchoredPosition = new Vector2(0f, -55f);
        TMP_Text stage = CreateText(infoCanvas, "Sequence_Stage", "Display: Ready", 34f,
            TextAlignmentOptions.Bottom);
        stage.rectTransform.anchoredPosition = new Vector2(0f, 20f);

        UAS_RideStatusUIH status = parent.gameObject.AddComponent<UAS_RideStatusUIH>();
        status.Configure(
            FindText(parent, "Boarding_Instructions"),
            FindText(parent, "Forest_Title"),
            state,
            stage,
            null,
            null);
        return status;
    }

    private static Transform CreateCanvas(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Vector2 size)
    {
        GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(parent, false);
        canvasObject.transform.position = position;
        canvasObject.transform.rotation = rotation;
        canvasObject.transform.localScale = Vector3.one * 0.01f;
        RectTransform rect = canvasObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(canvasObject.transform, false);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = new Color(0.08f, 0.035f, 0.015f, 0.88f);
        return canvasObject.transform;
    }

    private static TMP_Text CreateText(
        Transform canvas,
        string name,
        string value,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvas, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = new Color(1f, 0.88f, 0.63f);
        text.alignment = alignment;
        text.enableWordWrapping = true;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(24f, 18f);
        rect.offsetMax = new Vector2(-24f, -18f);
        return text;
    }

    private static GameObject CreatePrompt(Transform parent, string value, Vector3 offset)
    {
        Transform canvas = CreateCanvas(parent, "Interaction_Prompt", parent.position + offset,
            Quaternion.identity, new Vector2(340f, 80f));
        canvas.localPosition = offset;
        TMP_Text text = CreateText(canvas, "Prompt_Text", value, 32f, TextAlignmentOptions.Center);
        text.color = Color.white;
        canvas.gameObject.SetActive(false);
        return canvas.gameObject;
    }

    private static UAS_RideZoneTrigger CreateTrigger(
        string name,
        Transform parent,
        Vector3 localPosition,
        Vector3 size,
        UAS_RideZoneTrigger.ZoneMode mode)
    {
        GameObject triggerObject = new GameObject(name);
        triggerObject.transform.SetParent(parent, false);
        triggerObject.transform.localPosition = localPosition;
        BoxCollider box = triggerObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = size;
        UAS_RideZoneTrigger trigger = triggerObject.AddComponent<UAS_RideZoneTrigger>();
        trigger.Configure(mode, null, null, null);
        return trigger;
    }

    private static void CreateSun(Scene scene)
    {
        GameObject sunObject = new GameObject("Demo_Sun");
        SceneManager.MoveGameObjectToScene(sunObject, scene);
        sunObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        Light sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.88f, 0.68f);
        sun.intensity = 0.7f;
        RenderSettings.sun = sun;
        RenderSettings.ambientIntensity = 0.7f;
        RenderSettings.reflectionIntensity = 0.6f;
    }

    private static GameObject Primitive(
        string name,
        PrimitiveType type,
        Transform parent,
        Vector3 position,
        Vector3 scale,
        Material material,
        bool keepCollider)
    {
        GameObject item = GameObject.CreatePrimitive(type);
        item.name = name;
        item.transform.SetParent(parent, false);
        item.transform.localPosition = position;
        item.transform.localScale = scale;
        Renderer renderer = item.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        if (!keepCollider)
        {
            Object.DestroyImmediate(item.GetComponent<Collider>());
        }

        return item;
    }

    private static Transform Marker(Transform parent, string name, Vector3 worldPosition)
    {
        Transform marker = Child(parent, name);
        marker.position = worldPosition;
        return marker;
    }

    private static Transform Child(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static Transform AdoptOrCreateRoot(Scene scene, string name, Transform parent)
    {
        Transform result = FindRoot(scene, name);
        if (result == null)
        {
            result = Child(parent, name);
        }
        else
        {
            result.SetParent(parent, true);
        }

        return result;
    }

    private static Transform FindRoot(Scene scene, string name)
    {
        GameObject root = scene.GetRootGameObjects().FirstOrDefault(item => item.name == name);
        return root != null ? root.transform : null;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent == null)
        {
            return null;
        }

        return parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(item => item.name == name);
    }

    private static TMP_Text FindText(Transform parent, string name)
    {
        return parent.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(item => item.name == name);
    }

    private static void RemoveRoot(Scene scene, string name)
    {
        Transform root = FindRoot(scene, name);
        if (root != null)
        {
            Object.DestroyImmediate(root.gameObject);
        }
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private readonly struct CartParts
    {
        public CartParts(
            UAS_RideVehicleController vehicle,
            UAS_RideSeatInteractable seat,
            UAS_StartRideInteractable start,
            Transform seatAnchor)
        {
            Vehicle = vehicle;
            Seat = seat;
            Start = start;
            SeatAnchor = seatAnchor;
        }

        public UAS_RideVehicleController Vehicle { get; }
        public UAS_RideSeatInteractable Seat { get; }
        public UAS_StartRideInteractable Start { get; }
        public Transform SeatAnchor { get; }
    }
}
