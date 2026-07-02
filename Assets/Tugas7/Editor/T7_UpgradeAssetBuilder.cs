using System.IO;
using System.Linq;
using TMPro;
using Tugas7;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Tugas7.Editor
{
    public static class T7_UpgradeAssetBuilder
    {
        public const string LavaMaterialPath = "Assets/Tugas7/Materials/T7_AnimatedLava.mat";
        public const string NpcPrefabPath = "Assets/Tugas7/Prefabs/T7_TutorialNPC.prefab";
        private const string WavingPath = "Assets/Animations/Ch44_nonPBR@Waving.fbx";
        private const string TalkingPath = "Assets/Animations/Ch44_nonPBR@Talking.fbx";
        private const string HeadHitPath = "Assets/Animations/Ch44_nonPBR@Head Hit.fbx";
        private const string VictoryPath = "Assets/Animations/Ch44_nonPBR@Victory Idle.fbx";
        private const string ControllerPath = "Assets/Tugas7/Animations/T7_TutorialNPC.controller";
        private const string NpcMaterialFolder = "Assets/Tugas7/Materials/NPC";
        private const string NpcTextureFolder = "Assets/Tugas7/Textures/NPC";

        public static void PrepareAll()
        {
            EnsureFolders();
            ConfigureTextures();
            ConfigureNpcModels();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            CreateNpcMaterials();
            CreateLavaMaterial();
            CreateEnvironmentMaterials();
            AnimatorController controller = CreateAnimatorController();
            CreateNpcPrefab(controller);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Tugas7", "Animations");
            EnsureFolder("Assets/Tugas7", "Prefabs");
            EnsureFolder("Assets/Tugas7/Prefabs", "Environment");
            EnsureFolder("Assets/Tugas7/Materials", "Environment");
            EnsureFolder("Assets/Tugas7/Materials", "NPC");
            EnsureFolder("Assets/Tugas7/Textures", "NPC");
        }

        private static void ConfigureTextures()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[]
            {
                "Assets/Tugas7/Textures/Lava",
                "Assets/Tugas7/ThirdParty/Industrial/Textures"
            });
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                    continue;
                bool isNormal = path.ToLowerInvariant().Contains("normal");
                importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = path.Contains("BaseColor") || path.Contains("basecolor") ||
                                       path.Contains("emissive");
                importer.mipmapEnabled = true;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.maxTextureSize = path.Contains("/Lava/") ? 1024 : 2048;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureNpcModels()
        {
            ConfigureModel(WavingPath, "Waving", null);
            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(WavingPath).OfType<Avatar>().FirstOrDefault();
            ConfigureModel(TalkingPath, "Talking", avatar);
            ConfigureModel(HeadHitPath, "Head Hit", avatar, false);
            ConfigureModel(VictoryPath, "Victory", avatar);
        }

        private static void ConfigureModel(string path, string clipName, Avatar sourceAvatar, bool loopTime = true)
        {
            if (AssetImporter.GetAtPath(path) is not ModelImporter importer)
                return;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.importAnimation = true;
            importer.avatarSetup = sourceAvatar == null
                ? ModelImporterAvatarSetup.CreateFromThisModel
                : ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = sourceAvatar;
            ModelImporterClipAnimation clip = importer.defaultClipAnimations.FirstOrDefault() ??
                                              new ModelImporterClipAnimation();
            clip.name = clipName;
            clip.loopTime = loopTime;
            clip.keepOriginalOrientation = true;
            clip.keepOriginalPositionXZ = true;
            clip.keepOriginalPositionY = true;
            clip.lockRootRotation = true;
            clip.lockRootPositionXZ = true;
            clip.lockRootHeightY = true;
            importer.clipAnimations = new[] { clip };
            importer.SaveAndReimport();
        }

        private static void CreateNpcMaterials()
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(WavingPath);
            Material[] imported = model == null
                ? System.Array.Empty<Material>()
                : model.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .SelectMany(renderer => renderer.sharedMaterials).Where(material => material != null).ToArray();
            Texture2D clothingMap = FindSemanticMap(imported, false, FindBaseMap);
            Texture2D skinMap = FindSemanticMap(imported, true, FindBaseMap);
            Texture2D clothingNormal = FindSemanticMap(imported, false, FindNormalMap);
            Texture2D skinNormal = FindSemanticMap(imported, true, FindNormalMap);
            if (clothingMap == null)
                clothingMap = EnsureFallbackTexture("T7_NPC_Clothing.png", false);
            if (skinMap == null)
                skinMap = EnsureFallbackTexture("T7_NPC_Skin.png", true);
            CreateNpcMaterial("T7_NPC_Clothing", clothingMap, clothingNormal, false);
            CreateNpcMaterial("T7_NPC_Skin", skinMap, skinNormal, true);
        }

        private static Texture2D FindSemanticMap(Material[] materials, bool skin,
            System.Func<Material, Texture2D> selector)
        {
            foreach (Material material in materials)
            {
                Texture2D texture = selector(material);
                if (texture == null)
                    continue;
                string description = $"{material.name} {texture.name}".ToLowerInvariant();
                bool skinCue = description.Contains("skin") || description.Contains("face") ||
                               description.Contains("head");
                bool clothingCue = description.Contains("cloth") || description.Contains("shirt") ||
                                   description.Contains("jacket") || description.Contains("pants") ||
                                   description.Contains("uniform") || description.Contains("outfit");
                if (skin ? skinCue : clothingCue)
                    return texture;
            }
            return null;
        }

        private static Texture2D FindBaseMap(Material material)
        {
            foreach (string property in new[] { "_BaseMap", "_MainTex" })
            {
                if (material.HasProperty(property) && material.GetTexture(property) is Texture2D texture)
                    return texture;
            }
            return null;
        }

        private static Texture2D FindNormalMap(Material material)
        {
            foreach (string property in new[] { "_BumpMap", "_NormalMap" })
            {
                if (material.HasProperty(property) && material.GetTexture(property) is Texture2D texture)
                    return texture;
            }
            return null;
        }

        private static Texture2D EnsureFallbackTexture(string fileName, bool skin)
        {
            string path = $"{NpcTextureFolder}/{fileName}";
            if (!File.Exists(path))
            {
                const int size = 256;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels = new Color32[size * size];
                var random = new System.Random(skin ? 7128 : 1947);
                Color baseColor = skin ? new Color(0.56f, 0.28f, 0.18f) : new Color(0.055f, 0.19f, 0.27f);
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float detail = skin
                        ? (float)(random.NextDouble() - 0.5) * 0.09f
                        : (((x / 3 + y / 3) & 1) == 0 ? 0.055f : -0.035f) +
                          (float)(random.NextDouble() - 0.5) * 0.025f;
                    pixels[y * size + x] = (Color32)new Color(
                        Mathf.Clamp01(baseColor.r + detail),
                        Mathf.Clamp01(baseColor.g + detail * 0.7f),
                        Mathf.Clamp01(baseColor.b + detail * 0.5f), 1f);
                }
                texture.SetPixels32(pixels);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            if (AssetImporter.GetAtPath(path) is TextureImporter importer &&
                (!importer.sRGBTexture || importer.maxTextureSize != 256))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.maxTextureSize = 256;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void CreateNpcMaterial(string name, Texture2D baseMap, Texture2D normalMap, bool skin)
        {
            string path = $"{NpcMaterialFolder}/{name}.mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BaseMap", baseMap);
            material.SetTexture("_BumpMap", normalMap);
            if (normalMap != null)
                material.EnableKeyword("_NORMALMAP");
            else
                material.DisableKeyword("_NORMALMAP");
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", skin ? 0.32f : 0.16f);
            EditorUtility.SetDirty(material);
        }

        private static void CreateLavaMaterial()
        {
            Shader shader = Shader.Find("Tugas7/Animated Lava");
            if (shader == null)
                return;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(LavaMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, LavaMaterialPath);
            }
            material.shader = shader;
            SetTexture(material, "_BaseMap", "Assets/Tugas7/Textures/Lava/Lava_01_basecolor_1K.png");
            SetTexture(material, "_EmissionMap", "Assets/Tugas7/Textures/Lava/Lava_01_emissive_1K.png");
            SetTexture(material, "_NormalMap", "Assets/Tugas7/Textures/Lava/Lava_01_normal_1K.png");
            SetTexture(material, "_HeightMap", "Assets/Tugas7/Textures/Lava/Lava_01_height_1K.png");
            SetTexture(material, "_RoughnessMap", "Assets/Tugas7/Textures/Lava/Lava_01_roughness_1K.png");
            SetTexture(material, "_AOMap", "Assets/Tugas7/Textures/Lava/Lava_01_ambientocclusion_1K.png");
            material.SetVector("_FlowSpeedA", new Vector4(0.025f, 0.01f, 0f, 0f));
            material.SetVector("_FlowSpeedB", new Vector4(-0.012f, 0.02f, 0f, 0f));
            material.SetFloat("_Tiling", 1.6f);
            material.SetFloat("_EmissionIntensity", 3.5f);
            material.SetFloat("_NormalStrength", 1.1f);
            material.SetFloat("_DistortionStrength", 0.035f);
            material.SetFloat("_DisplacementAmplitude", 0.018f);
            material.SetColor("_CrustColor", new Color(0.16f, 0.025f, 0.008f, 1f));
            material.SetColor("_HotColor", new Color(5f, 0.7f, 0.03f, 1f));
            EditorUtility.SetDirty(material);
        }

        private static void CreateEnvironmentMaterials()
        {
            CreateLitMaterial("WeatheredMetal", new Color(0.28f, 0.31f, 0.34f), 0.82f, 0.28f,
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Trim_02_BaseColor.png",
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Trim_02_Normal.png");
            CreateLitMaterial("ReinforcedConcrete", new Color(0.35f, 0.37f, 0.39f), 0.05f, 0.18f,
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_BaseColor.png",
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_Normal.png");
            CreateLitMaterial("DarkConcreteWall", new Color(0.16f, 0.18f, 0.21f), 0.05f, 0.14f,
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_BaseColor.png",
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_Normal.png");
            CreateLitMaterial("DangerMetal", new Color(0.95f, 0.035f, 0.025f), 0.72f, 0.24f,
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Trim_02_BaseColor.png",
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Trim_02_Normal.png",
                new Color(1.5f, 0.025f, 0.01f));
            CreateLitMaterial("InteractableMetal", new Color(0.02f, 0.72f, 0.9f), 0.58f, 0.3f,
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_BaseColor.png",
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_Normal.png",
                new Color(0.02f, 1.2f, 1.8f));
            CreateLitMaterial("GoldMetal", new Color(1f, 0.55f, 0.03f), 0.68f, 0.32f,
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_BaseColor.png",
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_Normal.png",
                new Color(2f, 0.7f, 0.02f));
            CreateLitMaterial("VolcanicRock", new Color(0.12f, 0.1f, 0.095f), 0.02f, 0.12f,
                "Assets/Tugas7/Textures/Lava/Lava_01_basecolor_1K.png",
                "Assets/Tugas7/Textures/Lava/Lava_01_normal_1K.png");
            CreateLitMaterial("HazardStripe", new Color(0.95f, 0.55f, 0.03f), 0.35f, 0.2f,
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Trim_02_BaseColor.png",
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Trim_02_Normal.png");
            CreateLitMaterial("IndustrialProps", Color.white, 0.4f, 0.25f,
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_BaseColor.png",
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Batch1_Normal.png");
            CreateLitMaterial("IndustrialCrate", Color.white, 0.35f, 0.22f,
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Crates_BaseColor.png",
                "Assets/Tugas7/ThirdParty/Industrial/Textures/T_Props_Crates_Normal.png");
        }

        private static void CreateLitMaterial(string name, Color color, float metallic, float smoothness,
            string basePath, string normalPath, Color? emission = null)
        {
            string path = $"Assets/Tugas7/Materials/Environment/T7_{name}.mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(basePath));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
            material.EnableKeyword("_NORMALMAP");
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }
            EditorUtility.SetDirty(material);
        }

        private static AnimatorController CreateAnimatorController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            AnimatorControllerLayer layer = controller.layers[0];
            AnimatorStateMachine machine = layer.stateMachine;
            if (ControllerIsCurrent(controller, machine))
                return controller;
            foreach (ChildAnimatorState state in machine.states)
                machine.RemoveState(state.state);
            if (!controller.parameters.Any(p => p.name == "IsTalking"))
                controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);
            if (!controller.parameters.Any(p => p.name == "HeadHit"))
                controller.AddParameter("HeadHit", AnimatorControllerParameterType.Trigger);
            if (!controller.parameters.Any(p => p.name == "IsVictorious"))
                controller.AddParameter("IsVictorious", AnimatorControllerParameterType.Bool);

            AnimationClip waving = FindClip(WavingPath, "Waving");
            AnimationClip talking = FindClip(TalkingPath, "Talking");
            AnimationClip headHit = FindClip(HeadHitPath, "Head Hit");
            AnimationClip victory = FindClip(VictoryPath, "Victory");
            AnimatorState wavingState = machine.AddState("Waving");
            wavingState.motion = waving;
            AnimatorState talkingState = machine.AddState("Talking");
            talkingState.motion = talking;
            AnimatorState headHitState = machine.AddState("Head Hit");
            headHitState.motion = headHit;
            AnimatorState victoryState = machine.AddState("Victory");
            victoryState.motion = victory;
            machine.defaultState = wavingState;
            AddVictoryTransition(wavingState, victoryState, false);
            AddVictoryTransition(talkingState, victoryState, false);
            AddTransition(wavingState, talkingState, true);
            AddTransition(talkingState, wavingState, false);
            AnimatorStateTransition enterHit = machine.AddAnyStateTransition(headHitState);
            enterHit.hasExitTime = false;
            enterHit.duration = 0.15f;
            enterHit.canTransitionToSelf = false;
            enterHit.AddCondition(AnimatorConditionMode.If, 0f, "HeadHit");
            AddVictoryTransition(headHitState, victoryState, true);
            AddHeadHitExit(headHitState, talkingState, true);
            AddHeadHitExit(headHitState, wavingState, false);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static bool ControllerIsCurrent(AnimatorController controller, AnimatorStateMachine machine)
        {
            AnimatorState[] states = machine.states.Select(child => child.state).ToArray();
            AnimatorState waving = states.FirstOrDefault(state => state.name == "Waving");
            AnimatorState talking = states.FirstOrDefault(state => state.name == "Talking");
            AnimatorState headHit = states.FirstOrDefault(state => state.name == "Head Hit");
            AnimatorState victory = states.FirstOrDefault(state => state.name == "Victory");
            return states.Length == 4 &&
                   waving != null && talking != null && headHit != null && victory != null &&
                   controller.parameters.Any(parameter =>
                       parameter.name == "IsTalking" &&
                       parameter.type == AnimatorControllerParameterType.Bool) &&
                   controller.parameters.Any(parameter =>
                       parameter.name == "HeadHit" &&
                       parameter.type == AnimatorControllerParameterType.Trigger) &&
                   controller.parameters.Any(parameter =>
                       parameter.name == "IsVictorious" &&
                       parameter.type == AnimatorControllerParameterType.Bool) &&
                   machine.defaultState == waving &&
                   waving.motion == FindClip(WavingPath, "Waving") &&
                   talking.motion == FindClip(TalkingPath, "Talking") &&
                   headHit.motion == FindClip(HeadHitPath, "Head Hit") &&
                   victory.motion == FindClip(VictoryPath, "Victory") &&
                   victory.transitions.Length == 0 &&
                   HasTransition(waving, victory, "IsVictorious", AnimatorConditionMode.If, false) &&
                   HasTransition(talking, victory, "IsVictorious", AnimatorConditionMode.If, false) &&
                   HasTransition(headHit, victory, "IsVictorious", AnimatorConditionMode.If, true) &&
                   HasTransition(waving, talking, "IsTalking", AnimatorConditionMode.If, false) &&
                   HasTransition(talking, waving, "IsTalking", AnimatorConditionMode.IfNot, false) &&
                   HasTransition(headHit, talking, "IsTalking", AnimatorConditionMode.If, true) &&
                   HasTransition(headHit, waving, "IsTalking", AnimatorConditionMode.IfNot, true) &&
                   machine.anyStateTransitions.Any(transition =>
                       transition.destinationState == headHit &&
                       !transition.hasExitTime &&
                       !transition.canTransitionToSelf &&
                       Mathf.Abs(transition.duration - 0.15f) < 0.001f &&
                       transition.conditions.Any(condition =>
                           condition.parameter == "HeadHit" &&
                           condition.mode == AnimatorConditionMode.If));
        }

        private static bool HasTransition(AnimatorState from, AnimatorState to, string parameter,
            AnimatorConditionMode mode, bool exitTime) =>
            from.transitions.Any(transition =>
                transition.destinationState == to &&
                transition.hasExitTime == exitTime &&
                Mathf.Abs(transition.duration - 0.15f) < 0.001f &&
                transition.conditions.Any(condition =>
                    condition.parameter == parameter && condition.mode == mode) &&
                (parameter == "IsVictorious" || transition.conditions.Any(condition =>
                    condition.parameter == "IsVictorious" &&
                    condition.mode == AnimatorConditionMode.IfNot)));

        private static void AddTransition(AnimatorState from, AnimatorState to, bool value)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.15f;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "IsTalking");
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsVictorious");
        }

        private static void AddVictoryTransition(AnimatorState from, AnimatorState to, bool hasExitTime)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.exitTime = hasExitTime ? 0.92f : 0f;
            transition.duration = 0.15f;
            transition.AddCondition(AnimatorConditionMode.If, 0f, "IsVictorious");
        }

        private static void AddHeadHitExit(AnimatorState from, AnimatorState to, bool talking)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = 0.92f;
            transition.duration = 0.15f;
            transition.AddCondition(talking ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f, "IsTalking");
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsVictorious");
        }

        private static AnimationClip FindClip(string path, string name) =>
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                .FirstOrDefault(clip => clip.name == name) ??
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(clip => !clip.name.StartsWith("__"));

        private static void CreateNpcPrefab(AnimatorController controller)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(WavingPath);
            if (modelAsset == null)
                return;
            var root = new GameObject("T7_TutorialNPC");
            GameObject model = Object.Instantiate(modelAsset, root.transform);
            model.name = "GuideModel";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            foreach (Animator nestedAnimator in model.GetComponentsInChildren<Animator>(true))
                Object.DestroyImmediate(nestedAnimator);
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.keepAnimatorStateOnDisable = true;
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(WavingPath).OfType<Avatar>().FirstOrDefault();
            AssignNpcMaterials(model);
            var npc = root.AddComponent<T7_TutorialNPC>();
            CapsuleCollider hitCollider = root.AddComponent<CapsuleCollider>();
            hitCollider.center = new Vector3(0f, 1f, 0f);
            hitCollider.height = 2f;
            hitCollider.radius = 0.42f;

            var trigger = new GameObject("InteractionRange");
            trigger.transform.SetParent(root.transform, false);
            SphereCollider sphere = trigger.AddComponent<SphereCollider>();
            sphere.radius = 3f;
            sphere.isTrigger = true;
            var proximity = trigger.AddComponent<T7_NPCProximityPrompt>();
            proximity.Configure(npc);

            T7_WorldSpaceDialogue dialogue = BuildDialogue(root.transform);
            npc.Configure(animator, dialogue, null);
            PrefabUtility.SaveAsPrefabAsset(root, NpcPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void AssignNpcMaterials(GameObject model)
        {
            Material clothing = AssetDatabase.LoadAssetAtPath<Material>(
                $"{NpcMaterialFolder}/T7_NPC_Clothing.mat");
            Material skin = AssetDatabase.LoadAssetAtPath<Material>($"{NpcMaterialFolder}/T7_NPC_Skin.mat");
            foreach (SkinnedMeshRenderer renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Material[] originals = renderer.sharedMaterials;
                var replacements = new Material[originals.Length];
                for (int i = 0; i < originals.Length; i++)
                {
                    string originalName = originals[i] == null ? string.Empty : originals[i].name.ToLowerInvariant();
                    bool skinLike = originalName.Contains("skin") || originalName.Contains("face") ||
                                    originalName.Contains("head") || (originals.Length > 1 && i == originals.Length - 1);
                    replacements[i] = skinLike ? skin : clothing;
                }
                renderer.sharedMaterials = replacements;
            }
        }

        private static T7_WorldSpaceDialogue BuildDialogue(Transform parent)
        {
            var ui = new GameObject("WorldSpaceDialogue", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(T7_WorldSpaceDialogue));
            ui.transform.SetParent(parent, false);
            ui.transform.localPosition = new Vector3(1.2f, 2.2f, 0f);
            ui.transform.localScale = Vector3.one * 0.004f;
            RectTransform root = (RectTransform)ui.transform;
            root.sizeDelta = new Vector2(600, 260);
            Canvas canvas = ui.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 30;

            GameObject prompt = Panel("PromptPanel", root, new Color(0.01f, 0.025f, 0.04f, 0.94f),
                new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.95f));
            TMP_Text promptText = Text("Prompt", (RectTransform)prompt.transform, "", 34,
                new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.92f), new Color(0.35f, 0.95f, 1f));

            GameObject dialogue = Panel("DialoguePanel", root, new Color(0.015f, 0.02f, 0.03f, 0.96f),
                new Vector2(0f, 0f), new Vector2(1f, 0.62f));
            TMP_Text speaker = Text("Speaker", (RectTransform)dialogue.transform, "FACILITY GUIDE", 28,
                new Vector2(0.04f, 0.7f), new Vector2(0.96f, 0.95f), new Color(1f, 0.58f, 0.12f));
            speaker.alignment = TextAlignmentOptions.Left;
            TMP_Text line = Text("Line", (RectTransform)dialogue.transform, "", 27,
                new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.7f), Color.white);
            line.alignment = TextAlignmentOptions.TopLeft;
            T7_WorldSpaceDialogue component = ui.GetComponent<T7_WorldSpaceDialogue>();
            component.Configure(canvas, prompt, promptText, dialogue, speaker, line, null);
            return component;
        }

        private static GameObject Panel(string name, RectTransform parent, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static TMP_Text Text(string name, RectTransform parent, string value, float size,
            Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.text = value; text.fontSize = size; text.color = color;
            text.enableWordWrapping = true; text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        private static void SetTexture(Material material, string property, string path) =>
            material.SetTexture(property, AssetDatabase.LoadAssetAtPath<Texture2D>(path));

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
