using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class UAS_HorrorSceneSetup : EditorWindow
{
    [MenuItem("UAS/Setup Haunted House Scene")]
    public static void SetupHauntedHouse()
    {
        // 1. Setup Lighting & Atmospheric Environment
        // Set ambient lighting to flat color and dark blue/black
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.01f, 0.01f, 0.03f, 1f); // very dark
        
        // Find or create Directional Light
        Light[] lights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
        Light dirLight = null;
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                dirLight = l;
                break;
            }
        }

        if (dirLight != null)
        {
            dirLight.color = new Color(0.12f, 0.15f, 0.28f, 1f); // dim blue moonlight
            dirLight.intensity = 0.03f;
            Debug.Log("Directional light customized for Horror atmosphere.");
        }
        else
        {
            GameObject dlObj = new GameObject("Spooky Directional Light");
            dirLight = dlObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.12f, 0.15f, 0.28f, 1f);
            dirLight.intensity = 0.03f;
            Debug.Log("Spooky Directional Light created.");
        }

        // 2. Load and Apply Material
        Material darkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Zones/Zone_Horor/Materials/Mat_DarkWall.mat");
        if (darkMat == null)
        {
            // If the material wasn't found at that path, let's try to search the asset database
            string[] guids = AssetDatabase.FindAssets("Mat_DarkWall t:Material");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                darkMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            }
        }

        if (darkMat == null)
        {
            // Fallback: Create a dark material programmatically if still null
            darkMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (darkMat.shader == null) darkMat.shader = Shader.Find("Standard");
            darkMat.color = new Color(0.08f, 0.08f, 0.09f, 1f);
            AssetDatabase.CreateAsset(darkMat, "Assets/_Zones/Zone_Horor/Materials/Mat_DarkWall_Generated.mat");
            AssetDatabase.SaveAssets();
            Debug.Log("Created fallback dark material.");
        }

        // 3. Create Room Structure (Floor, Ceiling, Walls) if missing
        GetOrCreateObject("Floor", PrimitiveType.Cube, new Vector3(0, -0.05f, 0), new Vector3(30, 0.1f, 30), darkMat);
        GetOrCreateObject("Cealing", PrimitiveType.Cube, new Vector3(0, 5.05f, 0), new Vector3(30, 0.1f, 30), darkMat);
        GetOrCreateObject("Wall_Left", PrimitiveType.Cube, new Vector3(-15f, 2.5f, 0f), new Vector3(0.2f, 5f, 30f), darkMat);
        GetOrCreateObject("Wall_Right", PrimitiveType.Cube, new Vector3(15f, 2.5f, 0f), new Vector3(0.2f, 5f, 30f), darkMat);
        GetOrCreateObject("Wall_Back", PrimitiveType.Cube, new Vector3(0f, 2.5f, -15f), new Vector3(30f, 5f, 0.2f), darkMat);
        GetOrCreateObject("Wall_Front", PrimitiveType.Cube, new Vector3(0f, 2.5f, 15f), new Vector3(30f, 5f, 0.2f), darkMat);

        // 4. Create Spooky Point Lights in the Room
        CreateSpookyPointLight("PointLight_SpookyRed", new Vector3(0f, 3.5f, 0f), Color.red, 1.8f, 12f);
        CreateSpookyPointLight("PointLight_SpookyGreen", new Vector3(-6f, 3.5f, 4f), Color.green, 1.2f, 8f);

        // 5. Setup Player
        // Destroy default Main Camera at root to avoid conflict
        GameObject defaultCam = GameObject.Find("Main Camera");
        if (defaultCam != null && defaultCam.transform.parent == null)
        {
            DestroyImmediate(defaultCam);
            Debug.Log("Destroyed default root Main Camera to prevent conflict with Player Camera.");
        }

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj == null)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Easy FPS/Prefabs/Player.prefab");
            if (playerPrefab != null)
            {
                playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerObj.name = "Player";
                playerObj.transform.position = new Vector3(0f, 1f, -10f);
                Debug.Log("Instantiated Player prefab from Easy FPS.");
            }
            else
            {
                playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                playerObj.name = "Player";
                playerObj.transform.position = new Vector3(0f, 1f, -10f);
                playerObj.AddComponent<CharacterController>();
                Debug.Log("Created fallback Player capsule (no Easy FPS prefab found).");
            }
        }

        // 6. Create World Space Canvas UI if not present
        GameObject canvasObj = GameObject.Find("UAS_WorldSpaceCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("UAS_WorldSpaceCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Scale and Position Canvas
            RectTransform rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 400);
            rect.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            rect.localPosition = new Vector3(0f, 2.5f, 8.9f); // Float near the front wall
            rect.localRotation = Quaternion.Euler(0, 0, 0);

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Add Text Components
            GameObject statusTextObj = new GameObject("StatusText");
            statusTextObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI statusTMP = statusTextObj.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "Explore the horror zone...";
            statusTMP.fontSize = 45;
            statusTMP.alignment = TextAlignmentOptions.Center;
            RectTransform statusRect = statusTextObj.GetComponent<RectTransform>();
            statusRect.sizeDelta = new Vector2(750, 150);
            statusRect.localPosition = new Vector3(0, 80, 0);

            GameObject promptTextObj = new GameObject("PromptText");
            promptTextObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI promptTMP = promptTextObj.AddComponent<TextMeshProUGUI>();
            promptTMP.text = "[E] to Interact";
            promptTMP.color = Color.yellow;
            promptTMP.fontSize = 35;
            promptTMP.alignment = TextAlignmentOptions.Center;
            RectTransform promptRect = promptTextObj.GetComponent<RectTransform>();
            promptRect.sizeDelta = new Vector2(750, 100);
            promptRect.localPosition = new Vector3(0, -60, 0);

            Debug.Log("World Space Canvas created and configured.");
        }

        // 7. Setup Horror Manager Object
        GameObject manager = GameObject.Find("UAS_HorrorManager");
        if (manager == null)
        {
            manager = new GameObject("UAS_HorrorManager");
        }
        
        UAS_HorrorSystem horrorSystem = manager.GetComponent<UAS_HorrorSystem>();
        if (horrorSystem == null)
        {
            horrorSystem = manager.AddComponent<UAS_HorrorSystem>();
        }

        // Assign UI texts
        TextMeshProUGUI[] tmps = GameObject.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var tmp in tmps)
        {
            if (tmp.gameObject.name == "StatusText") horrorSystem.statusText = tmp;
            if (tmp.gameObject.name == "PromptText") horrorSystem.promptText = tmp;
        }

        // Find Player and assign camera to manager if available
        if (playerObj != null)
        {
            Camera cam = playerObj.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                horrorSystem.cameraTransform = cam.transform;
            }
        }

        // 8. Setup some Interactables & Trigger Zones in the room
        SetupSpookyInteractablesAndTriggers(horrorSystem);

        // Mark scene dirty so it prompts to save
        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log("Haunted House Scene setup complete! Press Save and hit Play.");
        EditorUtility.DisplayDialog("UAS Horror Setup", "Haunted House Scene setup completed successfully!\n\nAll walls, ceiling, floor, lights, player, and World Space UI have been created/configured.", "OK");
    }

    private static GameObject GetOrCreateObject(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.position = position;
            obj.transform.localScale = scale;
            Debug.Log($"Created primitive object: {name}");
        }
        
        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        if (mr != null && mat != null)
        {
            mr.sharedMaterial = mat;
        }
        return obj;
    }

    private static void CreateSpookyPointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObj = GameObject.Find(name);
        if (lightObj == null)
        {
            lightObj = new GameObject(name);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            lightObj.transform.position = position;
            Debug.Log($"Spooky point light '{name}' created at {position}");
        }
    }

    private static void SetupSpookyInteractablesAndTriggers(UAS_HorrorSystem horrorSystem)
    {
        // Spooky Doll (Interactable)
        GameObject doll = GameObject.Find("SpookyDoll");
        if (doll == null)
        {
            doll = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            doll.name = "SpookyDoll";
            doll.transform.position = new Vector3(3f, 1f, 5f);
            doll.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
            
            // Add a spooky red material to the doll to stand out
            MeshRenderer mr = doll.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Material dollMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (dollMat.shader == null) dollMat.shader = Shader.Find("Standard");
                dollMat.color = new Color(0.6f, 0.1f, 0.1f, 1f);
                mr.sharedMaterial = dollMat;
            }
        }

        UAS_HorrorInteractable interactable = doll.GetComponent<UAS_HorrorInteractable>();
        if (interactable == null)
        {
            interactable = doll.AddComponent<UAS_HorrorInteractable>();
            interactable.objectName = "Boneka Hantu";
            interactable.requiresTriggerZone = true; // Bonus challenge requirement!
        }

        // Trigger Zone for Doll
        GameObject triggerObj = GameObject.Find("TriggerZone_Doll");
        if (triggerObj == null)
        {
            triggerObj = new GameObject("TriggerZone_Doll");
            triggerObj.transform.position = new Vector3(3f, 1f, 2f);
            BoxCollider col = triggerObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 2f, 3f);
            
            UAS_HorrorTriggerZone triggerZone = triggerObj.AddComponent<UAS_HorrorTriggerZone>();
            triggerZone.zoneName = "Area Sekitar Boneka";
            triggerZone.horrorSystem = horrorSystem;
        }
        
        // Spooky Box (Rigidbody Pushable physics object)
        GameObject pushBox = GameObject.Find("PushableBox");
        if (pushBox == null)
        {
            pushBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pushBox.name = "PushableBox";
            pushBox.transform.position = new Vector3(-3f, 0.5f, 3f);
            Rigidbody rb = pushBox.AddComponent<Rigidbody>();
            rb.mass = 2f;
            
            MeshRenderer mr = pushBox.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Material boxMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (boxMat.shader == null) boxMat.shader = Shader.Find("Standard");
                boxMat.color = new Color(0.3f, 0.2f, 0.15f, 1f); // Wood color
                mr.sharedMaterial = boxMat;
            }
        }
    }
}
