# 45: Layered Hell — Blender (bpy) Prosedürel 3D Model ve Animasyon Üretim Paketi

Bu paket, **840+ modüler 3D modeli**, **UV haritalarını (UV0 + 2K Atlas Tiling için UV1)**, **Humanoid ve FPS El/Silah İskeletlerini (Armature/Rig)** ve **prosedürel animasyon kliplerini (NLA + Action)** sıfırdan inşa edip Unity 6 URP'ye hazır **FBX** veya **GLB** formatında dışa aktarır.

## Mimari ve Modüller (`Tools/Blender/ll_blender/`)

| Dosya | Görev |
|---|---|
| `atlas.py` | 3 katmanlı doku atlası tanımları: `ENV_*_2K` (2048x2048), `HERO_*_1K` (1024x1024), `PROP_*_512` (512x512). |
| `geo.py` | Prosedürel 3B geometri çekirdeği (`box`, `cylinder`, `sphere`, `wedge`, `hbar`, UV projeksiyonu, manifold ve hacim doğrulaması). |
| `rig_spec.py` | 20 kemikli Humanoid iskeleti (`HUMANOID_BONES`), 8 kemikli FPS kol/silah iskeleti (`FPS_BONES`) ve klip tanımları (`Idle, Walk, Run, Attack, Attack2, Hit, Death, Scream, DoorOpen` ve `FPS_*`). |
| `builders_common.py` | Parametrik model inşa yardımcıları (`obox`, `bx`, `prism`, `tube`, `ball`, `qpaint`, `front_label`, `label_cyl`). |
| `builders_*.py` | 12 kategori modülü (`builders_arch`, `builders_res`, `builders_office`, `builders_mall`, `builders_med`, `builders_bar`, `builders_craft`, `builders_weapons`, `builders_chars`, `builders_loot`, `builders_city`, `builders_lab`). |
| `catalog.py` | 16 kategoride **840+ benzersiz 3B model** kataloğu (`CATALOG`). |
| `bl_scene.py` | Blender sahne temizliği, `Geo -> bpy.types.Mesh` dönüşümü, UV katmanları, Vertex Group ağırlıkları ve `M_<ATLAS>` materyal kurulumu. |
| `bl_uv.py` | Cell-atlas UV sınır kontrolü ve isteğe bağlı `bpy.ops.uv.smart_project` paketleme. |
| `bl_rig.py` | Blender `EditBone` / `PoseBone` üzerinden otomatik Armature kurulumu ve Mesh bağlama. |
| `bl_anim_math.py` | Tüm zombi arketipleri (`shamble, sprint, hunch, heavy, boss`), hayatta kalanlar (`survivor`) ve FPS kolları (`fps`) için döngü-sürekli prosedürel animasyon matematiği. |
| `bl_anim.py` | 30 FPS Blender Action anahtar kare (keyframe) üretimi ve NLA Track/Strip aktarımı. |
| `bl_export.py` | Unity 6 eksen uyumlu (`-Z` ileri, `Y` yukarı, `1 birim = 1m`) FBX ve GLB dışa aktarımı. |

## Çalıştırma Komutları

### 1. Katalog ve Animasyon Doğrulaması (Blender gerektirmez)
```bash
python3 Tools/Verify/geo_selftest.py
python3 Tools/Verify/test_anim_math.py
python3 Tools/Verify/verify_catalog.py
python3 Tools/Verify/fake_bpy_check.py
```

### 2. Blender 4.2+ Headless Üretim (Tüm 840+ Model + Animasyon)
```bash
blender -b -P Tools/Blender/run_blender.py -- --out Assets/Art/Models --format fbx
```
Veya sarmalayıcı script ile:
```bash
bash Tools/Blender/run_all.sh Assets/Art/Models fbx
```

### 3. Filtreli veya Paralel (Shard) Çalıştırma
```bash
# Sadece zombileri üret:
blender -b -P Tools/Blender/run_blender.py -- --out Assets/Art/Models --only Zombies

# 4 paralel Blender süreciyle hızlandırılmış üretim:
for i in 0 1 2 3; do
  blender -b -P Tools/Blender/run_blender.py -- --out Assets/Art/Models --shard "$i/4" &
done
wait
```
