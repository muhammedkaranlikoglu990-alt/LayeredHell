// 45: Layered Hell — StoryWallAndRadio: 150 Turkish wall texts (3 per floor across -4..45), 40 RadioLogs, 6 Survivors, and all ObjectivePlacements.
using System.Collections.Generic;

namespace LayeredHell
{
    internal static class StoryWallAndRadio
    {
        public static void PopulateWallTexts(Dictionary<int, List<string>> map)
        {
            map.Clear();

            // Floors 45 down to -4 (3 atmospheric Turkish graffiti/wall writings per floor = 150 total)
            AddWalls(map, 45,
                "RADAR AKTİFKEN HELİKOPTER YAKLAŞAMAZ — ÖNCE RADAR KONSOLUNU DEVRE DIŞI BIRAK!",
                "UPLINK-7 KODU OLMADAN ACİL TAHLİYE SİNYALİ GÖNDERİLEMEZ",
                "FIRTINA DİNMİYOR... AŞAĞIDAKİ SÜRÜ ÇATI KAPISINA DAYANDI");

            AddWalls(map, 44,
                "VOLKAN'IN KASASI: KULE KAT SAYISI + SIĞINAK KATI (4538)",
                "BİZİ BURAYA GÖMDÜLER — BASINA TAHLİYE EDİLDİ YALANI SÖYLENDİ",
                "ALTIN KART OLMADAN ÇATI KİLİDİ AÇILMAZ");

            AddWalls(map, 43,
                "BALO SALONU KAN GÖLÜ — HAVA SAVUNMA ARŞİVİNDE UPLINK-7 KODU VAR",
                "ŞAMPANYA KADEHLERİ YERDE, MİSAFİRLER BİRBİRİNİ PARÇALIYOR",
                "SİSİ SOLUMAYIN! MENFEZLERDEN UZAK DURUN!");

            AddWalls(map, 42,
                "ORKESTRA SUSTU — SADECE HIRILTILAR VAR",
                "ISIRILAN BİRİ 2 SAAT İÇİNDE DÖNÜŞÜYOR, KİMSEYE GÜVENME",
                "KRİSTAL AVİZELERİN ALTINDA ÖLÜM DANSI");

            AddWalls(map, 41,
                "CHRONOS KURULUŞ TARİHİ: 26 KASIM (2611)",
                "GÜMÜŞ YÖNETİCİ KARTI KASADA — BRONZ KART GEREKLİ",
                "LAZARus PROJESİ HEPİMİZİN SONU OLDU");

            AddWalls(map, 40,
                "BODRUM KATLARINA İNEN GERİ DÖNMEDİ",
                "HAVALANDIRMA BACALARI B4'TEN ZEHİR POMPALIYOR",
                "PARANIN GEÇMEDİĞİ TEK YER: KULE 45");

            AddWalls(map, 39,
                "CEMİL AETHEL'İN KIZI DEFNE: 17 NİSAN (1704)",
                "YÖNETİCİLER ÖNCE KENDİ KAÇIŞLARINI PLANLADI",
                "BRONZ KART 39. KAT GÜVENLİK KASASINDA");

            AddWalls(map, 38,
                "38-B GÜVENLİ EV — ARAS KAYA: KAPINIZI BARİKATLAYIN, TELSİZİ88.7'YE AYARLAYIN",
                "YAĞMUR SUYUNU ARITMADAN İÇMEYİN — BALKON ARITICISINI KURUN",
                "HAVALANDIRMA KAPAĞINI HURDA METAL VE KOLİ BANDIYLA MÜHÜRLEYİN!");

            AddWalls(map, 37,
                "DIŞARIDA HELİKOPTERLER VAR AMA KİMSE YARDIM ETMİYOR",
                "38-B'YE ÇIKIN! ARAS SIĞINAK KURDU!",
                "ANNE ÖZÜR DİLERİM, KAPIYI AÇAMAM");

            AddWalls(map, 36,
                "MENFEZLERİ BANTLAYIN — GRİ SİS AKLI YOK EDİYOR",
                "36-C DAİRESİ TAMAMEN ENFEKTE, GİRMEYİN!",
                "SU KESİLDİ, ELEKTRİK KESİLDİ, UNUTULDUK");

            AddWalls(map, 35,
                "BABAMIN SESİYLE KAPIYI ÇALIYOR AMA O BABAM DEĞİL",
                "SESSİZ OLUN — KORİDORDA PİJAMALI KOMŞULAR DOLAŞIYOR",
                "31. KATTA DOKTOR SELİN VAR!");

            AddWalls(map, 34,
                "SPOR SALONUNDA KOŞMAYIN! SESİ DUYUP ÜSTÜNÜZE ATILIYORLAR!",
                "DOLAPLARDA ADRENALİN VE İZOTONİK İÇECEK KALDI",
                "KAFALARINA VURUN, GÖVDELERİ ACI HİSSETMİYOR");

            AddWalls(map, 33,
                "SELİN KIZIM SAKIN BANA GELME, SEN PANZEHİRİ BUL!",
                "33-B İÇERİDEN KİLİTLİ — AÇMAYIN",
                "GÖZLERİ MORARAN BİRİNİ GÖRÜRSENİZ KAÇIN");

            AddWalls(map, 32,
                "TEK BİR SİLAH SESİ BÜTÜN KATI BAŞINIZA TOPLAR",
                "BORU VE PALA KULLANIN, MERMİYİ SONA SAKLAYIN",
                "KİLERDE KONSERVE VAR AMA KORİDOR DOLU");

            AddWalls(map, 31,
                "ARSLAN POLİKLİNİĞİ — DR. SELİN ARSLAN: LZR-9 PANZEHİRİ ÜRETİLEBİLİR!",
                "KARANTİNA BÖLMESİNİ AÇMAYIN — HASTALAR BAĞLARINI KOPARDI",
                "ENFEKSİYON %100 OLURSA DÖNÜŞÜRSÜN, ANTİBİYOTİK VEYA PANZEHİR KULLAN!");

            AddWalls(map, 30,
                "ANA ASANSÖRLER ÇÖKTÜ, ŞAFTLAR GAZ DOLU!",
                "YANGIN MERDİVENİNDEN 38. KATA ÇIKIN",
                "13 KASIM GECE YARISI SAATLER DURDU");

            AddWalls(map, 29,
                "ŞEHRİN IŞIKLARI YANIYOR AMA BİZ CEHENNEMİN İÇİNDEYİZ",
                "27. KATTA TELSİZCİ NEHİR DIŞARIYLA TEMAS KURDU!",
                "ISIRIĞI GİZLEYEN HEPİMİZİ ÖLDÜRÜR");

            AddWalls(map, 28,
                "ŞEBEKE SUYU ZEHİRLİ! OCAKTA KAYNATIN VEYA ARITIN!",
                "BORULARDAN SU DEĞİL PAS VE KAN AKIYOR",
                "38-B'DE YAĞMUR SUYU FİLTRESİ VAR");

            AddWalls(map, 27,
                "FM 88.7 KULE RADYOSU — NEHİR ÖZKAN (DAİRE 27-D): SESİMİ DUYAN VAR MI?",
                "ORDU BİNAYI DIŞARIDAN KAYNAKLADI — HAREKÂT ADI: KAPAK",
                "TELSİZİ ONARIRSAK GERÇEĞİ ÖĞRENİRİZ");

            AddWalls(map, 26,
                "BU KATTAN AŞAĞISI KOŞUCU VE ASİT TÜKÜRÜCÜ KAYNIYOR!",
                "ÇÖMELEREK YÜRÜYÜN (CROUCH) — ADIM SESİNİZİ DUYUYORLAR",
                "YEŞİL SALGILI MUTANTLARDAN UZAK DURUN!");

            AddWalls(map, 25,
                "24. KATTA SİLAH USTASI KEMAL USTA DİRENİYOR!",
                "KONUT KATI KAPISI BARİKATLANDI, OFİS KATLARINDAN SÜRÜ GELİYOR",
                "KAFADAN VURUŞ TEK ÇÖZÜM");

            AddWalls(map, 24,
                "USTA SİLAH & TAMİR — KEMAL ÇELİK: BARUT VE HURDA GETİRİN, MERMİ BASALIM!",
                "SİLAHLARINIZI MODİFİYE EDİN, ZIRHLI ZOMBİLERE DÜZ TABANCA İŞLEMEZ",
                "BOŞ KOVANLARI ATMAYIN!");

            AddWalls(map, 23,
                "İNSAN KAYNAKLARI ARŞİVİ: 19 HASTAYI GÖNÜLLÜ DİYE KANDIRDILAR",
                "HASAN BOZKURT = DENEK-0",
                "CHRONOS BIO-LABS BİR HASTANE DEĞİL, SİLAH LABORATUVARI!");

            AddWalls(map, 22,
                "DR. LEYLA AKSOY 5 KANIT DOSYASINI B1-B4 TERMİNALLERİNE YEDEKLEDİ!",
                "KANITLARI TOPLAMADAN ÇIKARSANIZ GERÇEĞİ ÖRTECEKLER",
                "TÜM DIŞ İNTERNET HATLARI KESİLDİ");

            AddWalls(map, 21,
                "ASİT TÜKÜRÜĞÜ KURŞUN GEÇİRMEZ CAMI BİLE ERİTTİ!",
                "19. KATTAKİ KİMYAGER ELİF ASİT KORUMALI ZIRH YAPABİLİYOR",
                "MESAİYE KALDIĞIM GÜNE LANET OLSUN");

            AddWalls(map, 20,
                "HUKUK SERVİSİ İTİRAFI: KAPAK PROTOKOLÜ ÖNCEDEN İMZALANMIŞ!",
                "3.000 İNSANI GÖZDEN ÇIKARDILAR",
                "EVRAK İMHA MAKİNESİ BİLE BU SUÇU GİZLEYEMEZ");

            AddWalls(map, 19,
                "YILDIZ ANALİZ — KİMYAGER ELİF YILDIZ: MOLOTOF, BARUT VE ASİT ZIRHI!",
                "CHRONOS BANA YALAN SÖYLEDİ, BU GÜNAHI TEMİZLEYECEĞİM",
                "TÜKÜRÜCÜLERİN ASİT BEZİ KESESİNİ TOPLAYIN!");

            AddWalls(map, 18,
                "112 CEVAP VERMİYOR — SİNYAL KESİCİLER DEVREDE",
                "DIŞARIDAKİLER BİZİ ÇOKTAN ÖLDÜ SAYDI",
                "YUKARI ÇIKIN, ALT KATLAR TAMAMEN KAYBEDİLDİ");

            AddWalls(map, 17,
                "MİMARİ NOT: 38. KAT EN SAĞLAM SIĞINAK, ZEMİN KAT İSE ZIRHLI ARAÇLA KIRILABİLİR!",
                "B1 OTOPARKINDAKİ ZIRHLI MÜDAHALE ARACI TEK KURTULUŞ",
                "HAVALANDIRMA ŞAFTLARI ÖLÜM TAŞIYOR");

            AddWalls(map, 16,
                "TELEVİZYONDA DOĞALGAZ PATLAMASI DEDİLER, TAHLİYE TAMAMLANDI DEDİLER!",
                "YALANLARINA İNANMAYIN — KANITLARI B1-B4'TEN ALIN!",
                "PENCEREDEN İMDAT FİŞEĞİ YAKTIK AMA GÖRMEDİLER");

            AddWalls(map, 15,
                "BODRUMDAN ÇIKAN TEMİZLİKÇİLER 30 DAKİKADA CANAVARA DÖNÜŞTÜ",
                "MOR TÜPLER KIRILDI, SİS YÜKSELİYOR",
                "GAZ MASKESİ VEYA PANZEHİR OLMADAN B-KATLARINA İNMEYİN");

            AddWalls(map, 14,
                "KASADAKİ MİLYONLAR BİR ŞİŞE SU ETMİYOR",
                "TEK GEÇERLİ PARA: MERMİ, SU, BANDAJ",
                "AÇLIK VE SUSUZLUK ZOMBİLERDEN ÖNCE ÖLDÜRÜR");

            AddWalls(map, 13,
                "13. KAT HAVALANDIRMA DAMPERİ BOZUK — 38. KATTAKİ KAPAĞI MÜHÜRLEYİN!",
                "KOŞUCULAR MERDİVEN BOŞLUĞUNDA PUSUDA",
                "FENERİNİZİN PİLİNİ KORUYUN");

            AddWalls(map, 12,
                "VANGUARD KONTROL NOKTASI ÇÖKTÜ — KENDİ ASKERLERİMİZ DÖNÜŞTÜ!",
                "ZIRHLI GÜVENLİK ZOMBİLERİNE GÖVDEDEN ATEŞ ETMEYİN, KAFAYA NİŞAN ALIN!",
                "AVM KATLARINDAN YUKARIYA SÜRÜ AKIYOR");

            AddWalls(map, 11,
                "YEMEK KATI MUTFAĞINDA KONSERVE VE TEMİZ SU VAR",
                "ZEMİN KATA KOŞANLARIN HİÇBİRİ GERİ DÖNMEDİ",
                "YÜRÜYEN MERDİVENLER DURDU, KAÇIŞ YOK");

            AddWalls(map, 10,
                "SİNEMA SALONU 4'E GİRMEYİN — İÇERİSİ SÜRÜ DOLU!",
                "8. KAT GÜVENLİK KABİNİNDE MURAT ŞEN YARDIM BEKLİYOR!",
                "MURAT'TA LABORATUVAR ANAHTARI VE YÖNETİCİ ŞİFRELERİ VAR");

            AddWalls(map, 9,
                "MOLOTOF ATEŞİ KOŞUCU SÜRÜLERİNİ DURDURMANIN EN İYİ YOLU",
                "8. KATA İNİN, MURAT AĞABEYİ KURTARIN!",
                "SOĞUK HAVA DEPOSUNDA ERZAK KALDI");

            AddWalls(map, 8,
                "GÜVENLİK KABİNİ — MURAT ŞEN: CHRONOS LABORATUVAR ANAHTARI DOLABIMDA!",
                "ŞİFRELER: 39.KAT=1704 | 41.KAT=2611 | 44.KAT=4538",
                "BENİ 38-B'YE ÇIKARIN, TÜM GEÇİŞ KARTI SIRLARINI ANLATAYIM!");

            AddWalls(map, 7,
                "ECZANE DEPOSUNDA ANTİBİYOTİK VE AKTİF KARBON VAR",
                "ENFEKSİYONU DÜŞÜRMEK İÇİN DR. SELİN'İ 31. KATTAN KURTARIN",
                "VİTRİN CAMLARINA BASMAYIN, SES ÇIKARIYOR");

            AddWalls(map, 6,
                "KAMP MAĞAZASINDA MAYMUNCUK VE FENER PİLİ BULABİLİRSİNİZ",
                "KİLİTLİ KAPILARI MAYMUNCUKLA SESSİZCE AÇIN",
                "ALT KATLAR ZİFİRİ KARANLIK");

            AddWalls(map, 5,
                "VİTRİN MANKENLERİNE DİKKAT EDİN — BAZILARI NEFES ALIYOR!",
                "KUMAŞ VE DERİ TOPLAYIP TEZGÂHTA ZIRH DİKİN",
                "KORİDORDAKİ GÖLGELER KOŞUYOR");

            AddWalls(map, 4,
                "HİPERMARKET KEPENKLERİ KAPALI, İÇERİSİ ERZAK VE ZOMBİ DOLU",
                "SU VE KONSERVE ALMADAN DERİNE İNMEYİN",
                "KASA ARKASINDA İLK YARDIM MALZEMESİ VAR");

            AddWalls(map, 3,
                "ATRİUMDAN AŞAĞI BAKMAYIN — LOBİDEKİLERİN HEPSİ ÖLDÜ",
                "ZIRHLI VANGUARD TİMLERİ ENFEKTE OLDU!",
                "5.56 TÜFEK VEYA POMPALI OLMADAN ALT KATLARA İNMEYİN");

            AddWalls(map, 2,
                "B1 OTOPARKINDA TAMİRCİ TARIK ZIRHLI ARACI ONARIYOR!",
                "ZIRHLI ARAÇ İÇİN: MOTOR PARÇASI + AKÜ + YAKIT + ARAÇ ANAHTARI!",
                "MERDİVEN BARİKATI DAYANMIYOR");

            AddWalls(map, 1,
                "ORDU ZEMİN KAT KAPILARINI DIŞARIDAN ÇELİK PLAKALARLA KAYNAKLADI!",
                "KAPAK HAREKÂTI: KİMSEYİ SAĞ ÇIKARMAYACAKLAR",
                "TEK ÇIKIŞ YA 45. KAT ÇATI HELİKOPTERİ YA DA B1 ZIRHLI ARACI!");

            AddWalls(map, 0,
                "MÜHÜRLÜ ANA LOBİ — DIŞARIDAN KAYNAKLI ÇELİK PLAKALARLA KAPATILDI!",
                "YALNIZCA B1'DEKİ ONARILMIŞ ZIRHLI MÜDAHALE ARACI BU BARİKATI YIKABİLİR!",
                "B4'TEKİ GOLIATH ÖLMEDEN VE ARAÇ TAMİR EDİLMEDEN BURADAN ÇIKILAMAZ!");

            AddWalls(map, -1,
                "B1 OTOPARK ATÖLYESİ — TARIK AYDIN: ZIRHLI ARACI ONARIP LOBİYİ YARACAĞIZ!",
                "CHRONOS LABORATUVAR GİRİŞİ: 8. KATTAKİ MURAT'IN ANAHTARI (KEY_LAB) GEREKLİ",
                "B1 GÜVENLİK TERMİNALİNDE KANIT DOSYASI #1 VAR!");

            AddWalls(map, -2,
                "B2 SERVİS TÜNELLERİ — KANIT DOSYASI #2 VE YEDEK MOTOR PARÇASI BURADA!",
                "VOLKAN DEMİRCİ LZR-9 GAZINI KULE HAVALANDIRMASINA KASTEN VERDİ!",
                "JENERATÖR BUHAR HATLARINA DİKKAT EDİN");

            AddWalls(map, -3,
                "B3 KARANTİNA KATI — DR. LEYLA AKSOY'UN KANIT #3 VE KANIT #4 TERMİNALLERİ",
                "DENEK KAFESLERİ PARÇALANDI — B4'TEN KÜKREME SESLERİ GELİYOR!",
                "PROJECT LAZARUS GERÇEĞİNİ DÜNYAYA DUYURUN!");

            AddWalls(map, -4,
                "B4 CHRONOS BIO-LABS MERKEZİ — DENEK-0 HASAN BOZKURT (“GOLIATH”) SERBEST!",
                "ANA SUNUCUDAN KANIT DOSYASI #5'İ İNDİRİN!",
                "GOLIATH'IN KAFASINA NİŞAN ALIN VE HÜCUMLARINDAN YANA KAÇIN!");
        }

