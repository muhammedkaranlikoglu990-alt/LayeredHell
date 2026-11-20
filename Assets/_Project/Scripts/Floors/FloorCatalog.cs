// 45: Layered Hell - 50-Floor Catalog (-4..45) with themes, audio zones, loot tables, and zombie mixes.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    public enum FloorTheme
    {
        Lab,
        LabStorage,
        Tunnels,
        Parking,
        Lobby,
        Mall,
        FoodCourt,
        Office,
        Chemist,
        Gunsmith,
        Residential,
        Clinic,
        Gym,
        Base,
        Executive,
        Ballroom,
        Suite,
        Roof
    }

    [Serializable]
    public class FloorInfo
    {
        public int floor;
        public string name;
        public FloorTheme theme;
        public string ambienceId;
        public string musicId;
        public int danger;
        public string lootTable;
        public string[] zombieMix;
        public string description;

        public FloorInfo(int floor, string name, FloorTheme theme, string ambienceId, string musicId,
            int danger, string lootTable, string[] zombieMix, string description)
        {
            this.floor = floor;
            this.name = name;
            this.theme = theme;
            this.ambienceId = ambienceId;
            this.musicId = musicId;
            this.danger = Mathf.Clamp(danger, 1, 5);
            this.lootTable = lootTable;
            this.zombieMix = zombieMix ?? Array.Empty<string>();
            this.description = description;
        }
    }

    public static class FloorCatalog
    {
        private static readonly Dictionary<int, FloorInfo> ByFloor = new Dictionary<int, FloorInfo>(GameConstants.TotalFloors);
        private static readonly List<FloorInfo> AllList = new List<FloorInfo>(GameConstants.TotalFloors);

        public static IReadOnlyList<FloorInfo> All
        {
            get
            {
                EnsureInitialized();
                return AllList;
            }
        }

        public static FloorInfo Get(int floor)
        {
            EnsureInitialized();
            int clamped = Mathf.Clamp(floor, GameConstants.MinFloor, GameConstants.MaxFloor);
            if (ByFloor.TryGetValue(clamped, out FloorInfo info))
            {
                return info;
            }
            return ByFloor[GameConstants.BaseFloor];
        }

        private static void EnsureInitialized()
        {
            if (AllList.Count > 0) return;

            string[] mixUpperWalkers = { "walker", "walker", "walker", "runner" };
            string[] mixExec = { "walker", "walker", "runner", "spitter", "armored" };
            string[] mixResidential = { "walker", "walker", "walker", "runner" };
            string[] mixMidOffice = { "runner", "runner", "spitter", "walker" };
            string[] mixLowerOffice = { "runner", "runner", "spitter", "spitter", "walker" };
            string[] mixMall = { "walker", "runner", "runner", "spitter" };
            string[] mixLobby = { "armored", "armored", "runner", "spitter" };
            string[] mixBasement = { "armored", "armored", "spitter", "runner" };
            string[] mixLabDeep = { "armored", "armored", "spitter", "spitter" };
            string[] mixSafeBase = Array.Empty<string>();

            Register(new FloorInfo(-4, "B4 - Chronos Lazarus Merkez Laboratuvarı", FloorTheme.Lab, "lab", "mus_boss", 5, "lab", mixLabDeep,
                "Project Lazarus'un sıfır noktası. Patlamış LZR-9 basınç tankları, parçalanmış biyokapsüller ve Denek-0 Hasan Bozkurt'un (Goliath) inine dönüşen ana reaktör salonu."));
            Register(new FloorInfo(-3, "B3 - Biyokarantina ve Numune Depolama", FloorTheme.LabStorage, "lab", "mus_tension", 5, "lab", mixLabDeep,
                "Chronos Bio-Labs dondurucu üniteleri, karantina kafesleri ve Dr. Leyla Aksoy'un son nefesine kadar kanıt sakladığı yüksek güvenlikli laboratuvar arşivleri."));
            Register(new FloorInfo(-2, "B2 - Ana Jeneratör ve Servis Tünelleri", FloorTheme.Tunnels, "basement", "mus_tension", 4, "basement", mixBasement,
                "Kule 45'in kalbini besleyen dev yedek jeneratörler, buhar boruları ve LZR-9 gazının üst katlara sızdığı ana havalandırma şaftları."));
            Register(new FloorInfo(-1, "B1 - Yeraltı Otoparkı ve Vanguard Garajı", FloorTheme.Parking, "parking", "mus_tension", 4, "military", mixBasement,
                "Ağır hasarlı araçlar, Vanguard Güvenlik zırhlı müdahale aracı ve tamirci Tarık Aydın'ın kendini kilitlediği bakım atölyesi."));
            Register(new FloorInfo(0, "Zemin Kat - Mühürlü Ana Lobi", FloorTheme.Lobby, "lobby", "mus_tension", 5, "military", mixLobby,
                "KAPAK Harekâtı kapsamında dışarıdan kaynaklı çelik plakalarla mühürlenmiş devasa mermer lobi. Vanguard güvenlik barikatları ve terk edilmiş tahliye noktası."));

            Register(new FloorInfo(1, "1. Kat - Aethel Galeri Giriş Çarşısı", FloorTheme.Mall, "mall", "mus_explore", 3, "mall", mixMall,
                "Kule AVM'nin lüks vitrinleri, yağmalanmış kuyumcular ve panikle kaçışan kalabalığın geride bıraktığı bavullar."));
            Register(new FloorInfo(2, "2. Kat - Moda ve Tekstil Pasajı", FloorTheme.Mall, "mall", "mus_explore", 3, "mall", mixMall,
                "Devrilmiş vitrin mankenleri arasında dolaşan enfekteler, kırık camlar ve kanlı giysi reyonları."));
            Register(new FloorInfo(3, "3. Kat - Elektronik ve Teknoloji Mağazaları", FloorTheme.Mall, "mall", "mus_explore", 3, "mall", mixMall,
                "Kıvılcım saçan ekranlar, parçalanmış sunucu kabinleri ve devre parçalarıyla dolu teknoloji koridoru."));
            Register(new FloorInfo(4, "4. Kat - Süpermarket ve Kuru Gıda Deposu", FloorTheme.Mall, "mall", "mus_explore", 3, "kitchen", mixMall,
                "Konserve ve su bulmak için birbirini ezen insanların izleriyle dolu, rafları devrilmiş hipermarket katı."));
            Register(new FloorInfo(5, "5. Kat - Eczane, Kozmetik ve Optik Çarşısı", FloorTheme.Mall, "mall", "mus_explore", 3, "medical", mixMall,
                "İlk enfeksiyon belirtileri başladığında yağmalanan eczane kepenkleri ve acil yardım çadırları."));
            Register(new FloorInfo(6, "6. Kat - Kitabevi, Kırtasiye ve Sinema Fuayesi", FloorTheme.Mall, "mall", "mus_explore", 3, "mall", mixMall,
                "Yırtılmış afişler, karanlık cep sineması salonları ve barikat kurulmuş kitap reyonları."));

            Register(new FloorInfo(7, "7. Kat - Restoranlar ve Kafe Terası", FloorTheme.FoodCourt, "mall", "mus_explore", 3, "kitchen", mixMall,
                "Yarım kalmış akşam yemekleri, devrilmiş masalar ve mutfak tezgâhlarında çürüyen erzaklar."));
            Register(new FloorInfo(8, "8. Kat - Büyük Yemek Katı ve Güvenlik Noktası", FloorTheme.FoodCourt, "mall", "mus_explore", 3, "kitchen", mixMall,
                "AVM yemek alanı ve kule güvenlik görevlisi Murat Şen'in telsiz başında direndiği zırhlı güvenlik kabini."));
            Register(new FloorInfo(9, "9. Kat - Fast-Food Meydanı ve Endüstriyel Mutfak", FloorTheme.FoodCourt, "mall", "mus_explore", 3, "kitchen", mixMall,
                "Duman altı kalmış fritözler, soğuk hava depoları ve havalandırma ızgaralarından damlayan mutajen kalıntıları."));
            Register(new FloorInfo(10, "10. Kat - Eğlence Merkezi ve Oyun Salonu", FloorTheme.FoodCourt, "mall", "mus_explore", 3, "mall", mixMall,
                "Karanlıkta hâlâ yanıp sönen bozuk arcade makineleri ve sese duyarlı koşucu zombilerin pusuya yattığı koridorlar."));
            Register(new FloorInfo(11, "11. Kat - AVM Yönetim ve Lojistik Transfer Katı", FloorTheme.Mall, "mall", "mus_explore", 3, "tools", mixMall,
                "Ticari katları iş merkezine bağlayan yük asansörleri, kargo kafesleri ve bakım koridorları."));

            Register(new FloorInfo(12, "12. Kat - Kurumsal İletişim ve Çağrı Merkezi", FloorTheme.Office, "office", "mus_explore", 4, "office", mixLowerOffice,
                "Yüzlerce dar kabinden oluşan açık ofis labirenti; en ufak ayak sesinde harekete geçen koşucularla dolu."));
            Register(new FloorInfo(13, "13. Kat - Veri İşlem ve Sunucu Odaları", FloorTheme.Office, "office", "mus_explore", 4, "office", mixLowerOffice,
                "Uğuldayan sunucu kabinleri, soğutma boruları ve yedek güç akülerinin bulunduğu teknik ofis katı."));
            Register(new FloorInfo(14, "14. Kat - Finans ve Muhasebe Departmanı", FloorTheme.Office, "office", "mus_explore", 4, "office", mixLowerOffice,
                "Kilitli çelik dolaplar, evrak yığınları ve mesaiye kalan çalışanların son barikatları."));
            Register(new FloorInfo(15, "15. Kat - Hukuk Bürosu ve Arşiv Dairesi", FloorTheme.Office, "office", "mus_explore", 4, "office", mixLowerOffice,
                "Chronos Bio-Labs'ın paravan kira sözleşmelerinin gizlendiği ağır ahşap bölmeli hukuk koridorları."));
            Register(new FloorInfo(16, "16. Kat - Mimarlık ve Mühendislik Ofisleri", FloorTheme.Office, "office", "mus_explore", 4, "tools", mixLowerOffice,
                "Kule 45'in statik ve havalandırma planlarının serildiği geniş çizim masaları ve maket atölyesi."));
            Register(new FloorInfo(17, "17. Kat - Medya Ajansı ve Kayıt Stüdyoları", FloorTheme.Office, "office", "mus_explore", 4, "office", mixLowerOffice,
                "Ses yalıtımlı stüdyo odaları, kırık kamera ekipmanları ve karanlık montaj kabinleri."));
            Register(new FloorInfo(18, "18. Kat - Biyomedikal Tedarik ve İthalat Ofisi", FloorTheme.Office, "office", "mus_explore", 4, "lab", mixLowerOffice,
                "Alt katlardaki gizli laboratuvara kimyasal sevkiyat ayarlayan paravan şirketin ofis katı."));

            Register(new FloorInfo(19, "19. Kat - Yıldız Analiz Kimya Laboratuvarı", FloorTheme.Chemist, "office", "mus_explore", 3, "lab", mixMidOffice,
                "Kimyager Elif Yıldız'ın bağımsız analiz laboratuvarı. Asit nötralize edici bileşikler, reaktif dolapları ve güvenlik duşları."));

            Register(new FloorInfo(20, "20. Kat - Lojistik Planlama ve Denizcilik Ofisi", FloorTheme.Office, "office", "mus_explore", 3, "office", mixMidOffice,
                "Cam bölmeleri asit tüküren enfekteler tarafından eritilmiş, evrakları saçılmış kurumsal ofis katı."));
            Register(new FloorInfo(21, "21. Kat - Sigorta ve Risk Yönetimi Katı", FloorTheme.Office, "office", "mus_explore", 3, "office", mixMidOffice,
                "Yangın merdiveni kapılarının dolaplarla kapatıldığı, tavandan kabloların sarktığı karanlık çalışma alanı."));
            Register(new FloorInfo(22, "22. Kat - İnsan Kaynakları ve Eğitim Salonları", FloorTheme.Office, "office", "mus_explore", 3, "office", mixMidOffice,
                "Projeksiyon perdelerine kanla uyarılar yazılmış seminer odaları ve devrilmiş su sebilleri."));
            Register(new FloorInfo(23, "23. Kat - Güvenlik İzleme ve Teknik Bakım Ofisi", FloorTheme.Office, "office", "mus_explore", 3, "tools", mixMidOffice,
                "Orta katların kamera monitörleri, alet dolapları ve tesisat şaftı geçiş kapıları."));

            Register(new FloorInfo(24, "24. Kat - Usta Silah & Tamir Atölyesi", FloorTheme.Gunsmith, "office", "mus_explore", 3, "military", mixMidOffice,
                "Emekli silah ustası Kemal Çelik'in çelik kapılı özel tamirhanesi, torna tezgâhları ve mühimmat dolapları."));

            Register(new FloorInfo(25, "25. Kat - Aile Konutları (Güney Blok)", FloorTheme.Residential, "residential", "mus_explore", 2, "residential", mixResidential,
                "Konut bölgesinin başlangıcı. Gece yarısı uykusunda yakalanan ailelerin daireleri ve koridora saçılmış oyuncaklar."));
            Register(new FloorInfo(26, "26. Kat - Rezidans Daireleri (26-A / 26-F)", FloorTheme.Residential, "residential", "mus_explore", 2, "residential", mixResidential,
                "Kapıları içeriden sürgülenmiş daireler, mutfaklarda yarıda kalmış çay demlikleri ve sessiz koridorlar."));
            Register(new FloorInfo(27, "27. Kat - Konut Katı ve 27-D Amatör Telsiz Odası", FloorTheme.Residential, "residential", "mus_explore", 2, "residential", mixResidential,
                "Telsiz operatörü Nehir Özkan'ın 88.7 Kule Radyosu yayınını kurduğu anten kablolarıyla dolu daire."));
            Register(new FloorInfo(28, "28. Kat - Stüdyo Daireler ve Çamaşırhane", FloorTheme.Residential, "residential", "mus_explore", 2, "residential", mixResidential,
                "Dar koridorlar, ortak çamaşırhane makineleri ve havalandırma mazgallarından gelen uğultu."));
            Register(new FloorInfo(29, "29. Kat - Panoramik Konut Daireleri", FloorTheme.Residential, "residential", "mus_explore", 2, "residential", mixResidential,
                "Fırtınadan kırılmış balkon camları, yağmur suyu dolmuş salonlar ve rüzgârda savrulan perdeler."));
            Register(new FloorInfo(30, "30. Kat - Ara Tesisat ve Konut Katı", FloorTheme.Residential, "residential", "mus_explore", 2, "residential", mixResidential,
                "Üst konutlara su pompalayan basınç boruları ve barikatlı daire girişleri."));

            Register(new FloorInfo(31, "31. Kat - Arslan Polikliniği ve Acil Müdahale", FloorTheme.Clinic, "medical", "mus_explore", 2, "medical", mixResidential,
                "Cerrah Dr. Selin Arslan'ın özel kliniği. Sedye koridorları, ilaç dolapları ve mutajen protein analiz cihazları."));

            Register(new FloorInfo(32, "32. Kat - Rezidans Konutları (32. Blok)", FloorTheme.Residential, "residential", "mus_explore", 2, "residential", mixResidential,
                "Karantina anonslarından sonra valizleriyle asansör önünde beklerken dönüşen bina sakinleri."));
            Register(new FloorInfo(33, "33. Kat - Rezidans Konutları ve Ortak Teras", FloorTheme.Residential, "residential", "mus_explore", 2, "residential", mixResidential,
                "Duvarlarında karanlıkta tebeşirle çizilmiş yardım okları bulunan bakımsız konut koridoru."));
            Register(new FloorInfo(34, "34. Kat - Kule Fitness, Spa ve Spor Kompleksi", FloorTheme.Gym, "gym", "mus_explore", 2, "gym", mixResidential,
                "Koşu bantları, ağırlık sehpaları, kırık aynalar ve soyunma odası dolaplarıyla dolu geniş spor salonu."));
            Register(new FloorInfo(35, "35. Kat - Üst Segment Konut Daireleri", FloorTheme.Residential, "residential", "mus_explore", 2, "residential", mixUpperWalkers,
                "Parke zeminleri kanla lekelenmiş geniş daireler, erzak kilerleri ve kilitli yatak odaları."));
            Register(new FloorInfo(36, "36. Kat - Rezidans Konutları (36-A / 36-D)", FloorTheme.Residential, "residential", "mus_explore", 1, "residential", mixUpperWalkers,
                "Üst sığınak katına yakın, kısmen temizlenmiş ancak karanlık köşelerinde enfektelerin pusuda beklediği daireler."));
            Register(new FloorInfo(37, "37. Kat - Tampon Konut Katı", FloorTheme.Residential, "residential", "mus_explore", 1, "residential", mixUpperWalkers,
                "Aras'ın güvenli evinin hemen altındaki kat. Merdiven boşluğunda erken uyarı tenekeleri ve barikat kalıntıları."));

            Register(new FloorInfo(38, "38. Kat - Daire 38-B Güvenli Ev (Ana Üs)", FloorTheme.Base, "base", "mus_safehouse", 1, "residential", mixSafeBase,
                "Aras Kaya'nın barikatlarla güçlendirdiği dairesi. Yağmur suyu arıtıcısı, tezgâhlar, telsiz ve kurtarılanların sığındığı tek güvenli nokta."));

            Register(new FloorInfo(39, "39. Kat - Aethel-Vanguard İcra Kurulu Ofisleri", FloorTheme.Executive, "executive", "mus_explore", 2, "executive", mixUpperWalkers,
                "Maun masalar, zırhlı cam bölmeler ve Yönetim Kurulu Başkanı Cemil Aethel'in şifreli güvenlik kasası (1704)."));
            Register(new FloorInfo(40, "40. Kat - VIP Toplantı ve Strateji Katı", FloorTheme.Executive, "executive", "mus_explore", 2, "executive", mixUpperWalkers,
                "Şehir manzaralı dev toplantı salonu, deri koltuklar ve Chronos finansman belgelerinin saklandığı dolaplar."));
            Register(new FloorInfo(41, "41. Kat - Chronos Üst Yönetim İrtibat Katı", FloorTheme.Executive, "executive", "mus_explore", 3, "executive", mixExec,
                "Baran Tuncel ve Prof. Dr. Orhan Vural'ın özel ofisleri; kuruluş tarihli yönetici kasası (2611)."));
            Register(new FloorInfo(42, "42. Kat - Kristal Balo Salonu (Alt Fuaye)", FloorTheme.Ballroom, "executive", "mus_explore", 3, "executive", mixExec,
                "Cuma gecesi galası sırasında felaketin vurduğu, dev avizeleri yere çakılmış görkemli davet salonu."));
            Register(new FloorInfo(43, "43. Kat - Balo Salonu Galeri ve Savunma Arşivi", FloorTheme.Ballroom, "executive", "mus_explore", 3, "executive", mixExec,
                "Balo salonunun üst asma katı ve Vanguard hava savunma radar kodlarının (UPLINK-7) tutulduğu güvenlik arşivi."));
            Register(new FloorInfo(44, "44. Kat - Penthouse Yönetici Süiti ve Çatı Güvenliği", FloorTheme.Suite, "executive", "mus_tension", 4, "safe", mixExec,
                "Volkan Demirci'nin özel güvenlik merkezi, 4538 şifreli zırhlı kasa ve çatı helikopter pistine çıkan kilitli çelik kapı."));
            Register(new FloorInfo(45, "45. Kat - Çatı Helikopter Pisti ve Radar Kulesi", FloorTheme.Roof, "roof", "mus_tension", 5, "military", mixExec,
                "Fırtına ve sağanak altında Kule 45'in zirvesi. Vanguard hava savunma radarı, uydu uplink terminali ve son direniş pisti."));
        }

        private static void Register(FloorInfo info)
        {
            ByFloor[info.floor] = info;
            AllList.Add(info);
        }
    }
}
