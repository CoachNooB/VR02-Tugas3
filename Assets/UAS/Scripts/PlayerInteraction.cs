using UnityEngine;

public class PlayerInteraction : MonoBehaviour {
    [Header("Raycast Settings")]
    [Tooltip("Jarak maksimal interaksi Raycast dengan NPC")]
    public float interactionDistance = 4f;
    
    [Tooltip("Gunakan Layer khusus NPC jika diperlukan, atau biarkan Default")]
    public LayerMask npcLayer;

    private NPCController lastLookedNPC; // Menyimpan NPC terakhir yang dilihat agar bisa dimatikan prompt-nya

    void Update() {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(transform.position, transform.forward * interactionDistance, Color.yellow);

        // Lakukan Raycast
        if (Physics.Raycast(ray, out hit, interactionDistance, npcLayer)) {
            NPCController npc = hit.collider.GetComponent<NPCController>();

            if (npc != null) {
                // Jika melihat NPC baru, matikan prompt NPC sebelumnya (jika ada)
                if (lastLookedNPC != null && lastLookedNPC != npc) {
                    lastLookedNPC.HidePrompt();
                }

                lastLookedNPC = npc;
                
                // Perintahkan NPC yang sedang dilihat untuk memunculkan teks "Tekan E" miliknya sendiri
                npc.ShowPrompt();

                // Cek input tombol E untuk interaksi
                if (Input.GetKeyDown(KeyCode.E)) {
                    npc.Interact();
                }
            } 
            else {
                ClearLastNPC();
            }
        } 
        else {
            ClearLastNPC();
        }
    }

    void ClearLastNPC() {
        if (lastLookedNPC != null) {
            lastLookedNPC.HidePrompt();
            lastLookedNPC = null;
        }
    }
}