        private static void AddWalls(Dictionary<int, List<string>> map, int floor, string w1, string w2, string w3)
        {
            map[floor] = new List<string> { w1, w2, w3 };
        }

        public static void PopulateRadioLogs(List<RadioLog> logs)
        {
            logs.Clear();

            // 1. Start of game
            logs.Add(new RadioLog("rad_start_01", "88.7 Kule Radyosu (Parazitli Yayın)",
                "Cızırtı... Duyan var mı?! Burası 27. kat, ben Nehir! Gece yarısı B4 katındaki patlamadan sonra ordu zemin katı dışarıdan çelik plakalarla kaynakladı! Havalandırmadan gri bir sis yükseliyor, ısırılanlar dönüşüyor! 38. katta yaşayan arama-kurtarmacı Aras Kaya, beni duyuyorsan önce daire kapını barikatla ve telsizini onar!",
                "start"));

            // 2. Floor arrivals
            logs.Add(new RadioLog("rad_fl_38", "Aras Kaya (İç Ses / Telsiz Notu)",
                "38-B Güvenli Ev'deyim. Balkona yağmur suyu arıtıcısı kurup kapıyı kalaslarla sağlamlaştırmalıyım. Kuledeki uzmanları buraya toplarsam buradan çıkış yolunu açabiliriz.",
                "floor_38"));

            logs.Add(new RadioLog("rad_fl_39", "Vanguard İç Güvenlik Kanalı",
                "Dikkat! 39. kat yönetici ofislerinde enfekte yönetim kurulu personeli var. Cemil Aethel'in kasası kızı Defne'nin doğum günü olan 17 Nisan (1704) şifresiyle kilitli.",
                "floor_39"));

            logs.Add(new RadioLog("rad_fl_41", "Chronos Yönetim Frekansı",
                "41. kat Chronos Bio-Labs yönetici katı. Gümüş Yönetici Kartı kasası için şirket kuruluş tarihi olan 26 Kasım (2611) ve Bronz Kart gerekiyor.",
                "floor_41"));

            logs.Add(new RadioLog("rad_fl_43", "Kaptan Pilot Emre (Acil Kayıt)",
                "43. kat balo salonu arşivi... Çatıdaki hava savunma radarından geçebilmek için buradaki terminalden UPLINK-7 radar kodunu almanız şart!",
                "floor_43"));

            logs.Add(new RadioLog("rad_fl_44", "Güvenlik Şefi Volkan Demirci (Kayıt)",
                "44. kat süit kasasına 3. Kademe Altın Kartı kilitledim. Şifre kule kat sayısı ile aşağıdaki sığınak katı: 4538. Çatı kapısı paneli bu kartla açılır.",
                "floor_44"));

            logs.Add(new RadioLog("rad_fl_45", "Kule Hava Savunma Sistemi",
                "UYARI: 45. Kat Helipad Hava Savunma Radarı AKTİF. Yaklaşan tüm helikopterler kilitlenecektir. Konsolu devre dışı bırakmak için 2 Elektronik Devre Kartı ve 2 Bakır Kablo gereklidir.",
                "floor_45"));

            logs.Add(new RadioLog("rad_fl_34", "Antrenör Bora (34. Kat Gym)",
                "34. kat spor salonuna giren kimse, sakın koşma! Buradaki enfekteler sese duyarlı koşucular! Çömelerek ilerle!",
                "floor_34"));

            logs.Add(new RadioLog("rad_fl_31", "Op. Dr. Selin Arslan (31. Kat Klinik)",
                "Burası 31. kat Arslan Polikliniği! Ameliyathanede mahsur kaldım! Elimde LZR-9 mutajenine karşı panzehir formülü var, lütfen koridordaki enfekteleri temizleyip beni çıkarın!",
                "floor_31"));

            logs.Add(new RadioLog("rad_fl_27", "Nehir Özkan (27. Kat 27-D)",
                "27. kata birinin ayak sesleri geldi! Aras sen misin?! 27-D dairesindeyim, elimde 88.7 Kule Radyosu'nun yedek verici parçaları var!",
                "floor_27"));

            logs.Add(new RadioLog("rad_fl_24", "Kemal Usta (24. Kat Silah Atölyesi)",
                "24. kat Usta Silah & Tamir! Kepengin arkasındayım evlat! Beni 38. kata çıkarırsan sana her kalibrede mermi basar, silahlarını modifiye ederim!",
                "floor_24"));

            logs.Add(new RadioLog("rad_fl_19", "Kimyager Elif Yıldız (19. Kat Laboratuvar)",
                "19. kat Yıldız Analiz laboratuvarındayım! Koridor asit tüküren mutantlarla dolu. Yanıma ulaşabilirseniz asit korumalı zırh ve patlayıcı üretebilirim!",
                "floor_19"));

            logs.Add(new RadioLog("rad_fl_12", "Vanguard Taktik Telsizi",
                "12. kat kontrol noktası düştü! Aşağıdaki kendi zırhlı güvenlik timlerimiz enfekte oldu. Gövde zırhları kurşun geçirmiyor, kafalarına nişan alın!",
                "floor_12"));

            logs.Add(new RadioLog("rad_fl_8", "Murat Şen (8. Kat Güvenlik Kabini)",
                "Hey! 8. kat yemek alanı güvenlik kabinindeyim! B1-B4 Chronos laboratuvarının anahtarı ve yönetici katı şifreleri bende! Kabin kapısını açıyorum!",
                "floor_8"));

            logs.Add(new RadioLog("rad_fl_4", "AVM Acil Anons Sistemi",
                "4. kat hipermarket bölgesinde yoğun enfekte hareketliliği. Erzak reyonlarında konserve ve su mevcut ancak ses çıkarmamaya dikkat edin.",
                "floor_4"));

            logs.Add(new RadioLog("rad_fl_0", "Albay Sinan Erdem (KAPAK Komutanlığı Megafonu)",
                "Kule 45 içerisindeki unsurların dikkatine! KAPAK Harekâtı uyarınca zemin kat çelik plakalarla mühürlenmiştir. Hiçbir yaya çıkışı mümkün değildir!",
                "floor_0"));

            logs.Add(new RadioLog("rad_fl_b1", "Tarık Aydın (B1 Otopark Atölyesi)",
                "B1 otoparkına inen kim?! Ben makinist Tarık! Önümde 12 tonluk Vanguard Zırhlı Müdahale Aracı var. Bana motor parçası, akü ve yakıt getirirsen zemin kattaki ordu mührünü kırıp geçeriz!",
                "floor_-1"));

            logs.Add(new RadioLog("rad_fl_b2", "B2 Otomatik Sensör Uyarısı",
                "B2 Servis Tünelleri ve Jeneratör Katı. Havalandırma arşiv terminalinde Chronos Kanıt Dosyası #2 ve teknik depoda Ağır Vasıta Motor Parçası tespit edildi.",
                "floor_-2"));

            logs.Add(new RadioLog("rad_fl_b3", "Dr. Leyla Aksoy (Otomatik Acil Kayıt)",
                "B3 Biyo-Karantina Katına ulaştınız. Lütfen karantina terminalinden Kanıt #3'ü ve şifreli USB sürücümden Kanıt #4'ü alın. B4'e inmeden önce silahlarınızı doldurun; Denek-0 aşağıda.",
                "floor_-3"));

            logs.Add(new RadioLog("rad_fl_b4", "Prof. Dr. Orhan Vural (B4 Acil Anons)",
                "B4 Chronos Merkez Laboratuvarı karantina ihlali! Denek-0 Hasan Bozkurt — Goliath — serbest! Ana sunucudaki Project Lazarus verilerini (Kanıt #5) silmeye vaktimiz kalmadı!",
                "floor_-4"));

            // 3. Objective triggers
            logs.Add(new RadioLog("rad_obj_barricade", "Aras Kaya",
                "38-B kapı ve pencere barikatları sağlamlaştırıldı. En azından koridordaki yürüyüşçüler içeri elini kolunu sallayarak giremeyecek.",
                "barricade_built"));

            logs.Add(new RadioLog("rad_obj_purifier", "Aras Kaya",
                "Balkondaki yağmur suyu arıtıcısı devreye girdi. Fırtına devam ettiği sürece sandıkta temiz içme suyu birikecek.",
                "purifier_built"));

            logs.Add(new RadioLog("rad_obj_generator", "Aras Kaya",
                "38-B daire jeneratörü çalıştı! Işıklar, tezgâhlar ve telsiz hattı artık tam güçte.",
                "generator_online"));

            logs.Add(new RadioLog("rad_obj_radio", "Nehir Özkan (88.7 Kule Radyosu)",
                "88.7 Kule Radyosu vericisi 38-B'de tam kapasite yayında! Artık hem kurtarma helikopteri frekansını hem de bodrum katı sinyallerini net duyabiliyoruz.",
                "radio_repaired"));

            logs.Add(new RadioLog("rad_obj_vent", "Aras Kaya",
                "38. katın ana havalandırma şaft kapağını çelik sac ve bantla mühürledim. Artık B4'ten yükselen Sis gazı sığınağımıza sızamaz.",
                "vent_sealed"));

            logs.Add(new RadioLog("rad_obj_selin", "Op. Dr. Selin Arslan",
                "38-B'ye sağ salim ulaştım Aras. Tıbbi İstasyonu kurdum; bana dondurulmuş LZR-9 örneği ve antibiyotik getirirsen Panzehir Serumu üretebilirim.",
                "selin_rescued"));

            logs.Add(new RadioLog("rad_obj_kemal", "Kemal Usta",
                "Tezgâhı 38-B'ye kurdum evlat! Barut, hurda metal ve boş kovan getir; seni mermisiz bırakmam.",
                "kemal_rescued"));

            logs.Add(new RadioLog("rad_obj_elif", "Kimyager Elif Yıldız",
                "Kimya İstasyonu hazır Aras. Tükürücülerin asit bezlerinden asit korumalı zırh, gübre ve yakıttan da patlayıcı hazırlayabiliriz.",
                "elif_rescued"));

            logs.Add(new RadioLog("rad_obj_nehir", "Nehir Özkan",
                "38-B'deki telsiz antenini dış frekanslara bağladım! Çatıdaki radar susturulursa bağımsız arama-kurtarma helikopteri Kartal-3 bizi almaya gelecek!",
                "nehir_rescued"));

            logs.Add(new RadioLog("rad_obj_murat", "Murat Şen",
                "Sağ olasın Aras! Sana Chronos Laboratuvar Anahtarı'nı verdim; 8. kattaki dolabımdan yedeğini de alabilirsin. Unutma: 39. kat şifresi 1704, 41. kat 2611, 44. kat 4538!",
                "murat_rescued"));

            logs.Add(new RadioLog("rad_obj_tarik", "Tarık Aydın",
                "Zırhlı aracın kontak anahtarını sana verdim Aras! B2'den motor parçasını, bir akü ve bir bidon yakıtla birlikte B1'deki araca getirirsen bu çelik tabutu yarar çıkarız!",
                "tarik_rescued"));

            logs.Add(new RadioLog("rad_obj_kc1", "Aras Kaya",
                "39. kattaki Cemil Aethel'in kasasını 1704 şifresiyle açtım. 1. Kademe Bronz Yönetici Kartı elimde.",
                "exec_keycard_1"));

            logs.Add(new RadioLog("rad_obj_kc2", "Aras Kaya",
                "41. kattaki Chronos kasasını 2611 şifresiyle açtım. 2. Kademe Gümüş Yönetici Kartı'nı aldım; şimdi 43. kattaki arşivden radar kodunu ve 44. kattan Altın Kartı alabilirim.",
                "exec_keycard_2"));

            logs.Add(new RadioLog("rad_obj_kc3", "Aras Kaya",
                "44. kattaki Volkan Demirci'nin kasasından 3. Kademe Altın Yönetici Kartı'nı (4538) aldım! Çatı kapısı kilidini açma vakti.",
                "exec_keycard_3"));

            logs.Add(new RadioLog("rad_obj_roof", "Nehir Özkan (Telsiz)",
                "Aras çatı kapısını açtın! Şimdi 45. kattaki radar konsolunu 2 devre kartı ve 2 bakır kabloyla kısa devre yaptır, sonra UPLINK-7 koduyla helikopter sinyalini gönder!",
                "roof_unlocked"));

            logs.Add(new RadioLog("rad_obj_radar", "Kartal-3 Kurtarma Helikopteri",
                "Kule 45, burası bağımsız arama-kurtarma helikopteri Kartal-3! Çatıdaki hedef kilitleme radarının sustuğunu görüyoruz! Şimdi Uplink terminalinden UPLINK-7 koordinat sinyalini gönderin!",
                "radar_disabled"));

            logs.Add(new RadioLog("rad_obj_signal", "Kartal-3 Kurtarma Helikopteri",
                "UPLINK-7 sinyali alındı Aras! Fırtına içinden 45. kat helipadine yaklaşıyoruz ama siren sesi bütün üst katlardaki enfekte sürüsünü çatıya çekti! Helikopter inene kadar pisti savunun!",
                "signal_sent"));

            logs.Add(new RadioLog("rad_obj_lab", "Dr. Selin Arslan (Telsiz)",
                "Aras, B1'deki Chronos laboratuvar kapısını açtın. İçerideki 5 kanıt terminalindeki verileri indirirsen bu katliamın sorumlularını tüm dünyaya ifşa edebiliriz!",
                "lab_unlocked"));

            logs.Add(new RadioLog("rad_obj_ev5", "Nehir Özkan (Telsiz)",
                "B4 ana sunucusundan 5. kanıt paketini de aldın! Project Lazarus ve KAPAK Harekâtı'nın bütün suç belgeleri elimizde!",
                "evidence_collected_5"));

            logs.Add(new RadioLog("rad_obj_goliath", "Aras Kaya",
                "B4 katındaki Denek-0... Hasan Bozkurt, yani Goliath sonunda devrildi. Artık acı çekmiyor. Şimdi buradan çıkma zamanı.",
                "goliath_defeated"));

            logs.Add(new RadioLog("rad_obj_vehicle", "Tarık Aydın",
                "Dizel motor kükredi Aras! 12 tonluk Vanguard Zırhlı Müdahale Aracı tam güçte! Zemin kata (Kat 0) çıkıp o kaynaklı çelik lobi kapısını paramparça edelim!",
                "vehicle_repaired"));

            // 4. Ghost / ambient eerie transmissions
            logs.Add(new RadioLog("rad_ghost_01", "Bilinmeyen Frekans (Parazit)",
                "...sis... havalandırmadan geliyor... kapıyı açma... o artık senin kardeşin değil...",
                "ghost"));

            logs.Add(new RadioLog("rad_ghost_02", "KAPAK Komutanlığı Şifreli Kanal",
                "...Sabah 06:00 itibarıyla Kule 45'te termal hareketlilik devam ederse B4 kolonlarına yerleştirilen yıkım protokolü değerlendirilecektir...",
                "ghost"));

            logs.Add(new RadioLog("rad_ghost_03", "Kesik Sivil Çağrısı",
                "...35. kattayız... suyumuz bitti... dışarıdaki helikopterler neden sadece bakıyor?!...",
                "ghost"));
        }

