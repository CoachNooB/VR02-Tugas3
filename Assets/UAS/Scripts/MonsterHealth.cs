using UnityEngine;

public class MonsterHealth : MonoBehaviour {
    [Header("Health Settings")]
    public int maxHitCount = 10; // Dibutuhkan 10 hit untuk hancur
    private int currentHitCount = 0;

    [Header("Floating Animation Settings")]
    [Tooltip("Seberapa tinggi monster mengambang naik-turun")]
    public float floatAmplitude = 0.5f; 
    [Tooltip("Seberapa cepat gerakan naik-turunnya")]
    public float floatSpeed = 2f; 

    private Vector3 startPosition;

    void Start() {
        // Simpan posisi awal monster saat game dimulai
        startPosition = transform.position;
    }

    void Update() {
        // Menghitung pergeseran posisi menggunakan rumus Sinus
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        
        // Terapkan posisi baru ke monster
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // Fungsi ini akan dipanggil oleh sistem senjata ketika mengenai monster
    public void TakeDamage() {
        currentHitCount++;
        Debug.Log(gameObject.name + " menerima hit ke-" + currentHitCount);

        if (currentHitCount >= maxHitCount) {
            Die();
        }
    }

    void Die() {
        // Menambah 1 poin ke sistem inventory/manager player
        GunInventory inventory = FindObjectOfType<GunInventory>();
        if (inventory) {
            inventory.AddMonsterPoint();
        }

        Debug.Log(gameObject.name + " telah dikalahkan!");
        Destroy(gameObject); // Monster hilang dari scene
    }
}