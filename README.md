# 45: Layered Hell — Unity 6 URP 3D FPS Zombie Survival & Action-RPG (Android)

**45: Layered Hell**, Android işletim sistemi için **Unity 6 (`6000.0.28f1`)**, **C#** ve **Universal Render Pipeline (URP 17.0.3)** kullanılarak geliştirilmiş, 6–7 saatlik derin bir oynanış süresine sahip birinci şahıs (FPS) 3B zombi kıyameti hayatta kalma ve aksiyon-RPG oyunudur.

---

## 1. Teknik Özellikler ve Hedef Platform

| Özellik | Değer |
|---|---|
| **Oyun Motoru** | Unity 6 (`6000.0.28f1`) — Universal Render Pipeline (URP 17.0.3) |
| **Hedef Platform** | Android (Minimum: **Android 10 / API Level 29**, Hedef: **Android 15+ / API Level 35**) |
| **Mimari & Backend** | **ARM64** (`arm64-v8a`), **IL2CPP**, **.NET Standard 2.1**, **Vulkan + OpenGLES3** |
| **Performans Hedefi** | Sabit **60 FPS** (`GameRenderingController` dinamik kalite katmanı ve çözünürlük ölçekleyici) |
| **Paket Boyutu** | Hedef **~1.4 GB** (Kesinlikle **≤ 2.0 GB** sınırı içinde — `PreBuildValidator` ile doğrulanır) |
| **Doku Sıkıştırma** | **ASTC 6x6** (`TextureImportPostprocessor` — 2K Ortam, 1K Karakter/Silah, 512px Prop katmanları) |
| **Ses Sıkıştırma** | **Vorbis (OGG)** (`AudioImportPostprocessor` — kalite 0.45, süreye göre Streaming / CompressedInMemory / DecompressOnLoad) |
| **Dünya Mimarisi** | 50 Katlı (`-4 .. 45`) **Additive Scene Loading + Dinamik Chunk Streaming** (yalnızca aktif kat `±1` bellekte tutulur) |

---

## 2. Hikaye ve Atmosfer: Kule 45 (Aethel-Vanguard Mega Rezidansı)

- **Tarih & Mekan**: **13 Kasım 2026 Cuma gecesi**, kendi kendine yeten 45 katlı dev gökdelen ve 4 katlı yeraltı kompleksi (**toplam 50 kat: B4..B1, 0..45**) **Aethel-Vanguard Mega Rezidansı (Kule 45)**.
- **Chronos Bio-Labs & Project Lazarus**: Yeraltındaki B2–B4 katlarını paravan olarak kiralayan Chronos Bio-Labs'ın yürüttüğü yasadışı **Project Lazarus** deneyinde, **Denek-0 (Hasan Bozkurt / Goliath)** üzerinde yapılan 9. doz **LZR-9 ("Sis")** mutajen testi gece 00:00'da basınç tankı patlamasına yol açar. Gaz havalandırma şaftları ve asansör boşluklarından üst katlara yayılır.
- **KAPAK Harekâtı (Askeri Karantina)**: Albay Sinan Erdem komutasındaki Kule Karantina Komutanlığı, ilk saat içinde zemin kat girişlerini dışarıdan kaynaklı çelik plakalarla mühürler; elektrik ve suyu keserek içerideki 3.000'den fazla sakini ölüme terk eder.
- **Ana Karakter — Aras Kaya**: 38. kattaki **38-B** dairesinde yaşayan 34 yaşındaki eski arama-kurtarma teknisyeni ve saha mühendisi. Dairesini barikatlar, yağmur suyu arıtıcısı, derme çatma ocak, silah/kimya/tıp istasyonları ve telsiz sistemiyle **Güvenli Ev (Safe House)** haline getirir.
- **İki Farklı Son**:
  1. **Rota A — Çatı Helikopter Tahliyesi (`"helicopter"`)**: Yönetici katlarındaki (39–44) şifreli kasalardan (**1704**, **2611**, **4538**) 3 Yönetici Kartını ve 43. kattaki **Radar Kodunu (`UPLINK-7`)** toplayıp 45. kat çatısındaki otomatik hava savunma radarını devre dışı bırakmak, acil durum sinyalini göndermek ve büyük çatı sürü savunmasını kazanarak helikopterle tahliye olmak.
  2. **Rota B — Zırhlı Araç ve Chronos İfşası (`"armored_vehicle"`)**: 8. kattaki güvenlik görevlisi Murat'tan **Laboratuvar Anahtarını** alıp B1–B4 Chronos laboratuvarlarına inmek, Dr. Leyla Aksoy'un bıraktığı **5 gizli Chronos Kanıt Dosyasını** toplamak, B4'te **Denek-0 Goliath** boss'unu yenmek, B1 otoparkında tamirci Tarık ile **Vanguard Zırhlı Müdahale Aracını** onarmak ve zemin kattaki çelik mühürleri kırarak şehre çıkmak.