        public static void PopulateSurvivors(List<SurvivorDef> list)
        {
            list.Clear();

            list.Add(new SurvivorDef(
                "selin",
                "Op. Dr. Selin Arslan",
                31,
                "med_station",
                "Cerrah & Biyokimyacı",
                new[]
                {
                    "Aras?! Tanrım, ameliyathane kapısına dayananların enfekte olduğunu görünce kimse sağ kalmadı sanmıştım! Ben Dr. Selin Arslan. Kan örneklerinde Chronos Bio-Labs'ın LZR-9 mutajenini tespit ettim. 38-B'de güvenli bir evin varsa hemen oraya çıkalım; tıbbi istasyonumu kurup panzehir üretebilirim!",
                    "Annem 33. katta kaldı ama beni yanına yaklaştırmadı... Onun fedakârlığını boşa çıkarmayacağım, 38-B'de hayat kurtaracağız."
                },
                new[]
                {
                    "38-B'deki Tıbbi İstasyon hazır Aras. Dondurulmuş LZR-9 örneği ve antibiyotik getirirsen kandaki enfeksiyonu temizleyen Panzehir Serumu (Antidote) sentezleyebilirim.",
                    "Alt katlardaki Sis gazı çok yoğun. Enfeksiyon oranın yükselirse hemen yanıma gel veya bir antibiyotik yut.",
                    "Dr. Leyla Aksoy B1 ile B4 arasındaki terminallere Project Lazarus kanıtlarını saklamış. Onları mutlaka kule dışına çıkarmalıyız."
                }));

            list.Add(new SurvivorDef(
                "kemal",
                "Kemal Usta (Kemal Çelik)",
                24,
                "weapon_bench",
                "Silah Ustası",
                new[]
                {
                    "İndir o silahı evlat, benden zarar gelmez! Ben Kemal Usta. Saatlerdir 24. kattaki atölyemde bu kudurmuş sürüye kurşun sıkıyorum ama havalandırmadan asit tükürenler sızmaya başladı. 38-B'de sağlam bir tezgâhın varsa takım çantamı sırtlanıp geliyorum!",
                    "Rahmetli Fatma'mın fotoğrafını cebime koydum. Hadi 38. kata çıkalım, sana gerçek mühimmat nasıl basılır göstereyim."
                },
                new[]
                {
                    "Silah Tezgâhı tıkır tıkır çalışıyor Aras! Barut, hurda metal, pirinç kovan ve kurşun getir; 9mm'den 7.62 keskin nişancı fişeğine kadar ne istersen dökeyim.",
                    "Zırhlı Vanguard zombilerine düz tabancayla mermi yakma. Silah Modifikasyon Kiti üretip tüfeğini güçlendir ve daima kafalarına nişan al.",
                    "Yakın dövüşte sessiz iş bitirmek istiyorsan tezgâhta sana karbon çelikten bir pala veya yangın baltası hazırlayabilirim."
                }));

            list.Add(new SurvivorDef(
                "elif",
                "Elif Yıldız",
                19,
                "chem_station",
                "Analitik Kimyager",
                new[]
                {
                    "Sakın ateş etme, enfekte değilim! Ben Elif... 19. kat kimya laboratuvarının sahibiyim. Chronos Bio-Labs'a aylarca ne olduğunu bilmeden çözücü sattım, bu felakette benim de payım var! Beni 38-B'ye götürürsen asit korumalı zırh ve patlayıcı üreterek borcumu öderim!",
                    "Tükürücü mutantların asidi normal yelekleri eritiyor ama geliştirdiğim polimer kaplama onları durduruyor. Hemen 38-B'ye çıkalım!"
                },
                new[]
                {
                    "Kimya İstasyonu'nu 38-B'ye kurdum Aras. Tükürücü mutantlardan çıkan Asit Bezi Kesesi ile sana Kimyasal & Asit Korumalı Zırh üretebilirim.",
                    "Dar koridorlardaki sürüleri temizlemek için boş şişe, bez ve yakıttan Molotof; barut ve çividen de El Bombası hazırlayabiliriz.",
                    "Barutun biterse endişelenme; kükürt, gübre ve kimyasal çözücülerden Kemal Usta'nın mermileri için taze barut sentezliyorum."
                }));

            list.Add(new SurvivorDef(
                "nehir",
                "Nehir Özkan",
                27,
                "radio_station",
                "Telsiz Operatörü (88.7 FM)",
                new[]
                {
                    "Aras! 88.7 frekansındaki çağrımı duydun demek! Ben Nehir. Ordu bütün telefon hatlarını kararttı ama kısa dalga telsizimle hem içerideki güvenlik kanallarını hem de dışarıdaki bağımsız kurtarma helikopteri Kartal-3'ü yakaladım! Beni 38-B'ye çıkarırsan ana radyoyu kurarım!",
                    "Dışarıdaki Albay Sinan Erdem sabaha karşı kuleyi tamamen imha etmekten bahsediyor! Acele etmeliyiz!"
                },
                new[]
                {
                    "88.7 Kule Radyosu 38-B'den yayında! Kartal-3 helikopterinin çatıya inebilmesi için önce 45. kattaki hava savunma radarını devre dışı bırakıp UPLINK-7 koduyla sinyal göndermemiz gerekiyor.",
                    "Telsizden yakaladığım tüm konuşmaları Günlük menüsünün Radyo sekmesinden tekrar dinleyebilirsin Aras.",
                    "Eğer çatı yolu çok riskliyse B1 otoparkındaki makinist Tarık'ın zırhlı aracıyla zemin kat barikatını kırmak ikinci şansımız!"
                }));

            list.Add(new SurvivorDef(
                "murat",
                "Murat Şen",
                8,
                "security_intel",
                "Kıdemli Güvenlik Görevlisi",
                new[]
                {
                    "Şükürler olsun yaşayan bir insan! Ben kule güvenliğinden Murat Şen. Volkan Demirci ve özel timi bizi burada ölüme terk etti ama ben B1-B4 Chronos Laboratuvar Anahtarı'nı (key_lab) ve yönetici katı şifrelerini sakladım! Al bu laboratuvar anahtarını, beni de 38-B'ye gönder!",
                    "Yönetici katı kasa şifrelerini iyi ezberle Aras: 39. kat Cemil'in kızı 1704, 41. kat Chronos kuruluşu 2611, 44. kat Volkan'ın süiti 4538!"
                },
                new[]
                {
                    "38-B'de güvendeyiz Aras. Çatıya çıkacaksan sırasıyla 39. kattan Bronz Kartı (1704), 41. kattan Gümüş Kartı (2611), 43. kattan Radar Kodunu ve 44. kattan Altın Kartı (4538) almalısın.",
                    "Bodruma ineceksen sana verdiğim Chronos Laboratuvar Anahtarı B1'deki zırhlı laboratuvar kapısını açar. B1-B4 arasındaki 5 kanıt terminalini sakın unutma.",
                    "Alt katlardaki eski mesai arkadaşlarım balistik yelekli. Onlara gövdeden kurşun işlemek çok zordur, tam kafaya nişan al."
                }));

            list.Add(new SurvivorDef(
                "tarik",
                "Tarık Aydın",
                -1,
                "vehicle_mechanic",
                "Ağır Vasıta & Zırhlı Araç Tamircisi",
                new[]
                {
                    "Aras usta! B1 otoparkına kadar inebildiysen bu kuleyi yıkıp geçeriz! Ben başmakinist Tarık. Önümüzdeki 12 tonluk Vanguard Zırhlı Müdahale Aracı'nın kontak anahtarını (key_vehicle) sana veriyorum! Bana bir Motor Parçası (B2'de var), bir Akü ve bir bidon Yakıt getir; aracı onarıp Zemin Kat'taki kaynaklı çelik kapıyı paramparça edelim!",
                    "Ben bakım kafesini ve telsiz hattını 38-B ile senkronize ediyorum. Parçaları bulup B1'deki zırhlı aracın başına geldiğinde motoru birlikte ateşleyeceğiz!"
                },
                new[]
                {
                    "Zırhlı Müdahale Aracı için gereken 4 şey: Ağır Vasıta Turbo Enjektör Bloğu (engine_part — B2 katında), Yüksek Kapasiteli Akü (battery), Bidon Yakıt (fuel) ve sana verdiğim Araç Anahtarı (key_vehicle).",
                    "Aracı B1'de onardıktan ve B4'teki Goliath'ı devirdikten sonra Zemin Kat (Kat 0) ana lobisine çık; oradaki çelik barikatı araçla yıkıp özgürlüğe çıkacağız!",
                    "Chronos'un 5 Kanıt Dosyasını da yanımıza alırsak dışarı çıktığımızda kimse bu olayı doğalgaz kazası diye örtbas edemez."
                }));
        }

