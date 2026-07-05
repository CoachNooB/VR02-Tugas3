# Aset Asset Store yang WAJIB di-import (biar scene UAS_Main jalan)

Scene `Assets/UAS/Scenes/UAS_Main.unity` mereferensi prefab dari paket Asset Store di bawah.
**Aset-nya TIDAK di-commit** (ukurannya ~2.3GB, kena limit GitHub + lisensi). Jadi tiap anggota
harus import sendiri lewat: link → **Add to My Assets** → Unity **Package Manager → My Assets → Import**.
Karena GUID Asset Store konsisten, referensi di scene otomatis nyambung setelah di-import.

## Daftar paket (semua Free)
| Zona / Fungsi | Paket | Link |
|---|---|---|
| Hutan (env) | Fantasy Worlds: Forest FREE | https://assetstore.unity.com/packages/package/282610 |
| Horror (ruang) | VINTAGE LIVING ROOM 3D | https://assetstore.unity.com/packages/package/314464 |
| Angkasa (ruang) | 3D Scifi Kit Starter Kit | https://assetstore.unity.com/packages/package/92152 |
| Boneka Hutan | Animals FREE (Animated) — ithappy | https://assetstore.unity.com/packages/package/260727 |
| Boneka Laut | Low Poly Fish — Floreswa | https://assetstore.unity.com/packages/package/339618 |
| Boneka Horror | Low Poly Casual Horror Doll Pack | https://assetstore.unity.com/packages/package/287900 |
| Boneka Horror/Angkasa | Horror Plush Toys: Spooky Alien Mascots | https://assetstore.unity.com/packages/package/255252 |

> Catatan: paket WaterWorks (air) DIHAPUS karena script-nya error di Unity 6. Zona Laut pakai plane biru biasa.
> Kalau aset import jadi pink/magenta → **Edit → Rendering → Materials → Convert All to URP**.
</content>
