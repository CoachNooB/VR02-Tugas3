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
        // 1. LIGHTING & ATMOSPHERE (Spooky Night)
        // ============================
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.005f, 0.005f, 0.015f, 1f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.01f, 0.03f, 0.015f, 1f); // Dark eerie green fog
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.05f;

        // Dim the Directional Light to resemble moonlight
        Light dirLight = FindOrCreateDirectionalLight();
        dirLight.color = new Color(0.05f, 0.06f, 0.15f, 1f);
        dirLight.intensity = 0.005f;
        dirLight.shadows = LightShadows.Soft;

        // ============================
        // 2. CLEANUP OLD SIMPLE ROOM STRUCTURE
        // ============================
        string[] oldObjects = { 
            "Floor", "Ceiling", "Wall_Left", "Wall_Right", "Wall_Back", "Wall_Front", "Haunted_House", 
            "Spooky_Pathway", "Ghost_White", "Ghost_Bloody", "Ghost_Black", "Hantu_Penjaga", "Arwah_Penasaran" 
        };
        foreach (string name in oldObjects)
        {
            GameObject oldObj = GameObject.Find(name);
            if (oldObj != null) Object.DestroyImmediate(oldObj);
        }

        // ============================
        // 3. SPAWN SPOOKY TOWN MAP (Sci_Fi_Island)
        // ============================
        string islandPrefabPath = "Assets/Mnostva_Art/Flying_Sci_Fi_Island_city/Prefabs/island/Sci_Fi_Island.prefab";
        GameObject islandObj = GameObject.Find("Sci_Fi_Island");
        if (islandObj == null)
        {
            var islandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(islandPrefabPath);
            if (islandPrefab != null)
            {
                islandObj = (GameObject)PrefabUtility.InstantiatePrefab(islandPrefab);
                islandObj.name = "Sci_Fi_Island";
                islandObj.transform.position = Vector3.zero;
                islandObj.transform.rotation = Quaternion.identity;
                islandObj.transform.localScale = Vector3.one;
            }
            else
            {
                Debug.LogWarning($"Island Prefab not found at path: {islandPrefabPath}");
            }
        }

        // ============================
        // 4. MATERIALS & PALETTE
        // ============================
        Material woodMat = FindOrCreateMaterial("Mat_Wood", new Color(0.20f, 0.12f, 0.06f)); // Dark Wood
        Material roofMat = FindOrCreateMaterial("Mat_Roof", new Color(0.12f, 0.12f, 0.14f)); // Dark Slate Grey
        Material foundationMat = FindOrCreateMaterial("Mat_Stone", new Color(0.35f, 0.35f, 0.38f)); // Dark Stone
        Material bloodMat = FindOrCreateMaterial("Mat_Blood", new Color(0.5f, 0.02f, 0.02f));
        Material greenGlowMat = FindOrCreateEmissiveMaterial("Mat_GreenGlow", new Color(0.1f, 0.8f, 0.1f), 3f);
        Material candleMat = FindOrCreateEmissiveMaterial("Mat_Candle", new Color(1f, 0.7f, 0.2f), 2.5f);
        Material scaryPurpleGlowMat = FindOrCreateEmissiveMaterial("Mat_PurpleGlow", new Color(0.6f, 0.1f, 0.8f), 3f);

        // ============================
        // 5. PLAYER PLACEMENT
        // ============================
        CleanupMainCamera();
        GameObject playerObj = SetupPlayer();
        if (playerObj != null)
        {
            // Position player on street facing the Haunted House
            playerObj.transform.position = new Vector3(0f, 1.5f, 0f);
            playerObj.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }

        // ============================
        // 5b. PATHWAY GENERATION (Jalan Setapak)
        // ============================
        GameObject pathwayObj = new GameObject("Spooky_Pathway");
        pathwayObj.transform.position = Vector3.zero;
        for (int zVal = -3; zVal <= 21; zVal += 2)
        {
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = $"StoneTile_{zVal}";
            tile.transform.SetParent(pathwayObj.transform);
            float xOffset = Mathf.Sin(zVal * 0.8f) * 0.12f;
            tile.transform.position = new Vector3(xOffset, 0.55f, zVal);
            tile.transform.localScale = new Vector3(3.5f, 0.15f, 1.8f);
            tile.transform.rotation = Quaternion.Euler(0f, Mathf.Sin(zVal) * 4f, 0f);
            SetMaterial(tile, foundationMat);
        }

        // ============================
        // 6. BUILD STYLIZED HAUNTED HOUSE (Hollow Room + Gothic Architecture)
        // ============================
        // Positioned down the street
        Vector3 housePosition = new Vector3(0f, 0.6f, 25f); 
        GameObject hauntedHouseObj = CreateStylizedSpookyHouse(housePosition, woodMat, roofMat, candleMat, foundationMat);

        // --- Coffin (Peti Mati) ---
        // Spawned inside the Haunted House
        GameObject coffinObj = GameObject.Find("Coffin");
        if (coffinObj == null)
        {
            coffinObj = new GameObject("Coffin");
            coffinObj.transform.position = new Vector3(-2f, 0.7f, 27f);
            coffinObj.transform.rotation = Quaternion.Euler(0, 15f, 0);
            
            // Base
            GameObject cBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cBase.name = "CoffinBase"; cBase.transform.SetParent(coffinObj.transform);
            cBase.transform.localPosition = new Vector3(0, 0.2f, 0);
            cBase.transform.localScale = new Vector3(0.8f, 0.4f, 2f);
            SetMaterial(cBase, woodMat);
            
            // Lid (tilted open)
            GameObject cLid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cLid.name = "CoffinLid"; cLid.transform.SetParent(coffinObj.transform);
            cLid.transform.localPosition = new Vector3(-0.35f, 0.55f, 0);
            cLid.transform.localScale = new Vector3(0.05f, 0.8f, 2f);
            cLid.transform.localRotation = Quaternion.Euler(0, 0, 25f);
            SetMaterial(cLid, woodMat);
        }

        // --- Broken Table (Meja Rusak) ---
        if (GameObject.Find("BrokenTable") == null)
        {
            GameObject table = new GameObject("BrokenTable");
            table.transform.position = new Vector3(2.2f, 0.7f, 24.5f);
            
            // Top
            GameObject tTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tTop.name = "TableTop"; tTop.transform.SetParent(table.transform);
            tTop.transform.localPosition = new Vector3(0, 0.7f, 0);
            tTop.transform.localScale = new Vector3(1.4f, 0.08f, 0.8f);
            tTop.transform.localRotation = Quaternion.Euler(0, 0, 3f);
            SetMaterial(tTop, woodMat);
            
            // Legs
            CreateTableLeg(table.transform, "Leg1", new Vector3(-0.55f, 0.35f, 0.3f), woodMat);
            CreateTableLeg(table.transform, "Leg2", new Vector3(0.55f, 0.35f, 0.3f), woodMat);
            CreateTableLeg(table.transform, "Leg3", new Vector3(-0.55f, 0.35f, -0.3f), woodMat);
        }

        // --- Bookshelf (Rak Buku) ---
        if (GameObject.Find("Bookshelf") == null)
        {
            GameObject shelf = new GameObject("Bookshelf");
            shelf.transform.position = new Vector3(-3.2f, 0.7f, 23.5f);
            shelf.transform.rotation = Quaternion.Euler(0, 90, 0);
            
            GameObject sBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sBack.name = "ShelfBack"; sBack.transform.SetParent(shelf.transform);
            sBack.transform.localPosition = new Vector3(0, 1.2f, -0.15f);
            sBack.transform.localScale = new Vector3(1.2f, 2.4f, 0.05f);
            SetMaterial(sBack, woodMat);
            
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
        CreateCandle("Candle_1", new Vector3(2.2f, 1.48f, 24.5f), candleMat);
        CreateCandle("Candle_2", new Vector3(-3.2f, 2.55f, 23.5f), candleMat);
        CreateCandle("Candle_3", new Vector3(0f, 0.7f, 28f), candleMat);

        // --- Glowing Window (Jendela Bercahaya Hijau di Dinding Belakang) ---
        if (GameObject.Find("GlowingWindow") == null)
        {
            GameObject window = new GameObject("GlowingWindow");
            window.transform.position = new Vector3(0f, 3.1f, 28.95f);
            window.transform.rotation = Quaternion.identity;
            
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "WindowFrame"; frame.transform.SetParent(window.transform);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(1.2f, 1.2f, 0.05f);
            SetMaterial(frame, woodMat);
            
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "WindowGlass"; glass.transform.SetParent(window.transform);
            glass.transform.localPosition = new Vector3(0, 0, -0.02f);
            glass.transform.localScale = new Vector3(1f, 1f, 0.02f);
            SetMaterial(glass, greenGlowMat);
        }

        // --- Blood Stains (Noda Darah) ---
        CreateBloodStain("BloodStain_1", new Vector3(0f, 0.71f, 21f), new Vector3(1.5f, 0.01f, 0.8f), bloodMat);
        CreateBloodStain("BloodStain_2", new Vector3(1.5f, 0.71f, 25f), new Vector3(0.8f, 0.01f, 1.2f), bloodMat);
        CreateBloodStain("BloodStain_3", new Vector3(-2f, 0.71f, 27f), new Vector3(1.0f, 0.01f, 0.6f), bloodMat);

        // --- Hanging Chains & Spiderwebs ---
        Material chainMat = FindOrCreateMaterial("Mat_Chain", new Color(0.3f, 0.3f, 0.32f));
        CreateHangingChain("Chain_1", new Vector3(-1f, 5.0f, 26f), 2.2f, chainMat);
        CreateHangingChain("Chain_2", new Vector3(2f, 5.0f, 27f), 1.8f, chainMat);
        
        Material webMat = FindOrCreateMaterial("Mat_SpiderWeb", new Color(0.9f, 0.9f, 0.9f, 0.3f));
        CreateSpiderWeb("Web_1", new Vector3(-3.2f, 4.8f, 21.8f), webMat);
        CreateSpiderWeb("Web_2", new Vector3(3.2f, 4.8f, 27.8f), webMat);

        // --- Ritual Circle (Lingkaran Ritual di Rumah Hantu) ---
        if (GameObject.Find("RitualCircle") == null)
        {
            Material ritualMat = FindOrCreateEmissiveMaterial("Mat_Ritual", new Color(0.8f, 0.1f, 0.1f), 1.5f);
            var circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            circle.name = "RitualCircle";
            circle.transform.position = new Vector3(0f, 0.71f, 25.5f);
            circle.transform.localScale = new Vector3(2.5f, 0.005f, 2.5f);
            SetMaterial(circle, ritualMat);
            
            // Candles around circle
            for (int i = 0; i < 5; i++)
            {
                float angle = i * (360f / 5f) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 1.3f, 0.7f, 25.5f + Mathf.Sin(angle) * 1.3f);
                CreateCandle($"RitualCandle_{i}", pos, candleMat);
            }
        }

        // ============================
        // 7. EERIE TOWN LIGHTING & DECORATIONS
        // ============================
        CreatePointLight("StreetLightSpooky_1", new Vector3(4f, 4f, 6f), new Color(0.8f, 0.2f, 0.1f), 2.5f, 12f); // Creepy Orange
        CreatePointLight("StreetLightSpooky_2", new Vector3(-4f, 4f, 15f), new Color(0.5f, 0.1f, 0.8f), 2.5f, 12f); // Spooky Purple
        CreatePointLight("StreetLightSpooky_3", new Vector3(4f, 4f, 20f), new Color(0.1f, 0.8f, 0.2f), 3.0f, 15f); // Toxic Green at Haunted House Gate

        // Ambient Spooky lights inside/outside Haunted House
        CreatePointLight("Light_RitualRed", new Vector3(0f, 1.3f, 25.5f), new Color(0.9f, 0.05f, 0.05f), 1.8f, 8f);
        CreatePointLight("Light_SpookyGreen_Int", new Vector3(0f, 3.1f, 28.5f), new Color(0.1f, 0.9f, 0.1f), 1.5f, 6f);
        CreatePointLight("Light_Purple_Upper", new Vector3(0f, 4.5f, 25f), new Color(0.6f, 0.1f, 0.8f), 2.0f, 10f);

        // ============================
        // 8. INTERACTIVE GHOSTS & MONSTERS
        // ============================
        string ghostPrefabPath = "Assets/Monsters/Prefabs/Ghost.prefab";
        
        // --- 1. HANTU PENJAGA (Spawns outside, has Dialogue and Vanishes) ---
        GameObject guardGhost = GameObject.Find("Hantu_Penjaga");
        if (guardGhost == null)
        {
            var ghostPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ghostPrefabPath);
            if (ghostPrefab != null)
            {
                guardGhost = (GameObject)PrefabUtility.InstantiatePrefab(ghostPrefab);
                guardGhost.name = "Hantu_Penjaga";
                guardGhost.transform.position = new Vector3(0f, 1.0f, 17f); // In front of house gate porch
                guardGhost.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Face the approaching player
                guardGhost.transform.localScale = Vector3.one * 1.5f;

                // Configure interaction
                var interactable = guardGhost.AddComponent<UAS_HorrorInteractable>();
                interactable.objectName = "Hantu Penjaga Gerbang";
                interactable.isGhostNPC = true;
                interactable.dialogueLines = new string[] {
                    "Hantu Penjaga: Hentikan langkahmu, pengembara fana...",
                    "Hantu Penjaga: Rumah gothic di belakangku dipenuhi aura iblis jahat.",
                    "Hantu Penjaga: Jika kamu berani memasukinya, carilah jalan untuk membebaskan mereka.",
                    "Hantu Penjaga: Ambillah senjata laci di bunker jika kamu butuh bertahan hidup!",
                    "Hantu Penjaga: Sekarang, jalan masuk telah terbuka. Masuklah jika kamu bernyali..."
                };
                interactable.vanishAfterInteract = true;
                interactable.vanishDelay = 0.5f;
                interactable.floatAndRotate = true;
                interactable.floatSpeed = 2f;
                interactable.floatHeight = 0.2f;

                // Add box collider for interaction
                var bc = guardGhost.AddComponent<BoxCollider>();
                bc.center = new Vector3(0, 0.5f, 0);
                bc.size = new Vector3(1.2f, 1.8f, 1.2f);
            }
        }

        // --- 2. THREE SHEET GHOSTS (Inside Haunted House: Clean, Bloody, Black with Red Eyes) ---
        string[] ghostNames = { "Ghost_White", "Ghost_Bloody", "Ghost_Black" };
        Vector3[] ghostPositions = {
            new Vector3(-2f, 1.2f, 23.5f), // Left side inside
            new Vector3(2f, 1.2f, 25.5f),  // Right side inside
            new Vector3(0f, 1.2f, 27.5f)   // Center back inside
        };
        string[][] ghostDialogues = {
            new string[] {
                "Arwah Putih: Selamat datang di rumah penderitaan ini...",
                "Arwah Putih: Jiwa kami terikat di sini oleh kutukan ritual jahat.",
                "Arwah Putih: Hanya dengan mengungkap rahasia peti mati di sudut ruangan kamu bisa keluar dari sini.",
                "Arwah Putih: Berhati-hatilah dengan arwah hitam di belakang..."
            },
            new string[] {
                "Arwah Berdarah: Darah... mengalir di mana-mana...",
                "Arwah Berdarah: Kekuatan kegelapan di altar ini merobek jiwa kami.",
                "Arwah Berdarah: Carilah petunjuk di dekat altar ritual untuk melemahkan kutukan."
            },
            new string[] {
                "Arwah Hitam: Beraninya makhluk fana menginjakkan kaki di domain kami!",
                "Arwah Hitam: Kegelapan akan melahap jiwamu jika kamu tidak segera pergi!",
                "Arwah Hitam: Sentuh peti mati kuno itu jika kamu berani menantang kutukan ini!"
            }
        };

        for (int i = 0; i < 3; i++)
        {
            string gName = ghostNames[i];
            GameObject ghostObj = GameObject.Find(gName);
            if (ghostObj == null)
            {
                var ghostPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ghostPrefabPath);
                if (ghostPrefab != null)
                {
                    ghostObj = (GameObject)PrefabUtility.InstantiatePrefab(ghostPrefab);
                    ghostObj.name = gName;
                    ghostObj.transform.position = ghostPositions[i];
                    ghostObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    ghostObj.transform.localScale = Vector3.one * 1.5f;

                    // Apply custom materials
                    var meshRenderer = ghostObj.GetComponentInChildren<Renderer>();
                    if (meshRenderer != null)
                    {
                        if (i == 0) // White Ghost
                        {
                            var matWhite = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Zones/Zone_Horor/Materials/Mat_GhostWhite.mat");
                            if (matWhite == null) matWhite = FindOrCreateMaterial("Mat_GhostWhite", new Color(0.95f, 0.95f, 0.95f));
                            meshRenderer.sharedMaterial = matWhite;
                        }
                        else if (i == 1) // Bloody Ghost
                        {
                            var matBloody = FindOrCreateMaterial("Mat_GhostBloody", new Color(0.9f, 0.6f, 0.6f));
                            meshRenderer.sharedMaterial = matBloody;
                        }
                        else if (i == 2) // Black Ghost with red eyes
                        {
                            var matBlack = FindOrCreateMaterial("Mat_GhostBlack", new Color(0.05f, 0.05f, 0.05f));
                            meshRenderer.sharedMaterial = matBlack;

                            // Spawn two tiny glowing red eyes in front of the face
                            GameObject eyeL = new GameObject("Eye_L");
                            eyeL.transform.SetParent(ghostObj.transform);
                            eyeL.transform.localPosition = new Vector3(-0.15f, 0.85f, 0.25f);
                            var lightL = eyeL.AddComponent<Light>();
                            lightL.type = LightType.Point;
                            lightL.color = Color.red;
                            lightL.intensity = 2f;
                            lightL.range = 0.5f;

                            GameObject eyeR = new GameObject("Eye_R");
                            eyeR.transform.SetParent(ghostObj.transform);
                            eyeR.transform.localPosition = new Vector3(0.15f, 0.85f, 0.25f);
                            var lightR = eyeR.AddComponent<Light>();
                            lightR.type = LightType.Point;
                            lightR.color = Color.red;
                            lightR.intensity = 2f;
                            lightR.range = 0.5f;
                        }
                    }

                    // Configure interaction
                    var interactable = ghostObj.AddComponent<UAS_HorrorInteractable>();
                    interactable.objectName = gName == "Ghost_White" ? "Arwah Putih" : (gName == "Ghost_Bloody" ? "Arwah Berdarah" : "Arwah Hitam");
                    interactable.isGhostNPC = true;
                    interactable.dialogueLines = ghostDialogues[i];
                    interactable.vanishAfterInteract = false;
                    interactable.floatAndRotate = true;
                    interactable.floatSpeed = 1.2f + (i * 0.3f);
                    interactable.floatHeight = 0.2f;

                    // Add box collider for interaction
                    var bc = ghostObj.AddComponent<BoxCollider>();
                    bc.center = new Vector3(0, 0.5f, 0);
                    bc.size = new Vector3(1.2f, 1.8f, 1.2f);
                }
            }
        }

        // --- 3. WANDERING ZOMBIES & EXTRA MONSTERS ---
        SpawnPrefab("Assets/Monsters/Prefabs/Ghost Skull.prefab", "SpookySkull_1", new Vector3(-2f, 0.8f, 13f), new Vector3(0, 45, 0), 1f);
        SpawnPrefab("Assets/Monsters/Prefabs/Ghost Skull.prefab", "SpookySkull_2", new Vector3(3.2f, 1.5f, 24f), new Vector3(0, -90, 0), 0.8f);
        SpawnPrefab("Assets/Monsters/Prefabs/Orc Skull.prefab", "SpookySkull_3", new Vector3(-1.8f, 1.1f, 26f), new Vector3(15, 0, 0), 0.8f);
        
        SpawnPrefab("Assets/Monsters/Prefabs/Demon.prefab", "LurkingDemon", new Vector3(-6f, 0.6f, 18f), new Vector3(0, 90, 0), 1.2f);
        SpawnPrefab("Assets/Monsters/Prefabs/Demon Flying.prefab", "RoofDemon", new Vector3(3f, 6.5f, 22f), new Vector3(0, 200, 0), 1.3f);
        
        SpawnPrefab("Assets/Zombie/Prefabs/Zombie1.prefab", "StreetZombie_1", new Vector3(5f, 0.6f, 15f), new Vector3(0, -90, 0), 0.01f);
        SpawnPrefab("Assets/Zombie/Prefabs/Zombie2.prefab", "StreetZombie_2", new Vector3(-5f, 0.6f, 8f), new Vector3(0, 90, 0), 0.01f);
        SpawnPrefab("Assets/Zombie/Prefabs/Zombie3.prefab", "InteriorZombie_3", new Vector3(-2.8f, 0.7f, 24f), new Vector3(0, 120, 0), 0.01f);

        // ============================
        // 9. WORLD SPACE UI CANVAS SETUP
        // ============================
        SetupWorldSpaceCanvas();

        // ============================
        // 10. SYSTEM MANAGER LINKING & TRIGGER ZONES
        // ============================
        SetupHorrorManager(playerObj);

        // ============================
        // DONE
        // ============================
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Haunted Spooky Town Scene setup complete!");
        EditorUtility.DisplayDialog("UAS Spooky Town Setup",
            "Stylized Haunted House & Spooky Town successfully constructed!\n\n" +
            "Created custom wooden gothic house layout with pointed gables and spires,\n" +
            "Spooky island map, interactive dialog ghosts, candles, and zombies.\n\n" +
            "Press Ctrl+S to save, then Play to test!", "OK");
    }

    // ===== GOTHIC HOUSE BUILDER METHOD =====
    static GameObject CreateStylizedSpookyHouse(Vector3 position, Material woodMat, Material roofMat, Material windowMat, Material foundationMat)
    {
        GameObject house = new GameObject("Haunted_House");
        house.transform.position = position;
        house.transform.rotation = Quaternion.identity;

        // 1. Foundation (Stone base)
        GameObject fd = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fd.name = "Foundation"; fd.transform.SetParent(house.transform);
        fd.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        fd.transform.localScale = new Vector3(8.2f, 0.1f, 8.2f);
        SetMaterial(fd, foundationMat);

        // 2. Hollow Room Walls (Allows player to walk inside)
        // Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor"; floor.transform.SetParent(house.transform);
        floor.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        floor.transform.localScale = new Vector3(8f, 0.05f, 8f);
        SetMaterial(floor, woodMat);

        // Ceiling
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling"; ceiling.transform.SetParent(house.transform);
        ceiling.transform.localPosition = new Vector3(0f, 4.45f, 0f);
        ceiling.transform.localScale = new Vector3(8f, 0.1f, 8f);
        SetMaterial(ceiling, woodMat);

        // Wall Left
        GameObject wallL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallL.name = "Wall_Left"; wallL.transform.SetParent(house.transform);
        wallL.transform.localPosition = new Vector3(-4f, 2.25f, 0f);
        wallL.transform.localScale = new Vector3(0.1f, 4.3f, 8f);
        SetMaterial(wallL, woodMat);

        // Wall Right
        GameObject wallR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallR.name = "Wall_Right"; wallR.transform.SetParent(house.transform);
        wallR.transform.localPosition = new Vector3(4f, 2.25f, 0f);
        wallR.transform.localScale = new Vector3(0.1f, 4.3f, 8f);
        SetMaterial(wallR, woodMat);

        // Wall Back
        GameObject wallB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallB.name = "Wall_Back"; wallB.transform.SetParent(house.transform);
        wallB.transform.localPosition = new Vector3(0f, 2.25f, 4f);
        wallB.transform.localScale = new Vector3(8f, 4.3f, 0.1f);
        SetMaterial(wallB, woodMat);

        // Wall Front (Constructed in pieces to create a doorway opening at the center)
        GameObject wallFL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallFL.name = "Wall_Front_Left"; wallFL.transform.SetParent(house.transform);
        wallFL.transform.localPosition = new Vector3(-2.5f, 2.25f, -4f);
        wallFL.transform.localScale = new Vector3(3f, 4.3f, 0.1f);
        SetMaterial(wallFL, woodMat);

        GameObject wallFR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallFR.name = "Wall_Front_Right"; wallFR.transform.SetParent(house.transform);
        wallFR.transform.localPosition = new Vector3(2.5f, 2.25f, -4f);
        wallFR.transform.localScale = new Vector3(3f, 4.3f, 0.1f);
        SetMaterial(wallFR, woodMat);

        GameObject wallFT = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallFT.name = "Wall_Front_Top"; wallFT.transform.SetParent(house.transform);
        wallFT.transform.localPosition = new Vector3(0f, 3.65f, -4f);
        wallFT.transform.localScale = new Vector3(2f, 1.5f, 0.1f);
        SetMaterial(wallFT, woodMat);

        // 3. A-Frame Gothic Roof (Large Left and Right angled slabs)
        GameObject roofL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofL.name = "Gothic_Roof_Left"; roofL.transform.SetParent(house.transform);
        roofL.transform.localPosition = new Vector3(-2.8f, 6.2f, 0f);
        roofL.transform.localScale = new Vector3(0.2f, 6.4f, 8.8f);
        roofL.transform.localRotation = Quaternion.Euler(0f, 0f, 32f);
        SetMaterial(roofL, roofMat);

        GameObject roofR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofR.name = "Gothic_Roof_Right"; roofR.transform.SetParent(house.transform);
        roofR.transform.localPosition = new Vector3(2.8f, 6.2f, 0f);
        roofR.transform.localScale = new Vector3(0.2f, 6.4f, 8.8f);
        roofR.transform.localRotation = Quaternion.Euler(0f, 0f, -32f);
        SetMaterial(roofR, roofMat);

        // 4. Side Gothic High Tower (spire tower on the right)
        GameObject bodyTower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bodyTower.name = "High_Tower_Body"; bodyTower.transform.SetParent(house.transform);
        bodyTower.transform.localPosition = new Vector3(4.1f, 4.25f, 0f);
        bodyTower.transform.localScale = new Vector3(2.0f, 8.3f, 2.0f);
        SetMaterial(bodyTower, woodMat);

        // Tower Roof Spire Base
        GameObject tRoofBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tRoofBase.name = "Tower_Roof_Base"; tRoofBase.transform.SetParent(house.transform);
        tRoofBase.transform.localPosition = new Vector3(4.1f, 8.45f, 0f);
        tRoofBase.transform.localScale = new Vector3(2.2f, 0.2f, 2.2f);
        SetMaterial(tRoofBase, roofMat);

        // Stepped Spire segments to form a pointed tower roof programmatically
        for (int i = 0; i < 5; i++)
        {
            float stepScale = 1.8f - i * 0.35f;
            GameObject spireStep = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spireStep.name = $"Spire_Step_{i}"; spireStep.transform.SetParent(house.transform);
            spireStep.transform.localPosition = new Vector3(4.1f, 8.75f + i * 0.5f, 0f);
            spireStep.transform.localScale = new Vector3(stepScale, 0.5f, stepScale);
            spireStep.transform.localRotation = Quaternion.Euler(0f, i * 15f, 0f);
            SetMaterial(spireStep, roofMat);
        }
        // Metal tip
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tip.name = "Spire_Metal_Tip"; tip.transform.SetParent(house.transform);
        tip.transform.localPosition = new Vector3(4.1f, 11.25f, 0f);
        tip.transform.localScale = new Vector3(0.12f, 0.6f, 0.12f);
        SetMaterial(tip, roofMat);

        // 5. Front Spooky Porch
        // Porch Deck
        GameObject porchDeck = GameObject.CreatePrimitive(PrimitiveType.Cube);
        porchDeck.name = "Porch_Deck"; porchDeck.transform.SetParent(house.transform);
        porchDeck.transform.localPosition = new Vector3(0f, 0.1f, -4.9f);
        porchDeck.transform.localScale = new Vector3(4.5f, 0.15f, 1.8f);
        SetMaterial(porchDeck, foundationMat);

        // Porch Support Pillars
        CreateHousePillar(house.transform, "Porch_Pillar_L", new Vector3(-2.0f, 1.15f, -5.6f), woodMat);
        CreateHousePillar(house.transform, "Porch_Pillar_R", new Vector3(2.0f, 1.15f, -5.6f), woodMat);

        // Porch Roof
        GameObject porchRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        porchRoof.name = "Porch_Roof"; porchRoof.transform.SetParent(house.transform);
        porchRoof.transform.localPosition = new Vector3(0f, 2.2f, -4.9f);
        porchRoof.transform.localScale = new Vector3(4.7f, 0.15f, 2.0f);
        porchRoof.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
        SetMaterial(porchRoof, roofMat);

        // 6. Glowing Windows (Emissive Yellow)
        // Main Front Window (pointed double-stacked window)
        CreateGlowingWindow(house.transform, "Win_Front_1", new Vector3(-2.2f, 2.2f, -4.06f), new Vector3(0.6f, 1.0f, 0.05f), windowMat);
        CreateGlowingWindow(house.transform, "Win_Front_2", new Vector3(2.2f, 2.2f, -4.06f), new Vector3(0.6f, 1.0f, 0.05f), windowMat);
        
        // Gable Front Window
        CreateGlowingWindow(house.transform, "Win_Front_Gable_1", new Vector3(0f, 4.8f, -3.9f), new Vector3(0.6f, 1.2f, 0.05f), windowMat);
        CreateGlowingWindow(house.transform, "Win_Front_Gable_2", new Vector3(0f, 5.4f, -3.9f), new Vector3(0.4f, 0.4f, 0.05f), windowMat);

        // Tower Windows
        CreateGlowingWindow(house.transform, "Win_Tower_Front", new Vector3(4.1f, 5.2f, -1.05f), new Vector3(0.5f, 1.0f, 0.05f), windowMat);
        CreateGlowingWindow(house.transform, "Win_Tower_Side_R1", new Vector3(5.15f, 4.0f, 0f), new Vector3(0.05f, 1.0f, 0.5f), windowMat);
        CreateGlowingWindow(house.transform, "Win_Tower_Side_R2", new Vector3(5.15f, 5.6f, 0f), new Vector3(0.05f, 0.8f, 0.5f), windowMat);

        // Left Side Windows
        CreateGlowingWindow(house.transform, "Win_Side_L1", new Vector3(-4.06f, 2.0f, 1.5f), new Vector3(0.05f, 0.8f, 0.6f), windowMat);
        CreateGlowingWindow(house.transform, "Win_Side_L2", new Vector3(-4.06f, 2.0f, -1.5f), new Vector3(0.05f, 0.8f, 0.6f), windowMat);

        // 7. Stone Chimney
        GameObject chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chimney.name = "Stone_Chimney"; chimney.transform.SetParent(house.transform);
        chimney.transform.localPosition = new Vector3(-3.2f, 6.2f, 2.2f);
        chimney.transform.localScale = new Vector3(0.6f, 2.2f, 0.6f);
        SetMaterial(chimney, foundationMat);

        return house;
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

    static void SetMaterial(GameObject obj, Material mat)
    {
        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        if (mr != null && mat != null) mr.sharedMaterial = mat;
    }

    static void CreatePointLight(string name, Vector3 pos, Color color, float intensity, float range)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
        }
        var l = obj.GetComponent<Light>();
        if (l == null) l = obj.AddComponent<Light>();
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
        
        var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stick.name = "Stick"; stick.transform.SetParent(candle.transform);
        stick.transform.localPosition = new Vector3(0, 0.15f, 0);
        stick.transform.localScale = new Vector3(0.06f, 0.15f, 0.06f);
        SetMaterial(stick, mat);
        
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
        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.4f;
        cc.center = new Vector3(0f, 1f, 0f);
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
        var canvasObj = GameObject.Find("UAS_WorldSpaceCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("UAS_WorldSpaceCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 400);
            rect.localScale = Vector3.one * 0.003f;
            // Positioned right in front of the Haunted House gate porch
            rect.localPosition = new Vector3(0f, 2.8f, 16f);
            rect.localRotation = Quaternion.Euler(0f, 180f, 0f);

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(canvasObj.transform, false);
            var sTMP = statusObj.AddComponent<TextMeshProUGUI>();
            sTMP.text = "Explore the horror zone...";
            sTMP.fontSize = 35; sTMP.alignment = TextAlignmentOptions.Center;
            statusObj.GetComponent<RectTransform>().sizeDelta = new Vector2(750, 150);
            statusObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 80, 0);

            var promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(canvasObj.transform, false);
            var pTMP = promptObj.AddComponent<TextMeshProUGUI>();
            pTMP.text = ""; pTMP.color = Color.yellow;
            pTMP.fontSize = 28; pTMP.alignment = TextAlignmentOptions.Center;
            promptObj.GetComponent<RectTransform>().sizeDelta = new Vector2(750, 100);
            promptObj.GetComponent<RectTransform>().localPosition = new Vector3(0, -60, 0);
        }
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

        // Interactable: Coffin
        var coffin = GameObject.Find("Coffin");
        if (coffin != null && coffin.GetComponent<UAS_HorrorInteractable>() == null)
        {
            var ci = coffin.AddComponent<UAS_HorrorInteractable>();
            ci.objectName = "Peti Mati Misterius";
            ci.requiresTriggerZone = true;
            ci.floatAndRotate = false;

            var col = coffin.GetComponent<BoxCollider>();
            if (col == null) col = coffin.AddComponent<BoxCollider>();
            col.size = new Vector3(1.2f, 0.8f, 2.4f);
            col.center = new Vector3(0, 0.4f, 0);
        }

        // Trigger zone near coffin (altar trigger)
        if (GameObject.Find("TriggerZone_Coffin") == null)
        {
            var tz = new GameObject("TriggerZone_Coffin");
            tz.transform.position = new Vector3(0f, 1f, 25.5f); // Surrounding the altar
            var bc = tz.AddComponent<BoxCollider>();
            bc.isTrigger = true; 
            bc.size = new Vector3(4f, 3f, 4f);
            
            var tzs = tz.AddComponent<UAS_HorrorTriggerZone>();
            tzs.zoneName = "Zona Ritual Altar"; 
            tzs.horrorSystem = sys;
        }

        EditorUtility.SetDirty(manager);
    }

    static void CreateHangingChain(string name, Vector3 startPos, float length, Material mat)
    {
        if (GameObject.Find(name) != null) return;
        var chain = new GameObject(name);
        chain.transform.position = startPos;
        int links = Mathf.RoundToInt(length / 0.3f);
        for (int i = 0; i < links; i++)
        {
            var link = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            link.name = $"Link_{i}";
            link.transform.SetParent(chain.transform);
            link.transform.localPosition = new Vector3(0f, -i * 0.15f, 0f);
            link.transform.localScale = new Vector3(0.04f, 0.1f, 0.04f);
            SetMaterial(link, mat);
        }
    }

    static void CreateSpiderWeb(string name, Vector3 pos, Material mat)
    {
        if (GameObject.Find(name) != null) return;
        var web = GameObject.CreatePrimitive(PrimitiveType.Quad);
        web.name = name;
        web.transform.position = pos;
        web.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        web.transform.rotation = Quaternion.Euler(45, 45, 0);
        SetMaterial(web, mat);
        
        var col = web.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
    }

    static void CreateHousePillar(Transform parent, string name, Vector3 localPos, Material mat)
    {
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = name;
        pillar.transform.SetParent(parent);
        pillar.transform.localPosition = localPos;
        pillar.transform.localScale = new Vector3(0.12f, 0.9f, 0.12f);
        SetMaterial(pillar, mat);
    }

    static void CreateGlowingWindow(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject win = GameObject.CreatePrimitive(PrimitiveType.Cube);
        win.name = name;
        win.transform.SetParent(parent);
        win.transform.localPosition = localPos;
        win.transform.localScale = localScale;
        SetMaterial(win, mat);
    }
}