        public static void PopulateObjectives(List<ObjectivePlacement> list)
        {
            list.Clear();

            // =================================================================
            // FLOOR 38 — Safe House (38-B) Vent Sealing
            // =================================================================
            list.Add(new ObjectivePlacement(
                "vent_sealed",
                38,
                "38. Kat Havalandırma Şaftını Mühürle (1 Hurda Metal, 1 Koli Bandı)",
                "vent",
                new[] { new ItemStack("scrap_metal", 1), new ItemStack("duct_tape", 1) },
                true,
                "38. kat havalandırma şaftı çelik sac ve koli bandıyla mühürlendi! Sis sızıntısı kesildi.",
                "",
                null,
                null,
                "arch_vent_01"));

            // =================================================================
            // ROUTE A — HELICOPTER (Floors 39, 41, 43, 44, 45)
            // =================================================================
            // Floor 39: Executive Safe (Code 1704 -> keycard_exec_1)
            list.Add(new ObjectivePlacement(
                "exec_keycard_1",
                39,
                "39. Kat Cemil Aethel Güvenlik Kasası (Şifre: 1704 — Defne'nin Doğum Günü)",
                "safe",
                new ItemStack[0],
                false,
                "Şifre 1704 doğrulandı! 1. Kademe Yönetici Kartı (Bronz) alındı.",
                "1704",
                new[] { new ItemStack("keycard_exec_1", 1) },
                null,
                "exe_safe_01"));

            // Floor 41: Chronos Executive Safe (Code 2611 + keycard_exec_1 -> keycard_exec_2)
            list.Add(new ObjectivePlacement(
                "exec_keycard_2",
                41,
                "41. Kat Chronos Yönetici Kasası (Şifre: 2611, Gerekli: Yönetici Kartı 1)",
                "safe",
                new[] { new ItemStack("keycard_exec_1", 1) },
                false,
                "Şifre 2611 ve Bronz Kart doğrulandı! 2. Kademe Yönetici Kartı (Gümüş) alındı.",
                "2611",
                new[] { new ItemStack("keycard_exec_2", 1) },
                null,
                "exe_safe_01"));

            // Floor 43: Ballroom Air Defense Archive Terminal (requires keycard_exec_2 -> grants radar_code)
            list.Add(new ObjectivePlacement(
                "radar_code_acquired",
                43,
                "43. Kat Hava Savunma Arşiv Terminali — UPLINK-7 Radar Kodunu İndir (Yönetici Kartı 2)",
                "terminal",
                new[] { new ItemStack("keycard_exec_2", 1) },
                false,
                "Hava Savunma Uplink Kodu (UPLINK-7) indirildi!",
                "",
                new[] { new ItemStack("radar_code", 1) },
                null,
                "off_monitor_01"));

            // Floor 44: Volkan Demirci Suite Safe (Code 4538 + keycard_exec_2 -> keycard_exec_3)
            list.Add(new ObjectivePlacement(
                "exec_keycard_3",
                44,
                "44. Kat Volkan Demirci Zırhlı Kasası (Şifre: 4538, Gerekli: Yönetici Kartı 2)",
                "safe",
                new[] { new ItemStack("keycard_exec_2", 1) },
                false,
                "Şifre 4538 doğrulandı! 3. Kademe Yönetici Kartı (Altın) alındı.",
                "4538",
                new[] { new ItemStack("keycard_exec_3", 1) },
                null,
                "exe_safe_01"));

            // Floor 44: Roof Access Security Panel (consumes keycard_exec_3 -> grants key_roof & raises roof_unlocked)
            list.Add(new ObjectivePlacement(
                "roof_unlocked",
                44,
                "45. Kat Çatı Helipad Güvenlik Paneli (Yönetici Kartı 3 Harcanır)",
                "console",
                new[] { new ItemStack("keycard_exec_3", 1) },
                true,
                "Çatı Helipad güvenlik kilidi açıldı ve Çatı Anahtarı (key_roof) alındı!",
                "",
                new[] { new ItemStack("key_roof", 1) },
                new[] { "exec_keycard_3" },
                "off_monitor_01"));

            // Floor 45: Roof Air Defense Radar Console (consumes 2 circuit + 2 wire -> radar_disabled)
            list.Add(new ObjectivePlacement(
                "radar_disabled",
                45,
                "Hava Savunma Radar Konsolunu Devre Dışı Bırak (2 Devre Kartı, 2 Bakır Kablo)",
                "console",
                new[] { new ItemStack("circuit", 2), new ItemStack("wire", 2) },
                true,
                "Çatı Hava Savunma Radarı kısa devre yaptırılarak devre dışı bırakıldı!",
                "",
                null,
                null,
                "exe_radar_01"));

            // Floor 45: Helicopter Uplink Terminal (requires radar_disabled, consumes radar_code -> signal_sent)
            list.Add(new ObjectivePlacement(
                "signal_sent",
                45,
                "Helipad Uplink Terminali — Kartal-3 Kurtarma Helikopterine Sinyal Gönder (UPLINK-7)",
                "terminal",
                new[] { new ItemStack("radar_code", 1) },
                true,
                "UPLINK-7 kurtarma sinyali gönderildi! Helikopter inene kadar çatı pistini sürüye karşı savun!",
                "",
                null,
                new[] { "radar_disabled" },
                "off_monitor_01"));

            // =================================================================
            // ROUTE B — ARMORED VEHICLE & CHRONOS LABS (Floors 8, -1, -2, -3, -4, 0)
            // =================================================================
            // Floor 8: Murat's Security Locker (grants key_lab if player needs it)
            list.Add(new ObjectivePlacement(
                "murat_locker_opened",
                8,
                "Murat Şen'in Güvenlik Dolabı — Chronos Laboratuvar Anahtarı'nı Al",
                "locker",
                new ItemStack[0],
                false,
                "Chronos Laboratuvar Anahtarı (key_lab) ve yedek mühimmat alındı!",
                "",
                new[] { new ItemStack("key_lab", 1), new ItemStack("ammo_9mm", 12) },
                new[] { "murat_rescued" },
                "off_cabinet_01"));

            // Floor -1 (B1): Chronos Lab Blast Door (requires key_lab -> lab_unlocked)
            list.Add(new ObjectivePlacement(
                "lab_unlocked",
                -1,
                "Chronos Bio-Labs B1-B4 Zırhlı Giriş Kapısı (Chronos Laboratuvar Anahtarı)",
                "console",
                new[] { new ItemStack("key_lab", 1) },
                false,
                "Chronos Bio-Labs yüksek güvenlikli kapı kilidi açıldı!",
                "",
                null,
                null,
                "off_monitor_01"));

            // Floor -1 (B1): Evidence Terminal #1
            list.Add(new ObjectivePlacement(
                "evidence_collected_1",
                -1,
                "B1 Güvenlik Terminali — Chronos Kanıt Dosyası #1'i İndir (Sevk Kayıtları)",
                "terminal",
                new ItemStack[0],
                false,
                "Chronos Kanıt Dosyası #1 (Kaçak Denek Sevk Kayıtları) indirildi!",
                "",
                new[] { new ItemStack("evidence_1", 1) },
                new[] { "lab_unlocked" },
                "lab_server_01"));

            // Floor -1 (B1): Vanguard Armored Response Vehicle Repair
            list.Add(new ObjectivePlacement(
                "vehicle_repaired",
                -1,
                "Vanguard Zırhlı Müdahale Aracı'nı Onar (Motor Parçası, Akü, Yakıt, Araç Anahtarı)",
                "vehicle",
                new[]
                {
                    new ItemStack("engine_part", 1),
                    new ItemStack("battery", 1),
                    new ItemStack("fuel", 1),
                    new ItemStack("key_vehicle", 1)
                },
                true,
                "Vanguard Zırhlı Müdahale Aracı onarıldı ve çalıştırıldı! Zemin kattaki (Kat 0) çelik barikatı yıkmaya hazır!",
                "",
                null,
                new[] { "tarik_rescued" },
                "veh_armored_response_01"));

            // Floor -2 (B2): Evidence Terminal #2 + Engine Part crate
            list.Add(new ObjectivePlacement(
                "evidence_collected_2",
                -2,
                "B2 Jeneratör Kontrol Arşivi — Kanıt Dosyası #2 ve Yedek Motor Parçasını Al",
                "terminal",
                new ItemStack[0],
                false,
                "Chronos Kanıt Dosyası #2 (Havalandırma Raporu) ve Ağır Vasıta Motor Parçası alındı!",
                "",
                new[] { new ItemStack("evidence_2", 1), new ItemStack("engine_part", 1) },
                null,
                "lab_server_01"));

            // Floor -3 (B3): Evidence Terminal #3 (Denek-0 Logs)
            list.Add(new ObjectivePlacement(
                "evidence_collected_3",
                -3,
                "B3 Karantina Terminali — Chronos Kanıt Dosyası #3'ü İndir (Denek-0 Raporu)",
                "terminal",
                new ItemStack[0],
                false,
                "Chronos Kanıt Dosyası #3 (Denek-0 Hasan Bozkurt Klinik Raporu) indirildi!",
                "",
                new[] { new ItemStack("evidence_3", 1) },
                null,
                "lab_server_01"));

            // Floor -3 (B3): Evidence Terminal #4 (Dr. Leyla Aksoy's Drive)
            list.Add(new ObjectivePlacement(
                "evidence_collected_4",
                -3,
                "Dr. Leyla Aksoy'un Araştırma Masası — Kanıt Dosyası #4'ü Al (Şifreli USB Disk)",
                "desk",
                new ItemStack[0],
                false,
                "Chronos Kanıt Dosyası #4 (Dr. Leyla Aksoy'un Şifreli Sürücüsü) alındı!",
                "",
                new[] { new ItemStack("evidence_4", 1), new ItemStack("mutagen_sample", 1) },
                null,
                "off_desk_01"));

            // Floor -4 (B4): Evidence Terminal #5 (Project Lazarus Mainframe)
            list.Add(new ObjectivePlacement(
                "evidence_collected_5",
                -4,
                "B4 Project Lazarus Ana Sunucusu — Chronos Kanıt Dosyası #5'i İndir",
                "terminal",
                new ItemStack[0],
                false,
                "Chronos Kanıt Dosyası #5 (Lazarus & KAPAK Protokolü Ana Verisi) indirildi!",
                "",
                new[] { new ItemStack("evidence_5", 1) },
                null,
                "lab_server_01"));

            // Floor 0 (Ground Lobby): Armored Vehicle Breakout through welded steel barricades
            list.Add(new ObjectivePlacement(
                "vehicle_breakout",
                0,
                "Zırhlı Müdahale Aracıyla Mühürlü Zemin Kat Çelik Barikatını Yık ve Kule 45'ten Kaç!",
                "vehicle",
                new ItemStack[0],
                false,
                "12 tonluk Zırhlı Müdahale Aracı kaynaklı çelik plakaları parçalayarak Kule 45'in mührünü kırdı!",
                "",
                null,
                new[] { "vehicle_repaired" },
                "veh_armored_response_01"));
        }
    }
}
