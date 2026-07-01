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
        private const string ControllerPath = "Assets/Tugas7/Animations/T7_TutorialNPC.controller";

        public static void PrepareAll()
        {
            EnsureFolders();
            ConfigureTextures();
            ConfigureNpcModels();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
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
        }

        private static void ConfigureModel(string path, string clipName, Avatar sourceAvatar)
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
            clip.loopTime = true;
            clip.keepOriginalOrientation = true;
            clip.keepOriginalPositionXZ = true;
            clip.keepOriginalPositionY = true;
            clip.lockRootRotation = true;
            clip.lockRootPositionXZ = true;
            clip.lockRootHeightY = true;
            importer.clipAnimations = new[] { clip };
            importer.SaveAndReimport();
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
            string basePath, string normalPath)
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
            EditorUtility.SetDirty(material);
        }

        private static AnimatorController CreateAnimatorController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            AnimatorControllerLayer layer = controller.layers[0];
            AnimatorStateMachine machine = layer.stateMachine;
            foreach (ChildAnimatorState state in machine.states)
                machine.RemoveState(state.state);
            if (!controller.parameters.Any(p => p.name == "IsTalking"))
                controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);

            AnimationClip waving = FindClip(WavingPath, "Waving");
            AnimationClip talking = FindClip(TalkingPath, "Talking");
            AnimatorState wavingState = machine.AddState("Waving");
            wavingState.motion = waving;
            AnimatorState talkingState = machine.AddState("Talking");
            talkingState.motion = talking;
            machine.defaultState = wavingState;
            AddTransition(wavingState, talkingState, true);
            AddTransition(talkingState, wavingState, false);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void AddTransition(AnimatorState from, AnimatorState to, bool value)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.15f;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "IsTalking");
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
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(WavingPath).OfType<Avatar>().FirstOrDefault();
            var npc = root.AddComponent<T7_TutorialNPC>();

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
