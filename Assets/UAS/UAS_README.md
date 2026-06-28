# UAS VR 2026 — Soal 2: Rumah Boneka / Wahana Boneka

Folder kerja kelompok untuk tugas UAS. Semua aset & script UAS diletakkan di sini.

## Konvensi
- Semua file UAS pakai prefix **`UAS_`** (mis. `UAS_KeretaMover.cs`, `UAS_Main.unity`, `UAS_DollMaterial.mat`).
- Untuk script C#, nama class = nama file (jadi class juga `UAS_...`).

## Struktur
- `Scenes/` — scene wahana (mis. `UAS_Main.unity`)
- `Scripts/` — script C# (controller player, kereta, raycast, trigger, UI)
- `Prefabs/` — prefab display boneka, kereta, dll
- `Materials/` — material/warna
- `Audio/` — musik wahana & SFX

## Target tugas (ringkas)
Wahana indoor: player masuk → boarding → naik kereta mini → kereta jalan ikut track
lewat ≥3 display boneka (animasi beda) → finish ("Ride Complete").
UI utama **World Space Canvas**. Build WebGL → itch.io. First-person, keyboard + mouse.
