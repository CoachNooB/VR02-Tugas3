using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [Header("Pengaturan Pintu")]
    public Transform doorMesh;       
    public float openHeight = 5f;      // Tinggi pintu saat terbuka (meter)
    public float openSpeed = 3f;     

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;

    void Start()
    {
        if (doorMesh != null)
        {
            // Kunci posisi awal dan posisi akhir sejak awal game secara mutlak
            closedPosition = doorMesh.position;
            openPosition = closedPosition + new Vector3(0, openHeight, 0);
        }
        else
        {
            Debug.LogError("PENTING: Tarik objek Pintu ke kolom Door Mesh!");
        }
    }

    void Update()
    {
        if (doorMesh == null) return;

        // Tentukan ke mana arah tujuan posisi secara real-time
        Vector3 target = isOpen ? openPosition : closedPosition;

        // Paksa objek bergerak ke target secara absolut
        doorMesh.position = Vector3.MoveTowards(doorMesh.position, target, openSpeed * Time.deltaTime);
    }

    // Fungsi yang dipanggil oleh Button
    public void ToggleDoor()
    {
        isOpen = !isOpen; 
        Debug.Log("Tombol diklik! Status pintu saat ini: " + (isOpen ? "TERBUKA" : "TERTUTUP"));
    }
}