---

## 3. Klasör Hiyerarşisi

```text
Layered_Hell_45/
├── Docs/
│   ├── CONTRACT.md                  # Tüm C#, Blender ve Ses sistemleri için teknik mimari sözleşmesi
│   └── STORY_BIBLE.md               # Türkçe hikaye, karakterler, şifreler ve kat temaları rehberi
├── Packages/
│   └── manifest.json                # Unity 6 URP 17.0.3, Input System, AI Navigation, Addressables, uGUI
├── ProjectSettings/                 # Android API 29-35, IL2CPP, ARM64, 53 Sahne, Kalite ve Katman ayarları
├── Tools/
│   ├── Audio/                       # Saf Python 3.14 prosedürel ses sentezleme paketi (350-400 WAV)
│   │   ├── synth_core.py            # Osilatörler, formant filtreleri, biquad, yankı, kesintisiz loop
│   │   ├── sounds_zombie.py         # 90 zombi sesi (Walker, Runner, Spitter, Armored, Goliath)
│   │   ├── sounds_weapons.py        # 88 silah, mermi kovanı, yakın dövüş ve patlama sesi
│   │   ├── sounds_foley.py          # 114 adım (6 zemin x 3 hız), 20 eşya toplama, kapı, telsiz, jeneratör sesi
│   │   ├── sounds_world.py          # 78 kat ambiyansı, gerilim müziği, kalp atışı, fırtına ve UI sesi
│   │   └── generate_all_audio.py    # Çok çekirdekli toplu WAV üretici ve doğrulayıcı
│   ├── Blender/                     # 840+ 3B Model, UV, İskelet ve Prosedürel Animasyon Üretim Paketi
│   │   ├── run_blender.py           # Headless Blender 4.2+ FBX/GLB + Armature + NLA Action üretici
│   │   ├── run_all.sh               # Tek komutluk Blender toplu üretim betiği
│   │   ├── export_standalone_glb.py # Blender gerektirmeden 840+ modeli doğrudan .glb olarak üreten araç
│   │   └── ll_blender/              # 16 kategoride 840+ parametrik model ve animasyon kütüphanesi
│   ├── Textures/
│   │   ├── CELL_CONVENTIONS.md      # 2K / 1K / 512px doku atlası hücre ve UV kuralları
│   │   └── gen_textures.py          # 9 adet prosedürel PNG doku atlası üreticisi
│   └── Verify/
│       ├── geo_selftest.py          # Geometri ve manifold doğrulama testi
│       ├── test_anim_math.py        # Döngü-sürekli prosedürel animasyon matematiği testi
│       ├── fake_bpy_check.py        # Blender 4.2+ API uyumluluk testi
│       └── verify_catalog.py        # 840+ model kataloğu, üçgen bütçesi, UV ve kemik doğrulayıcı
└── Assets/
    ├── Art/
    │   ├── Materials/               # GPU Instancing aktif 9 adet M_<ATLAS>.mat materyali
    │   ├── Models/                  # 16 kategoride 840+ 3B model (.glb / .fbx)
    │   └── Textures/                # 9 adet 2K / 1K / 512px PNG doku atlası (Android ASTC 6x6)
    ├── Audio/                       # Zombies, Weapons, Foley, World klasörlerinde 350-400 adet WAV
    ├── Editor/                      # Unity 6 otomatik kurulum, ASTC/Vorbis/FBX postprocessor ve APK/AAB derleyici
    ├── Resources/                   # ModelLibrary ve AudioBank çalışma zamanı veritabanları
    ├── Scenes/                      # Boot, MainMenu, GameWorld ve 50 adet kat sahnesi (Floor_M04..Floor_P45)
    ├── Shaders/
    │   └── AtlasTiling.shader       # SRP Batcher + GPU Instancing uyumlu URP Atlas Tiling shader'ı
    └── _Project/Scripts/
        ├── Core/                    # Contracts, GameBootstrap, GameManager, GameSettings
        ├── Player/                  # PlayerController, PlayerStats, PlayerFactory, FootstepPlayer
        ├── Input/                   # InputRouter (Mobil dokunmatik + Klavye/Fare + Gamepad)
        ├── Interaction/             # Interactor, Door, LootContainer, WorldPickup
        ├── Combat/                  # WeaponController (12 silah + eklentiler + headshot), ThrowableProjectile
        ├── Audio/                   # AudioManager (24 kanallı 3B ses havuzu + dinamik ambiyans/müzik)
        ├── Save/                    # SaveSystem (JSON tabanlı tam durum kaydı + otomatik kayıt)
        ├── Floors/                  # FloorCatalog (50 kat), FloorChunk, FloorStreamer, FloorTransitionPoint, CityBackdrop
        ├── AI/                      # NoiseSystem, ZombieArchetype, 4 katmanlı ZombieAI, GoliathBoss, AcidProjectile, ZombieDirector
        ├── Items/                   # ItemDatabase (110+ eşya, 13 ganimet tablosu), Inventory
        ├── Crafting/                # CraftingSystem (45+ üretim tarifi)
        ├── Base/                    # BaseManager (38. kat güvenli ev + 12 yükseltme), SurvivorNPC (6 uzman komşu), StationInteractable
        ├── Story/                   # StoryDatabase (64 Türkçe günlük, 150+ duvar yazısı, 32+ telsiz kaydı), QuestManager, EndingManager
        ├── Data/                    # ModelLibrary, AudioBank
        ├── Rendering/               # GameRenderingController, ProceduralSkyDarkener, FlickerLight, SceneBootstrapper
        └── UI/                      # UIFactory, MobileControls, HUD, GameMenuUI (6 sekme), MainMenuUI
```

