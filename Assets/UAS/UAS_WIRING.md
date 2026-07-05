# Checklist Wiring Scene UAS_Main (drag manual di Inspector)

> Scene `Assets/UAS/Scenes/UAS_Main.unity` sudah dibangun otomatis (objek, posisi,
> komponen, nilai, warna). Yang TIDAK bisa otomatis = **drag referensi antar-objek**.
> Ikuti daftar ini (buka objek di Hierarchy, drag objek ke field di Inspector).
> Centang tiap baris kalau sudah. Estimasi ±10 menit.

## 1. UAS_Kereta → komponen UAS_KeretaMover
- [ ] **Waypoints**: set Size = **5**, lalu drag berurutan:
  Element 0 = `UAS_WP0`, 1 = `UAS_WP1`, 2 = `UAS_WP2`, 3 = `UAS_WP3`, 4 = `UAS_WP4`

## 2. UAS_Player → UAS_FirstPersonController
- [ ] **Kamera Player** = drag `Main Camera` (anak dari UAS_Player)

## 3. UAS_Player → UAS_RaycastInteractor
- [ ] **Kamera** = drag `Main Camera`

## 4. UAS_TombolStart → UAS_ObjekInteraksi
- [ ] **Kereta Start** = `UAS_Kereta`
- [ ] **Player Naik** = `UAS_Player`
- [ ] **Kursi Kereta** = `UAS_Kursi` (anak dari UAS_Kereta)
- [ ] (opsional) **Objek Ubah Warna** = drag `UAS_TombolStart` sendiri (Mesh Renderer-nya) → tombol berubah hijau saat ditekan
- [ ] (opsional) **Suara** = drag objek ber-AudioSource

## 5. UAS_CanvasStatus → UAS_RideStatusUI
- [ ] **Teks Status** = `UAS_TeksStatus` (anak canvas)
- [ ] **Teks Checklist** = `UAS_TeksChecklist` (anak canvas)

## 6. Trigger zone → UAS_TriggerZone (drag Status UI ke semua)
- [ ] `UAS_TrigBoarding` → **Status UI** = `UAS_CanvasStatus`
- [ ] `UAS_TrigDisplay1` → **Status UI** = `UAS_CanvasStatus` ; (opsional) **Animasi Display** = `UAS_Display1`
- [ ] `UAS_TrigDisplay2` → **Status UI** = `UAS_CanvasStatus` ; (opsional) **Animasi Display** = `UAS_Display2`
- [ ] `UAS_TrigDisplay3` → **Status UI** = `UAS_CanvasStatus` ; (opsional) **Animasi Display** = `UAS_Display3`
- [ ] `UAS_TrigFinish` → **Status UI** = `UAS_CanvasStatus` ; **Kereta** = `UAS_Kereta`

## 7. Cek sebelum Play
- [ ] Tiap trigger (UAS_Trig...) → BoxCollider **Is Trigger** tercentang
- [ ] `UAS_Player` Tag = **Player** (harusnya sudah)
- [ ] Directional Light ada (default)

## Cara tes (Play)
1. Tekan **Play**. Cursor terkunci (first-person).
2. Jalan **W** mendekati kereta, arahkan crosshair ke **kotak START**, tekan **E**.
   → player nempel di kursi kereta + kereta mulai jalan + status "Moving".
3. Kereta lewat 4 zona → checklist World Space ke-update (Hutan→Laut→Horror→Angkasa),
   display muter/goyang/dll.
4. Sampai ujung → kereta berhenti + status **"Ride Complete"**.

> Kalau kereta gak gerak: cek Waypoints (no.1) sudah keisi 5.
> Kalau status UI gak berubah: cek no.5 & no.6 (Status UI ke-drag).
> Catatan: interaksi START pakai **physics raycast** (bukan UI button), jadi
> TIDAK butuh EventSystem. Kalau nanti nambah UI Button beneran, baru perlu EventSystem.
</content>
