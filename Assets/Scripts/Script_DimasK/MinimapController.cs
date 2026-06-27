using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform playerTransform; // Drag Player di sini

    [Header("Minimap Settings")]
    public float minimapHeight = 20f;         // Ketinggian kamera minimap
    public float minimapSize = 30f;           // Ukuran area yang terlihat
    public bool useRoundMask = true;          // Bentuk bulat
    public float transparency = 0.7f;         // Transparansi (0-1)

    [Header("UI Settings")]
    public Vector2 uiPosition = new Vector2(-20, -20); // Posisi dari kanan atas
    public Vector2 uiSize = new Vector2(200, 200);     // Ukuran minimap di layar

    private Camera minimapCamera;
    private RenderTexture renderTexture;
    private RawImage minimapImage;
    private GameObject uiObject;
    private GameObject maskObject;

    void Start()
    {
        // Jika player tidak di-set, cari secara otomatis
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else
            {
                Debug.LogError("Minimap: Player tidak ditemukan! Assign secara manual.");
                return;
            }
        }

        // 1. Buat kamera minimap
        CreateMinimapCamera();

        // 2. Setup UI Canvas (jika belum ada)
        SetupUI();
    }

    void LateUpdate()
    {
        if (playerTransform == null || minimapCamera == null) return;

        // Ikuti player dari atas
        Vector3 targetPos = playerTransform.position;
        targetPos.y = minimapHeight;
        minimapCamera.transform.position = targetPos;

        // Rotasi kamera menghadap ke bawah (90 derajat)
        minimapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    void CreateMinimapCamera()
    {
        // Buat GameObject untuk kamera minimap
        GameObject camObj = new GameObject("MinimapCamera");
        camObj.transform.SetParent(transform);

        minimapCamera = camObj.AddComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = minimapSize / 2f;
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = new Color(0, 0, 0, 0.5f); // Transparan
        minimapCamera.cullingMask = LayerMask.GetMask("Default"); // Sesuaikan layer

        // Buat RenderTexture
        renderTexture = new RenderTexture(512, 512, 16);
        renderTexture.Create();
        minimapCamera.targetTexture = renderTexture;

        // Tambahkan komponen untuk menampilkan icon player (opsional)
        // Bisa dibuat menggunakan GameObject kecil di bawah player
    }

    void SetupUI()
    {
        // Cari Canvas yang sudah ada (harus ScreenSpaceOverlay)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // Buat Canvas baru
            GameObject canvasObj = new GameObject("MinimapCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Buat GameObject untuk RawImage (tempat render minimap)
        uiObject = new GameObject("MinimapUI");
        uiObject.transform.SetParent(canvas.transform);

        RectTransform rect = uiObject.AddComponent<RectTransform>();
        // Posisi di pojok kanan atas
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = uiPosition;
        rect.sizeDelta = uiSize;

        // Tambahkan RawImage
        minimapImage = uiObject.AddComponent<RawImage>();
        minimapImage.texture = renderTexture;
        minimapImage.color = new Color(1, 1, 1, transparency); // Transparansi

        // Jika menggunakan mask bulat
        if (useRoundMask)
        {
            // Buat mask dengan Image (Sprite lingkaran)
            maskObject = new GameObject("Mask");
            maskObject.transform.SetParent(canvas.transform);
            RectTransform maskRect = maskObject.AddComponent<RectTransform>();
            maskRect.anchorMin = rect.anchorMin;
            maskRect.anchorMax = rect.anchorMax;
            maskRect.pivot = rect.pivot;
            maskRect.anchoredPosition = rect.anchoredPosition;
            maskRect.sizeDelta = rect.sizeDelta;

            Image maskImage = maskObject.AddComponent<Image>();
            // Buat sprite lingkaran secara procedural (sederhana)
            Texture2D circleTex = CreateCircleTexture(128, 128);
            maskImage.sprite = Sprite.Create(circleTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
            maskImage.color = Color.white;

            // Terapkan mask ke RawImage
            minimapImage.maskable = true;
            minimapImage.raycastTarget = false;

            // Pastikan urutan rendering: mask di bawah image
            maskObject.transform.SetSiblingIndex(uiObject.transform.GetSiblingIndex());
        }

        // Buat border sederhana (opsional)
        GameObject border = new GameObject("Border");
        border.transform.SetParent(canvas.transform);
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = rect.anchorMin;
        borderRect.anchorMax = rect.anchorMax;
        borderRect.pivot = rect.pivot;
        borderRect.anchoredPosition = rect.anchoredPosition;
        borderRect.sizeDelta = rect.sizeDelta + new Vector2(4, 4);
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(1, 1, 1, 0.3f);
        borderImage.raycastTarget = false;
        // Letakkan border di belakang
        border.transform.SetSiblingIndex(uiObject.transform.GetSiblingIndex() - 1);
    }

    // Fungsi untuk membuat texture lingkaran (untuk mask)
    Texture2D CreateCircleTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] colors = new Color[width * height];
        Vector2 center = new Vector2(width / 2f, height / 2f);
        float radius = Mathf.Min(width, height) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                    colors[y * width + x] = Color.white;
                else
                    colors[y * width + x] = Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    // Bersihkan resource saat di-destroy
    void OnDestroy()
    {
        if (renderTexture != null)
            renderTexture.Release();
    }
}