---

## 4. Kurulum ve Çalıştırma Adımları

### A. Ses, Doku ve 3B Modellerin Üretimi (Komut Satırı)
```bash
# 1. 9 adet 2K/1K/512px PNG doku atlasını üret:
python3 Tools/Textures/gen_textures.py

# 2. 840+ 3B model kataloğunu doğrula ve bağımsız .glb modellerini dışa aktar:
python3 Tools/Verify/verify_catalog.py
python3 Tools/Blender/export_standalone_glb.py

# (İsteğe bağlı) Blender 4.2+ kurulu sistemde tam Armature + NLA animasyonlu FBX üretimi:
bash Tools/Blender/run_all.sh Assets/Art/Models fbx

# 3. 350-400 adet 44.1 kHz 16-bit mono WAV ses dosyasını sentezle:
python3 Tools/Audio/generate_all_audio.py
```

### B. Unity 6 İçinde Açma ve Android APK / AAB Derleme
1. **Unity Hub** üzerinden **Unity 6 (`6000.0.28f1` veya üzeri)** ile `Layered_Hell_45` klasörünü açın.
2. Üst menüden **`Layered Hell -> 1. Full Setup (Android API 29-35 + URP + Materials + Libraries + 53 Scenes)`** seçeneğine tıklayın.
3. **`Layered Hell -> 8. Build Android APK (ARM64 IL2CPP)`** veya **`9. Build Android App Bundle AAB (Google Play)`** menüsüyle doğrudan Android çıktısı alın.
