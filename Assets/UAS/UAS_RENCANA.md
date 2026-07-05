# Rencana Wahana Boneka — Peta Poin & Pembagian Tugas

> Target: **nilai maksimal**. Konsep: wahana kereta indoor lewat **4 zona tema beda**.
> Semua dibuat di level yang sudah diajarkan (lihat `CLAUDE.md`) — biar bisa dijelasin saat presentasi.

## Konsep besar
Kereta mini bawa player keliling 4 zona tema, boneka tiap zona nyesuain tema:

| Zona | Tema | Boneka | Suasana (lighting/warna) |
|------|------|--------|--------------------------|
| 1 | **Hutan** | boneka hewan hutan (beruang, rusa, burung) | hijau, cahaya hangat |
| 2 | **Bawah Laut** | boneka ikan, gurita, penyu | biru, cahaya redup bergerak |
| 3 | **Horror** | boneka seram / porselen retak | gelap, merah/ungu, flicker |
| 4 | **Luar Angkasa** | boneka astronot / alien | gelap + bintang, biru-ungu |

> 4 zona = sudah penuhi wajib (min 3) **+ bonus "section ke-4" (+5)**.

## Layout scene (track ± 40 × 10 unit)
```
[PINTU MASUK] -> [BOARDING + tombol START] -> Zona1 Hutan -> Zona2 Laut
                                                                   |
                                              [FINISH/EXIT] <- Zona4 Angkasa <- Zona3 Horror
```
Bentuk **lurus atau L-shape** (boleh belok). Waypoint kereta ngikutin jalur ini.

---

## Peta poin (Fitur Spesifik 60 + Core 30) → cara simpel + penanggung jawab

| Poin | Item | Cara sederhana (level kelas) | Script/Komponen | PIC |
|------|------|------------------------------|-----------------|-----|
| 10 | Track & susunan jelas (40×10) | Susun lantai + objek manual: pintu, boarding, 4 zona, finish | Scene saja | Org 2 |
| 8 | Flow & end state jelas | Player jalan→boarding→naik→lewat zona→finish "Ride Complete" | Trigger + UI | Org 1+5 |
| **10** | **Kereta ikut track, gak keluar jalur** | Waypoint + `Vector3.MoveTowards` di `Update()` | `UAS_KeretaMover` ✅ | Org 2 |
| 8 | ≥3 display konsep beda berurutan | 4 zona tema beda, ditata urut di track | Scene + Prefab | Org 3 |
| 7 | ≥3 interactable via raycast | Tombol start, music box, tombol efek lampu, panel | `UAS_RaycastInteractor` + skrip objek | Org 4 |
| **9** | **≥3 boneka animasi BEDA** | Putar (`Rotate`), naik-turun (bob), goyang, denyut (scale) — beda tiap zona | `UAS_DisplayAnimasi` | Org 3 |
| 8 | ≥3 trigger + feedback | Collider `Is Trigger` + `OnTriggerEnter` → nyalain lampu/musik/warna | `UAS_TriggerZone` | Org 4 |
| **20** | Core: Immersive | FP controller nyaman + World Space UI + interaksi + audio nyatu | gabungan | semua |
| **10** | Core: Tema Visual | 4 zona warna/lighting/material beda, scene gak kosong | Material + Light | Org 5 |

UI utama **WAJIB World Space Canvas** (kalau Overlay → −20): status ride (Ready/Moving/Stopping/Ride Complete), label tiap zona, checklist zona dilewati. → `UAS_RideStatusUI` (Org 5).

---

## Bonus yang diincar (+25 realistis dari maks +30)
- ✅ **Section ke-4** beda konsep — sudah, kita pakai 4 zona (+5)
- **Stop/station**: kereta berhenti sebentar depan display lalu lanjut — `UAS_KeretaMover.Berhenti()` + timer (+5)
- **Sequence animasi 3 tahap**: 1 display → lampu nyala → boneka gerak → musik (timer di `Update`) (+5)
- **Transisi animasi karena interaksi**: tekan tombol (raycast) → tirai buka / lampu panggung nyala (+5)
- **Ride pacing**: kereta melambat depan zona penting (turunin `kecepatan` di trigger) (+5)
- (Opsional berat) Branching track via tombol/lever (+5) — kerjakan kalau sempat

---

## Daftar script (semua level kelas)
| Script | Fungsi | Status |
|--------|--------|--------|
| `UAS_KeretaMover` | Kereta jalan ikut waypoint, stop di akhir | ✅ selesai |
| `UAS_FirstPersonController` | Jalan WASD + lihat mouse | ⬜ |
| `UAS_RaycastInteractor` | Tembak ray dari kamera + tekan E utk interaksi | ⬜ |
| `UAS_TriggerZone` | Deteksi player/kereta masuk area → efek | ⬜ |
| `UAS_DisplayAnimasi` | Animasi boneka (mode: putar/bob/goyang/denyut) | ⬜ |
| `UAS_RideStatusUI` | Update teks status ride + checklist (World Space) | ⬜ |
| `UAS_TombolEfek` | Objek interactable: nyalain lampu/musik/tirai | ⬜ |

> Catatan teknik: untuk interaksi raycast boleh pakai `hit.collider.GetComponent<>()`
> **karena ini diajarkan di P10** (materi physics/raycast). Di luar konteks raycast,
> tetap pakai `[SerializeField]` + drag (jangan GetComponent).

---

## Pembagian 5 orang
1. **Player & Flow** — FP controller, boarding, alur masuk→finish, end state.
2. **Kereta & Track** — waypoint, `UAS_KeretaMover`, station stop, ride pacing.
3. **Display & Animasi** — 4 zona tema + boneka + animasi beda + sequence 3 tahap.
4. **Interaksi & Trigger** — raycast interactable + trigger zone + feedback audio/visual.
5. **UI & Visual** — World Space Canvas (status/checklist/label), lighting/material tiap tema, build WebGL → itch.io.

> Tiap orang HARUS bisa jelasin script & komponen bagiannya (presentasi: pemahaman tim 7 poin).
