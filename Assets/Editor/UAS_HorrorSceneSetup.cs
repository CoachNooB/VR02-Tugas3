using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class UAS_HorrorSceneSetup : EditorWindow
{
    [MenuItem("UAS/Setup Haunted House Scene")]
    public static void SetupHauntedHouse()
    {
        // ============================
        // 1. LIGHTING & ATMOSPHERE
        // ============================
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.01f, 0.01f, 0.03f, 1f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.02f, 0.03f, 0.02f, 1f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.06f;

        // Dim the Directional Light
        Light dirLight = FindOrCreateDirectionalLight();
        dirLight.color = new Color(0.08f, 0.10f, 0.20f, 1f);
        dirLight.intensity = 0.02f;
        dirLight.shadows = LightShadows.Soft;

        // ============================
        // 2. MATERIALS
        // ============================
        Material darkWallMat = FindOrCreateMaterial("Mat_DarkWall", new Color(0.06f, 0.06f, 0.07f));
        Material darkFloorMat = FindOrCreateMaterial("Mat_DarkFloor", new Color(0.04f, 0.04f, 0.05f));
        Material bloodMat = FindOrCreateMaterial("Mat_Blood", new Color(0.5f, 0.02f, 0.02f));
        Material woodMat = FindOrCreateMaterial("Mat_Wood", new Color(0.25f, 0.15f, 0.08f));
        Material greenGlowMat = FindOrCreateEmissiveMaterial("Mat_GreenGlow", new Color(0.1f, 0.8f, 0.1f), 3f);
        Material candleMat = FindOrCreateEmissiveMaterial("Mat_Candle", new Color(1f, 0.7f, 0.2f), 2f);
        Material ghostWhiteMat = FindOrCreateMaterial("Mat_GhostWhite", new Color(0.85f, 0.85f, 0.9f));
        Material coffinMat = FindOrCreateMaterial("Mat_Coffin", new Color(0.18f, 0.10f, 0.05f));

        // ============================
        // 3. ROOM STRUCTURE (smaller = scarier)
        // ============================
        float roomW = 12f, roomH = 4f, roomD = 14f;
        GetOrCreateWall("Floor",      new Vector3(0, -0.05f, 0),           new Vector3(roomW, 0.1f, roomD), darkFloorMat);
        GetOrCreateWall("Ceiling",    new Vector3(0, roomH + 0.05f, 0),    new Vector3(roomW, 0.1f, roomD), darkWallMat);
        GetOrCreateWall("Wall_Left",  new Vector3(-roomW/2, roomH/2, 0),   new Vector3(0.2f, roomH, roomD), darkWallMat);
        GetOrCreateWall("Wall_Right", new Vector3(roomW/2, roomH/2, 0),    new Vector3(0.2f, roomH, roomD), darkWallMat);
        GetOrCreateWall("Wall_Back",  new Vector3(0, roomH/2, -roomD/2),   new Vector3(roomW, roomH, 0.2f), darkWallMat);
        GetOrCreateWall("Wall_Front", new Vector3(0, roomH/2, roomD/2),    new Vector3(roomW, roomH, 0.2f), darkWallMat);

        // ============================
        // 4. ATMOSPHERIC LIGHTS
        // ============================
        CreatePointLight("Light_BloodRed",     new Vector3(0f, 3f, 0f),      new Color(0.8f, 0.1f, 0.05f), 2.5f, 10f);
        CreatePointLight("Light_GhostGreen",   new Vector3(-4f, 3f, 4f),     new Color(0.15f, 0.7f, 0.15f), 1.5f, 8f);
        CreatePointLight("Light_CandleWarm1",  new Vector3(4f, 2.2f, -5f),   new Color(1f, 0.6f, 0.2f), 1.2f, 5f);
        CreatePointLight("Light_CandleWarm2",  new Vector3(-4f, 2.2f, -3f),  new Color(1f, 0.5f, 0.15f), 1.0f, 4f);
        CreatePointLight("Light_WindowGreen",  new Vector3(5.7f, 3f, 2f),    new Color(0.2f, 0.9f, 0.2f), 2f, 6f);

        // ============================
        // 5. PLAYER
        // ============================
        CleanupMainCamera();
        GameObject playerObj = SetupPlayer();

        // ============================
        // 6. HORROR FURNITURE & PROPS
        // ============================

        // --- Coffin (Peti Mati) ---
        if (GameObject.Find("Coffin") == null)
        {
            GameObject coffin = new GameObject("Coffin");
            coffin.transform.position = new Vector3(-3f, 0f, 4f);
            coffin.transform.rotation = Quaternion.Euler(0, 15f, 0);
            // Base
            GameObject cBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cBase.name = "CoffinBase"; cBase.transform.SetParent(coffin.transform);
            cBase.transform.localPosition = new Vector3(0, 0.2f, 0);
            cBase.transform.localScale = new Vector3(0.8f, 0.4f, 2f);
            SetMaterial(cBase, coffinMat);
            // Lid (tilted open)
            GameObject cLid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cLid.name = "CoffinLid"; cLid.transform.SetParent(coffin.transform);
            cLid.transform.localPosition = new Vector3(-0.35f, 0.55f, 0);
            cLid.transform.localScale = new Vector3(0.05f, 0.8f, 2f);
            cLid.transform.localRotation = Quaternion.Euler(0, 0, 25f);
            SetMaterial(cLid, coffinMat);
        }

        // --- Broken Table (Meja Rusak) ---
        if (GameObject.Find("BrokenTable") == null)
        {
            GameObject table = new GameObject("BrokenTable");
            table.transform.position = new Vector3(3f, 0f, -4f);
            // Top
            GameObject tTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tTop.name = "TableTop"; tTop.transform.SetParent(table.transform);
            tTop.transform.localPosition = new Vector3(0, 0.7f, 0);
            tTop.transform.localScale = new Vector3(1.5f, 0.08f, 0.8f);
            tTop.transform.localRotation = Quaternion.Euler(0, 0, 3f); // slightly tilted
            SetMaterial(tTop, woodMat);
            // Legs
            CreateTableLeg(table.transform, "Leg1", new Vector3(-0.6f, 0.35f, 0.3f), woodMat);
            CreateTableLeg(table.transform, "Leg2", new Vector3(0.6f, 0.35f, 0.3f), woodMat);
            CreateTableLeg(table.transform, "Leg3", new Vector3(-0.6f, 0.35f, -0.3f), woodMat);
            // Missing 4th leg = broken
        }

        // --- Bookshelf (Rak Buku) ---
        if (GameObject.Find("Bookshelf") == null)
        {
            GameObject shelf = new GameObject("Bookshelf");
            shelf.transform.position = new Vector3(-5.5f, 0f, -2f);
            shelf.transform.rotation = Quaternion.Euler(0, 90, 0);
            // Back panel
            GameObject sBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sBack.name = "ShelfBack"; sBack.transform.SetParent(shelf.transform);
            sBack.transform.localPosition = new Vector3(0, 1.2f, -0.15f);
            sBack.transform.localScale = new Vector3(1.2f, 2.4f, 0.05f);
            SetMaterial(sBack, woodMat);
            // Shelves
            for (int i = 0; i < 4; i++)
            {
                GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
                s.name = $"Shelf_{i}"; s.transform.SetParent(shelf.transform);
                s.transform.localPosition = new Vector3(0, 0.05f + i * 0.6f, 0);
                s.transform.localScale = new Vector3(1.2f, 0.05f, 0.3f);
                SetMaterial(s, woodMat);
            }
        }

        // --- Candles (Lilin) ---
        CreateCandle("Candle_1", new Vector3(3f, 0.78f, -4f), candleMat);
        CreateCandle("Candle_2", new Vector3(-5.5f, 1.85f, -2f), candleMat);
        CreateCandle("Candle_3", new Vector3(0f, 0f, 5f), candleMat);

        // --- Glowing Window (Jendela Bercahaya Hijau) ---
        if (GameObject.Find("GlowingWindow") == null)
        {
            GameObject window = new GameObject("GlowingWindow");
            window.transform.position = new Vector3(5.85f, 2.8f, 2f);
            // Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "WindowFrame"; frame.transform.SetParent(window.transform);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(0.05f, 1.2f, 0.8f);
            SetMaterial(frame, woodMat);
            // Glass
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "WindowGlass"; glass.transform.SetParent(window.transform);
            glass.transform.localPosition = new Vector3(-0.02f, 0, 0);
            glass.transform.localScale = new Vector3(0.02f, 1f, 0.6f);
            SetMaterial(glass, greenGlowMat);
            // Cross bar
            GameObject crossH = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crossH.name = "CrossH"; crossH.transform.SetParent(window.transform);
            crossH.transform.localPosition = new Vector3(-0.01f, 0, 0);
            crossH.transform.localScale = new Vector3(0.06f, 0.05f, 0.6f);
            SetMaterial(crossH, woodMat);
            GameObject crossV = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crossV.name = "CrossV"; crossV.transform.SetParent(window.transform);
            crossV.transform.localPosition = new Vector3(-0.01f, 0, 0);
            crossV.transform.localScale = new Vector3(0.06f, 1f, 0.05f);
            SetMaterial(crossV, woodMat);
        }

        // --- Blood Stains (Noda Darah di lantai) ---
        CreateBloodStain("BloodStain_1", new Vector3(-1f, 0.01f, 2f), new Vector3(1.5f, 0.01f, 0.8f), bloodMat);
        CreateBloodStain("BloodStain_2", new Vector3(2f, 0.01f, -1f), new Vector3(0.6f, 0.01f, 1.2f), bloodMat);
        CreateBloodStain("BloodStain_3", new Vector3(-3f, 0.01f, 4.5f), new Vector3(0.8f, 0.01f, 0.5f), bloodMat);

        // ============================
        // 7. MONSTER PREFABS FROM PROJECT
        // ============================
        SpawnPrefab("Assets/Monsters/Prefabs/Ghost.prefab",       "Ghost_Corner",      new Vector3(-5f, 0.5f, 6f),  new Vector3(0, 140, 0), 1.5f);
        SpawnPrefab("Assets/Monsters/Prefabs/Ghost Skull.prefab", "GhostSkull_Shelf",   new Vector3(-5.5f, 1.3f, -2f), Vector3.zero, 1f);
        SpawnPrefab("Assets/Monsters/Prefabs/Demon.prefab",       "Demon_Dark",         new Vector3(4f, 0f, 5.5f),  new Vector3(0, -90, 0), 1.2f);
        SpawnPrefab("Assets/Monsters/Prefabs/Orc Skull.prefab",   "OrcSkull_Floor",     new Vector3(1f, 0.2f, 3f),  new Vector3(0, 45, 0), 0.8f);
        SpawnPrefab("Assets/Monsters/Prefabs/Blue Demon.prefab",  "BlueDemon_Coffin",   new Vector3(-3f, 0.5f, 4f), new Vector3(0, 180, 0), 1f);

        // Zombie near the door
        SpawnPrefab("Assets/Zombie/Prefabs/Zombie1.prefab",       "Zombie_Door",        new Vector3(0f, 0f, 6f),    new Vector3(0, 180, 0), 0.01f);
        SpawnPrefab("Assets/Zombie/Prefabs/Zombie2.prefab",       "Zombie_Corner",      new Vector3(5f, 0f, -5f),   new Vector3(0, -45, 0), 0.01f);

        // ============================
        // 8. WORLD SPACE CANVAS UI
        // ============================
        SetupWorldSpaceCanvas();

        // ============================
        // 9. HORROR MANAGER & INTERACTABLES
        // ============================
        SetupHorrorManager(playerObj);

        // ============================
        // DONE
        // ============================
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Haunted House Scene setup complete!");
        EditorUtility.DisplayDialog("UAS Horror Setup",
            "Haunted House Scene fully populated!\n\n" +
            "Added: Ghost, Demon, Zombie, Ghost Skull, Orc Skull,\n" +
            "Coffin, Broken Table, Bookshelf, Candles,\n" +
            "Glowing Window, Blood Stains, and atmospheric fog.\n\n" +
            "Press Ctrl+S to save, then Play to test!", "OK");
    }

    // ===== HELPER METHODS =====

    static Light FindOrCreateDirectionalLight()
    {
        foreach (var l in GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) return l;
        var obj = new GameObject("Directional Light");
        var light = obj.AddComponent<Light>();
        light.type = LightType.Directional;
        return light;
    }

    static Material FindOrCreateMaterial(string name, Color color)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:Material");
        if (guids.Length > 0) return AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[0]));

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material mat = new Material(shader);
        mat.color = color;
        string dir = "Assets/_Zones/Zone_Horor/Materials";
        if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/_Zones/Zone_Horor", "Materials");
        AssetDatabase.CreateAsset(mat, $"{dir}/{name}.mat");
        AssetDatabase.SaveAssets();
        return mat;
    }

    static Material FindOrCreateEmissiveMaterial(string name, Color color, float intensity)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:Material");
        if (guids.Length > 0) return AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[0]));

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material mat = new Material(shader);
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        string dir = "Assets/_Zones/Zone_Horor/Materials";
        if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/_Zones/Zone_Horor", "Materials");
        AssetDatabase.CreateAsset(mat, $"{dir}/{name}.mat");
        AssetDatabase.SaveAssets();
        return mat;
    }

    static GameObject GetOrCreateWall(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
        }
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.isStatic = true;
        SetMaterial(obj, mat);
        return obj;
    }

    static void SetMaterial(GameObject obj, Material mat)
    {
        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        if (mr != null && mat != null) mr.sharedMaterial = mat;
    }

    static void CreatePointLight(string name, Vector3 pos, Color color, float intensity, float range)
    {
        if (GameObject.Find(name) != null) return;
        var obj = new GameObject(name);
        var l = obj.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
        l.shadows = LightShadows.Soft;
        obj.transform.position = pos;
    }

    static void CreateTableLeg(Transform parent, string name, Vector3 localPos, Material mat)
    {
        var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leg.name = name; leg.transform.SetParent(parent);
        leg.transform.localPosition = localPos;
        leg.transform.localScale = new Vector3(0.08f, 0.7f, 0.08f);
        SetMaterial(leg, mat);
    }

    static void CreateCandle(string name, Vector3 pos, Material mat)
    {
        if (GameObject.Find(name) != null) return;
        var candle = new GameObject(name);
        candle.transform.position = pos;
        // Stick
        var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stick.name = "Stick"; stick.transform.SetParent(candle.transform);
        stick.transform.localPosition = new Vector3(0, 0.15f, 0);
        stick.transform.localScale = new Vector3(0.06f, 0.15f, 0.06f);
        SetMaterial(stick, mat);
        // Flame light
        var flameObj = new GameObject("FlameLight");
        flameObj.transform.SetParent(candle.transform);
        flameObj.transform.localPosition = new Vector3(0, 0.35f, 0);
        var fl = flameObj.AddComponent<Light>();
        fl.type = LightType.Point;
        fl.color = new Color(1f, 0.6f, 0.15f);
        fl.intensity = 0.8f;
        fl.range = 3f;
    }

    static void CreateBloodStain(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        if (GameObject.Find(name) != null) return;
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        SetMaterial(obj, mat);
    }

    static void SpawnPrefab(string path, string name, Vector3 pos, Vector3 rot, float scale)
    {
        if (GameObject.Find(name) != null) return;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogWarning($"Prefab not found: {path}"); return; }
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = name;
        obj.transform.position = pos;
        obj.transform.eulerAngles = rot;
        obj.transform.localScale = Vector3.one * scale;
    }

    static void CleanupMainCamera()
    {
        var cam = GameObject.Find("Main Camera");
        if (cam != null && cam.transform.parent == null)
        {
            Object.DestroyImmediate(cam);
        }
    }

    static GameObject SetupPlayer()
    {
        var existing = GameObject.Find("Player");
        if (existing != null && existing.GetComponent<UAS_SimpleFPSController>() == null)
        {
            Object.DestroyImmediate(existing);
            existing = null;
        }
        if (existing != null) return existing;

        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1f, -5f);
        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.3f;
        player.AddComponent<UAS_SimpleFPSController>();

        var camObj = new GameObject("PlayerCamera");
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        camObj.transform.localRotation = Quaternion.identity;
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.1f; cam.fieldOfView = 70f;
        camObj.AddComponent<AudioListener>();

        return player;
    }

    static void SetupWorldSpaceCanvas()
    {
        if (GameObject.Find("UAS_WorldSpaceCanvas") != null) return;
        var canvasObj = new GameObject("UAS_WorldSpaceCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rect = canvasObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(800, 400);
        rect.localScale = Vector3.one * 0.005f;
        rect.localPosition = new Vector3(0f, 2.5f, 6.85f);

        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(canvasObj.transform, false);
        var sTMP = statusObj.AddComponent<TextMeshProUGUI>();
        sTMP.text = "Explore the horror zone...";
        sTMP.fontSize = 45; sTMP.alignment = TextAlignmentOptions.Center;
        statusObj.GetComponent<RectTransform>().sizeDelta = new Vector2(750, 150);
        statusObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 80, 0);

        var promptObj = new GameObject("PromptText");
        promptObj.transform.SetParent(canvasObj.transform, false);
        var pTMP = promptObj.AddComponent<TextMeshProUGUI>();
        pTMP.text = ""; pTMP.color = Color.yellow;
        pTMP.fontSize = 35; pTMP.alignment = TextAlignmentOptions.Center;
        promptObj.GetComponent<RectTransform>().sizeDelta = new Vector2(750, 100);
        promptObj.GetComponent<RectTransform>().localPosition = new Vector3(0, -60, 0);
    }

    static void SetupHorrorManager(GameObject playerObj)
    {
        var manager = GameObject.Find("UAS_HorrorManager");
        if (manager == null) manager = new GameObject("UAS_HorrorManager");

        var sys = manager.GetComponent<UAS_HorrorSystem>();
        if (sys == null) sys = manager.AddComponent<UAS_HorrorSystem>();

        // Link UI
        foreach (var tmp in GameObject.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (tmp.gameObject.name == "StatusText") sys.statusText = tmp;
            if (tmp.gameObject.name == "PromptText") sys.promptText = tmp;
        }

        // Link camera
        if (playerObj != null)
        {
            var cam = playerObj.GetComponentInChildren<Camera>();
            if (cam != null) sys.cameraTransform = cam.transform;
        }

        // Interactable: Ghost
        var ghost = GameObject.Find("Ghost_Corner");
        if (ghost != null && ghost.GetComponent<UAS_HorrorInteractable>() == null)
        {
            var i = ghost.AddComponent<UAS_HorrorInteractable>();
            i.objectName = "Hantu Pojok"; i.requiresTriggerZone = false;
        }

        // Interactable: Coffin
        var coffin = GameObject.Find("Coffin");
        if (coffin != null && coffin.GetComponent<UAS_HorrorInteractable>() == null)
        {
            var ci = coffin.AddComponent<UAS_HorrorInteractable>();
            ci.objectName = "Peti Mati Misterius"; ci.requiresTriggerZone = true;
            // Add collider for raycast
            var col = coffin.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 0.6f, 2.2f);
            col.center = new Vector3(0, 0.3f, 0);
        }

        // Trigger zone near coffin
        if (GameObject.Find("TriggerZone_Coffin") == null)
        {
            var tz = new GameObject("TriggerZone_Coffin");
            tz.transform.position = new Vector3(-3f, 1f, 3f);
            var bc = tz.AddComponent<BoxCollider>();
            bc.isTrigger = true; bc.size = new Vector3(4f, 3f, 4f);
            var tzs = tz.AddComponent<UAS_HorrorTriggerZone>();
            tzs.zoneName = "Area Peti Mati"; tzs.horrorSystem = sys;
        }

        // Interactable: Demon
        var demon = GameObject.Find("Demon_Dark");
        if (demon != null && demon.GetComponent<UAS_HorrorInteractable>() == null)
        {
            var di = demon.AddComponent<UAS_HorrorInteractable>();
            di.objectName = "Iblis Kegelapan"; di.requiresTriggerZone = false;
        }

        // Pushable box
        if (GameObject.Find("PushableBox") == null)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "PushableBox";
            box.transform.position = new Vector3(2f, 0.5f, 2f);
            var rb = box.AddComponent<Rigidbody>();
            rb.mass = 2f;
            SetMaterial(box, FindOrCreateMaterial("Mat_Wood", new Color(0.25f, 0.15f, 0.08f)));
        }

        EditorUtility.SetDirty(manager);
    }
}
