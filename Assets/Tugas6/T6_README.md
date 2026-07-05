# Tugas 6 — Collider, Rigidbody, Raycast (Pertemuan 10)

> Konsep: **toko mainan** dengan **taman luar**. Player first-person mulai di taman,
> masuk lewat pintu ke toko, lihat/interaksi mainan di meja kiri-kanan.
> Semua UI **World Space Canvas** yang **ikut layar** (HUD). Review lengkap: `T6_CHECKLIST.md`.
> Scene: `Assets/Tugas6/Scenes/T6_Main.unity`.

## Kontrol
- **WASD** jalan, **mouse** lihat, **Space** lompat, **E** interaksi, **klik kiri** dorong.

## Isi scene
- **Taman** (`T6_TamanGround`) di luar + **pintu** (celah di `T6_Wall_S`/`T6_Wall_S2`).
- **Player** (`T6_Player`) physics: Rigidbody dinamis + gravity + lompat → nabrak dinding/meja, jatuhin mainan.
- **Meja kiri-kanan** (`T6_Meja_L`/`T6_Meja_R`) solid.
- **6 mainan** model campur (Rigidbody + `T6_Interactable`): `T6_Toy_Dog`, `_Kitty`, `_Pinguin`, `_Fish`, `_Doll`, + `T6_Toy4_Bonus` (Harimau, terkunci).
- **Kotak** `T6_PushBox` (Rigidbody) buat didorong.
- **Trigger** `T6_TriggerZone` (dalam ruangan): masuk/keluar ubah UI + buka bonus.
- **UI** `T6_CanvasUI` (World Space, HUD ikut layar): `T6_TeksStatus` + `T6_TeksInfo` + panel latar.

## Script (`Assets/Tugas6/Scripts/`)
- `T6_FirstPersonController` — jalan/lihat/**lompat** pakai Rigidbody physics.
- `T6_Interactable` — highlight **glow + membesar** saat dilihat + interaksi E (+ bonus terkunci).
- `T6_RaycastInteractor` — raycast kamera → highlight + E.
- `T6_RigidbodyPusher` — klik kiri dorong Rigidbody.
- `T6_TriggerZone` — masuk/keluar ubah UI + buka bonus.
- `T6_StatusUI` — 2 baris teks TMP.
- `T6_UIIkutKamera` — Canvas World Space mengikuti kamera (HUD) tapi tetap World Space.

## Reference auto-isi
Diisi otomatis di `Awake`/`Start` (`Camera.main`, `GetComponent`/`GetComponentInChildren`,
`transform.Find`, `FindAnyObjectByType`); field `[SerializeField]` tetap bisa di-drag manual.

## Cara test
Lihat `T6_CHECKLIST.md` (ada daftar cek + hal yang perlu dipastikan visual: magenta, skala, orientasi HUD).
