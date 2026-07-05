# Checklist Review Tugas 6 (Pertemuan 10)

> Buat Izhar review satu-satu: cocokin tiap baris pas **Play** di Unity.
> Scene: `Assets/Tugas6/Scenes/T6_Main.unity`. Kolom **Cek** = yang perlu kamu pastiin sendiri
> (aku gak bisa jalanin Play mode dari MCP).

## Requirement wajib (slide P10)

| # | Requirement | Implementasi di scene | Status | Cek saat Play |
|---|-------------|------------------------|--------|----------------|
| 1 | Player first-person (keyboard+mouse) | `T6_Player` + `T6_FirstPersonController` (physics + lompat), kamera di kepala | ✅ | WASD jalan, mouse lihat, **Space lompat** terasa enak |
| 2 | 1 Trigger Zone ubah status UI **masuk & keluar** | `T6_TriggerZone` (area dalam ruangan). Masuk dari taman lewat **pintu** → "Masuk toko mainan"; keluar → "Di taman (luar)" | ✅ | Jalan dari taman masuk pintu → teks Status berubah; keluar → balik |
| 3 | ≥3 objek Interactable highlight raycast | 6 mainan (`T6_Toy_Dog/Kitty/Pinguin/Fish/Doll` + `T6_Toy4_Bonus`), `T6_Interactable` + `T6_RaycastInteractor` | ✅ (6) | Arah pandang ke mainan → **menyala lembut + membesar dikit** (glow), teks Info muncul |
| 4 | ≥1 interact tombol E | tiap mainan `Interaksi()` (pesan/efek) | ✅ | Lihat mainan → tekan **E** → teks Info ganti (mis. "Guk! ...") |
| 5 | ≥1 box Rigidbody didorong klik mouse | `T6_PushBox` (kotak) + semua mainan = Rigidbody; `T6_RigidbodyPusher` | ✅ | Klik kiri ke kotak/mainan → terdorong. **Lompat/nabrak** mainan → jatuh berantakan |
| 6 | TextMeshPro tampil status | `T6_TeksStatus` + `T6_TeksInfo` (TMP) via `T6_StatusUI`, ada panel latar gelap | ✅ | Teks kebaca jelas |
| 7 | **SEMUA UI World Space Canvas** (Overlay = NILAI 0) | `T6_CanvasUI` Render Mode World Space + `T6_UIIkutKamera` (nempel di layar tapi tetap World Space) | ✅ **kritis** | UI ikut kemana kamera hadap; **cek Inspector Canvas → Render Mode = World Space** |
| Bonus | Objek baru bisa di-interact setelah masuk trigger | `T6_Toy4_Bonus` (Harimau) terkunci → dibuka `T6_TriggerZone` saat masuk ruangan | ✅ | Sebelum masuk ruangan, E ke Harimau → "terkunci"; setelah masuk → bisa |

## Bobot penilaian (slide P10) — perkiraan
| Bobot | Item | Status |
|-------|------|--------|
| 15% | Setup Scene & Player First-Person | ✅ (taman + ruangan + pintu, gerak+lompat) |
| 20% | Trigger Zone & UI Feedback | ✅ (taman↔ruangan) |
| 20% | Raycast & Highlight | ✅ (glow lembut + membesar) |
| 15% | Interaction tombol E | ✅ |
| 15% | Rigidbody Push | ✅ (klik + jatuhin dgn lompat) |
| 15% | Kreativitas & Presentasi | ⚠️ tema toko mainan + model campur (boneka/hewan/ikan) + taman = nilai plus. **Tim wajib bisa jelasin tiap script** |
| −  | UI Overlay / tanpa Canvas World | ✅ AMAN (World Space) |

## Yang WAJIB kamu cek visual (aku gak bisa lihat Game View)
1. **Magenta?** Mainan **Ikan** & **Boneka Hijau** materialnya belum tentu URP. Kalau warnanya pink/ungu ngejreng (magenta) → kabari, aku assign material URP / ganti.
2. **Skala & posisi mainan** di atas meja pas atau ada yang ngambang/nembus → kabari, aku setel.
3. **Orientasi HUD**: kalau teks kebalik/mirror → di Inspector `T6_CanvasUI` → `T6_UIIkutKamera` → centang **Putar Balik**. Posisi HUD bisa diatur field **Jarak** & **Geser**.
4. **Rasa gerak/lompat**: kalau kecepatan/gaya lompat kurang pas → bilang angkanya, aku ubah (`Kecepatan Jalan`, `Gaya Lompat` di Inspector).
5. **Collision meja**: pastikan player **tidak tembus** meja/dinding (kalau tembus, cek Player Rigidbody bukan kinematic).

## Catatan teknik (buat presentasi)
- Controller pakai **Rigidbody dinamis** (materi P10: physics/collision) → bisa nabrak meja & jatuhin mainan.
- Highlight = **emission glow + skala membesar** (bukan ganti warna solid).
- Reference antar-objek diisi **otomatis** (`Camera.main`, `GetComponent`, `FindAnyObjectByType`) tapi field tetap bisa di-drag manual.
