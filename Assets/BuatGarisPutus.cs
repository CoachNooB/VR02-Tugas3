using UnityEngine;

public class BuatGarisPutus : MonoBehaviour
{
    [Header("Posisi Taman")]
    public float halfSize = 5f;        // Tepi taman di X=±5, Z=±5
    public float linePositionFromCenter = 4.5f; // Geser: 4.5 artinya garis di X=4.5 dan Z=4.5

    [Header("Parameter Garis")]
    public float yPosition = 0.55f;     // Ketinggian garis
    public float segmenLength = 0.8f;   // Panjang tiap segmen
    public float jarakAntarSegmen = 1.2f; // Jarak antar pusat segmen
    public Material garisMaterial;      // Seret material putih Unlit nanti

    void Start()
    {
        float batas = linePositionFromCenter;
        float panjangSisi = halfSize * 2; // 10
        int jumlahSegmen = Mathf.FloorToInt(panjangSisi / jarakAntarSegmen);

        // Sisi Kanan (X = +batas)
        for (int i = 0; i < jumlahSegmen; i++)
        {
            float z = -halfSize + (i + 0.5f) * jarakAntarSegmen;
            BuatPotongan(new Vector3(batas, yPosition, z), new Vector3(0.15f, 0.05f, segmenLength));
        }

        // Sisi Kiri (X = -batas)
        for (int i = 0; i < jumlahSegmen; i++)
        {
            float z = -halfSize + (i + 0.5f) * jarakAntarSegmen;
            BuatPotongan(new Vector3(-batas, yPosition, z), new Vector3(0.15f, 0.05f, segmenLength));
        }

        // Sisi Depan (Z = +batas)
        for (int i = 0; i < jumlahSegmen; i++)
        {
            float x = -halfSize + (i + 0.5f) * jarakAntarSegmen;
            BuatPotongan(new Vector3(x, yPosition, batas), new Vector3(segmenLength, 0.05f, 0.15f));
        }

        // Sisi Belakang (Z = -batas)
        for (int i = 0; i < jumlahSegmen; i++)
        {
            float x = -halfSize + (i + 0.5f) * jarakAntarSegmen;
            BuatPotongan(new Vector3(x, yPosition, -batas), new Vector3(segmenLength, 0.05f, 0.15f));
        }
    }

    void BuatPotongan(Vector3 pos, Vector3 scale)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = pos;
        cube.transform.localScale = scale;
        // Gunakan material khusus jika ada, jika tidak buat warna putih solid
        if (garisMaterial != null)
            cube.GetComponent<Renderer>().material = garisMaterial;
        else
        {
            cube.GetComponent<Renderer>().material.color = Color.white;
            cube.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
        }
        Destroy(cube.GetComponent<BoxCollider>());
        cube.name = $"Garis_{pos.x}_{pos.z}";
    }
}