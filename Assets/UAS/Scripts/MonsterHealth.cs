using UnityEngine;

public class MonsterHealth : MonoBehaviour {
    [Header("Health Settings")]
    public int maxHitCount = 10; // Dibutuhkan 10 hit untuk hancur
    private int currentHitCount = 0;

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