// 45: Layered Hell — StoryJournals_Part1: 32 deep Turkish journals for floors 45 down to 20.
using System.Collections.Generic;

namespace LayeredHell
{
    internal static class StoryJournals_Part1
    {
        public static void Populate(List<JournalEntry> list)
        {
            // Floor 45 — Çatı Helipad & Radar
            list.Add(new JournalEntry(
                "jrn_45_pilot_log",
                45,
                "Helipad Uçuş ve Acil Tahliye Kaydı",
                "Kaptan Pilot Emre Soylu",
                "13 Kasım 2026 — 23:45\nFırtına şiddetleniyor. Yönetim Kurulu Başkanı Cemil Aethel'in özel helikopterini 45. kat pistine bağladık ama Volkan Demirci az önce garip bir talimat gönderdi: “Gece yarısından itibaren çatıdaki otomatik hava savunma ve hedef kilitleme radarı AKTİF konuma geçirilecek. UPLINK-7 doğrulama kodu girilmeden yaklaşan her hava aracı düşman kabul edilecektir.”\n\nBurası sivil bir rezidans kulesi! Neden çatıya askeri frekans karıştırıcı ve hedefleme radarı kurulduğunu kimse sorgulamıyor mu? Eğer içeride bir şey olursa ve o radar konsolundaki iki ana devre kartı ile bakır kablolar sökülüp devre dışı bırakılmazsa, dışarıdan hiçbir kurtarma helikopteri bu çatıya inemez."));

            // Floor 44 — Yönetici Süiti & Güvenlik Merkezi
            list.Add(new JournalEntry(
                "jrn_44_volkan_orders",
                44,
                "Vanguard Özel Güvenlik — Kırmızı Protokol",
                "Güvenlik Şefi Volkan Demirci",
                "14 Kasım 2026 — 00:18\nB4 laboratuvar katından gelen basınç patlaması tüm asansör şaftlarını sarstı. Prof. Orhan Vural telefonda bağırıyor; “Denek-0 kafesi parçaladı, LZR-9 gazı havalandırmaya karıştı!” diyor.\n\nAlbay Sinan Erdem ile doğrudan görüştüm. Ordu “KAPAK” harekâtını başlattı. Zemin katı dışarıdan çelik plakalarla kaynaklayıp mühürlüyorlar. Bizimkileri alt katlara indirdim ama telsizden gelen sesler insan sesine benzemiyor. Çatı kapısının 3. Kademe Altın Yönetici Kartı'nı süitimdeki zırhlı kasaya kilitledim. Şifreyi unutursam diye her zamanki kuralımı yazdım: önce bu lanet kulenin toplam kat sayısı, hemen yanına da eski arama-kurtarma teknisyeni Aras'ın kurduğu güvenli sığınak katı (45 ve 38). Çatıya yalnızca ben ve yönetim çıkacağız."));

            list.Add(new JournalEntry(
                "jrn_44_baran_memo",
                44,
                "TASLAK: Kamuoyu Basın Açıklaması (GİZLİ)",
                "Chronos Bio-Labs CEO'su Baran Tuncel",
                "BASINA VERİLECEK RESMÎ METİN:\n“Aethel-Vanguard Mega Rezidansı'nda 13 Kasım Cuma gecesi meydana gelen doğalgaz sıkışması kaynaklı patlama kontrol altına alınmış olup, bina sakinlerinin tamamı güvenli şekilde tahliye edilmiştir.”\n\nGERÇEK DURUM NOTU (YALNIZCA YÖNETİM KURULUNA):\nKimse tahliye edilmedi. İçeride üç binden fazla insan var. Eğer tek bir enfekte bile dışarı çıkarsa veya Dr. Leyla Aksoy'un B3'te topladığı Project Lazarus dosyaları basına sızarsa hepimiz müebbet yeriz. Havalandırma şaftlarından yükselen “Sis” üst katlara ulaştı bile. Çatıya çıkıp UPLINK-7 koduyla özel tahliye istemek zorundayım."));

            // Floor 43 — Balo Salonu (Üst)
            list.Add(new JournalEntry(
                "jrn_43_gala_blood",
                43,
                "Sonbahar Yatırımcı Galası — Yarım Kalan Konuşma",
                "Etkinlik Koordinatörü Ceyda Karan",
                "Saat tam 00:00'da şampanya kadehleri kalkarken alttan boğuk bir gümleme geldi. Önce deprem sandık. Sonra havalandırma menfezlerinden tatlımsı, metalik kokulu grimsi bir buhar — çalışanların fısıldayarak “Sis” dediği o şey — salona dolmaya başladı.\n\nOn dakika içinde garsonlardan ikisi kan kusarak yere yığıldı. Doktor müdahale etmek için eğildiğinde garson doktorun şahdamarını dişleriyle kopardı. İnsanlar abiye kıyafetlerle, smokinlerle birbirini ezerek merdivenlere koştu. Ben 43. kattaki hava savunma arşiv odasının arkasına saklandım. İçerideki terminalde “UPLINK-7” yazan mavi bir veri kartı var ama 2. Kademe Gümüş Yönetici Kartı olmadan kilit açılmıyor."));

            // Floor 42 — Balo Salonu (Alt)
            list.Add(new JournalEntry(
                "jrn_42_musician_diary",
                42,
                "Keman Kutusu İçindeki Kanlı Nota Kâğıdı",
                "Çellist Hakan Yurdakul",
                "Orkestra çukurunun altına gizlendim. Yukarıda, kristal avizelerin altında dün gece vals yapan insanlar şimdi hırıltılar çıkararak salonu arşınlıyor. Gözbebekleri tamamen bulanık, damarları boyunlarından siyah çizgiler halinde fırlamış.\n\nYanımdaki kemancı arkadaşım Melis kolundan ısırılmıştı. İlk bir saat sadece titredi ve ateşlendi. İkinci saatin sonunda bana bakıp “Hakan, içimde bir şey yanıyor, beni bağla” dedi. Onu nota sehpalarının kablolarıyla bağladım. Şimdi karanlıkta dişlerini gıcırdatarak yüzüme atılmaya çalışıyor. Özür dilerim Melis."));

            // Floor 41 — Yönetici Ofisleri
            list.Add(new JournalEntry(
                "jrn_41_chronos_charter",
                41,
                "Chronos Bio-Labs Kuruluş Yıldönümü Plaketi ve Kasa Notu",
                "Yönetici Asistanı Burcu Tekin",
                "Sayın Baran Tuncel'in dikkatine:\n41. kattaki yüksek güvenlikli yönetici kasasının şifresi, şirketimiz Chronos Bio-Labs'ın kuruluş günü olan “26 Kasım” (26.11.2011) tarihinin gün ve ay haneleri olarak (GGAA formatında: 2611) güncellenmiştir.\n\nNot: Kasayı açabilmek için önce 39. kattaki Cemil Aethel'in ofisinden 1. Kademe Bronz Yönetici Kartı'nın okutulması gerekmektedir. Kasanın içinde 43. ve 44. kat sistemlerini açan 2. Kademe Gümüş Yönetici Kartı muhafaza edilmektedir."));

            // Floor 40 — Yönetici Ofisleri
            list.Add(new JournalEntry(
                "jrn_40_audit_warning",
                40,
                "İç Denetim Raporu: B2-B4 Elektrik ve Gaz Sarfiyatı",
                "Başdenetçi Haluk Ergin",
                "Son altı aydır “Aethel Tıbbi Araştırma Eki” adıyla Chronos Bio-Labs'a kiralanan B2, B3 ve B4 bodrum katları, kulenin geri kalan 45 katının toplamından daha fazla elektrik tüketiyor.\n\nAyrıca B2 jeneratör katından doğrudan ana havalandırma bacasına bağlanan kaçak basınç valfleri tespit ettim. Raporu Cemil Aethel'e sunduğumda yüzüme bile bakmadan kâğıdı yırtıp attı. “Devlet üstü bir askeri rejenerasyon projesi yürütüyoruz Haluk, burnunu sokma” dedi. Şimdi o kaçak valflerden bütün kuleye ölüm pompalanıyor."));

            // Floor 39 — Yönetici Ofisleri
            list.Add(new JournalEntry(
                "jrn_39_cemil_birthday_clue",
                39,
                "Yönetim Kurulu Başkanı Cemil Aethel'in Masa Takvimi",
                "Cemil Aethel",
                "Masa üzerindeki deri çerçeveli fotoğrafın arkasına el yazısıyla düşülmüş not:\n“Canım kızım Defne'm, 17 Nisan doğumlu bahar çiçeğim... Babasının en kıymetli sırrı. 39. kattaki özel güvenlik kasamın dört haneli şifresini yine senin doğum günün ve ayın yaptım (17 Nisan -> 1704). İçindeki 1. Kademe Yönetici Kartı'nı benden başkası alamaz.”\n\nSayfanın alt kısmında aceleyle karalanmış titrek bir yazı daha var:\n“Tanrım, koridordaki korumalarım birbirini parçalıyor. Kasanın yanına kadar süründüm ama parmaklarım tutmuyor...”"));

            // Floor 38 — Güvenli Ev (Daire 38-B)
            list.Add(new JournalEntry(
                "jrn_38_aras_manifesto",
                38,
                "Aras Kaya'nın Saha Defteri — 38-B Güvenli Ev Planı",
                "Aras Kaya (38-B)",
                "14 Kasım 2026 — 01:10\nGece yarısı kulenin temelinden gelen o sarsıntıyı eski bir arama-kurtarma teknisyeni olarak nerede duysam tanırım: bu bir doğalgaz patlaması değil, yüksek basınçlı bir tank yırtılmasıydı.\n\nKardeşim Deniz'i aradım ama şebeke tamamen kesik. Dışarıya baktığımda ordunun zemin katı kaynak makineleriyle çelik plakalarla kapattığını gördüm. Bizi içeriye gömdüler.\n\n38-B dairemi merkez üs haline getirdim. Balkona yağmur suyu arıtıcısı kurabilirim, tezgâhta hurdadan zırh ve barikat malzemesi üretebilirim. Telsizi onarırsam kulede hayatta kalan uzmanları — 31. kattaki Dr. Selin'i, 24. kattaki Kemal Usta'yı, 19. kattaki kimyager Elif'i, 27. kattaki telsizci Nehir'i — buraya toplayabilirim. Ya çatıdaki radarı susturup helikopter çağıracağız ya da bodrumdaki zırhlı araçla zemin katı yarıp çıkacağız. Burada ölmeyeceğim."));

            list.Add(new JournalEntry(
                "jrn_38_neighbor_note",
                38,
                "38-B Kapı Altından Atılan Not",
                "Komşu Melahat Teyze (38-A)",
                "Aras oğlum,\nSen arama-kurtarmacısın, elinden her iş gelir. Saatlerdir koridordan hırıltılar geliyor, kapı dürbününden baktım; karşı dairedeki avukat bey pijamaları kan içinde duvarları tırmalıyor. Eşim Rıza'nın ateşi çıktı, öksürürken ağzından siyah kan geldi. Oğlum, eğer içeriden kilit sesi duyarsan sakın 38-A'yı açma. Yedek konserve ve sargı bezlerini kapının yanındaki dolaba bıraktım. Kardeşin Deniz'e kavuş, bizi düşünme."));

            // Floor 37 — Konut Katı
            list.Add(new JournalEntry(
                "jrn_37_quarantine_sms",
                37,
                "Gönderilemeyen Mesaj Taslakları",
                "Mert Soydan (Daire 37-C)",
                "[00:14] Anne, kulede bir patlama oldu ama iyiyim, merak etme.\n[00:42] Mesajlar gitmiyor. Elektrikler kesildi, koridor acil durum lambaları kırmızı yanıyor. Havalandırmadan garip bir koku geliyor, ıslak bezle menfezi tıkadım.\n[01:35] Alt kattan çığlıklar ve silah sesleri geliyor. Dışarıda askeri helikopterler projektör tutuyor ama kimseyi almıyorlar. Neden yardım etmiyorlar?\n[02:19] Kapıyı kırıyorlar. Anne seni çok seviyorum."));

            // Floor 36 — Konut Katı
            list.Add(new JournalEntry(
                "jrn_36_vent_warning",
                36,
                "Apartman Teknik Bakım Uyarısı",
                "Saha Teknisyeni Vedat",
                "35 ile 39. katlar arasındaki tüm sakinlerin dikkatine!\nSabaha karşı havalandırma şaftlarından yükselen morumsu aerosol gazı (“Sis”) doğrudan solunum yollarını yakmaktadır. Menfez kapaklarını koli bandı ve metal saclarla mühürlemeden uyumayın!\n\nGazı soluyanlarda 45 dakika içinde şiddetli saldırganlık, göz içi kanama ve ağrı duyusunun tamamen kaybolması görülüyor. Isırılan birini içeri almayın; dönüşüm geri döndürülemez!"));

            // Floor 35 — Konut Katı
            list.Add(new JournalEntry(
                "jrn_35_child_drawing",
                35,
                "Boya Kalemiyle Yazılmış Çocuk Defteri",
                "Elifsu (8 Yaşında, Daire 35-F)",
                "Bugün Cuma. Dışarıda çok yağmur yağıyor. Gece babamla film izliyorduk, sonra yer sallandı. Babam koridora bakmaya gitti ama geri gelmedi.\n\nAnnem beni elbise dolabının içine sakladı, “Elifsu, kim çağırırsa çağırsın, babamın sesi bile olsa dolaptan çıkma” dedi. Şimdi kapının önünde biri babamın sesiyle hırlıyor ama kelimeleri söyleyemiyor, sadece camları yumrukluyor. Kulaklarımı kapattım."));

            // Floor 34 — Spor Salonu (Gym)
            list.Add(new JournalEntry(
                "jrn_34_gym_holdout",
                34,
                "Apex Fitness & Gym — Gece Nöbeti Kaydı",
                "Antrenör Bora Çetin",
                "Gece antrenmanında salonda sekiz kişiydik. Asansör kapısı açıldığında içinden çıkan güvenlik görevlisinin boğazı parçalanmıştı; üstümüze atıldı. Halter barları ve beyzbol sopalarıyla kendimizi savunduk ama buradaki enfekteler üst katlardaki uyuşuk sakinlere benzemiyor!\n\nBazılarının kasları mutajen yüzünden gerilmiş, en ufak bir metal sesine veya koşu adımına saniyesinde tepki verip deli gibi koşuyorlar. Soyunma odasındaki dolaplara izotonik içecekleri, adrenalin şırıngalarını ve protein barlarını yığdım. Ses çıkarmadan hareket edin; koşarsanız sizi bulurlar."));

            // Floor 33 — Konut Katı
            list.Add(new JournalEntry(
                "jrn_33_selin_mother",
                33,
                "Kızım Selin'e Son Mektup",
                "Nermin Arslan (Daire 33-B)",
                "Canım kızım Selin,\n31. kattaki polikliniğinde nöbette olduğunu biliyorum. Sakın beni kurtarmak için 33. kata çıkmaya çalışma yavrum. Balkon kapısının camı kırıldı ve o gri sis içeri doldu; ciğerlerim yanıyor, ellerim titriyor.\n\nSen cerrahsın, bu kuledeki insanların sana ve geliştireceğin panzehire ihtiyacı var. Masanın üzerinde babanın eski saati ve senin mezuniyet fotoğrafın duruyor. Onlara bakarak gözlerimi kapatıyorum. Kapıyı içeriden kilitledim ki dönüştüğümde kimseye zarar veremeyeyim. Hakkım sana helal olsun."));

            // Floor 32 — Konut Katı
            list.Add(new JournalEntry(
                "jrn_32_hoarder_note",
                32,
                "Kilitli Kiler Kapısındaki İkaz",
                "Emekli Albay Rıfat (Daire 32-D)",
                "Bu dairede 40 kutu konserve ve 12 şişe temiz su var diye kapıma dayanan yağmacılara sesleniyorum: elimde Kobra .357 toplu tabanca var ve kapıdan geçen ilk kafaya tereddütsüz sıkarım.\n\nEK NOT (Saat 03:40): Silah sesi hata oldu. Tek bir .357 patlaması bütün alt koridorlardaki koşucuları kapıma topladı. Mermim bitti. Kileri açabilen hayatta kalan kimse, malzemeleri alsın ve tabancanın sesini mecbur kalmadıkça kullanmasın."));

            // Floor 31 — Arslan Polikliniği (Dr. Selin)
            list.Add(new JournalEntry(
                "jrn_31_selin_research",
                31,
                "Klinik Gözlem Raporu: LZR-9 Mutajenik Sendromu",
                "Op. Dr. Selin Arslan",
                "14 Kasım 2026 — 02:05 (31. Kat Arslan Polikliniği)\nGece yarısından beri kliniğe getirilen 14 hastanın tamamında aynı tablo var: kornea çevresinde mor halkalar, vücut ısısında 41.5 dereceye varan ani sıçrama ve limbik sistemin tamamen kontrolden çıkması.\n\nKan örneklerini santrifüjden geçirdiğimde doğal bir virüs değil, sentetik bir retro-protein zinciri buldum. Numune tüplerinin birinde Chronos Bio-Labs'ın “LZR-9” kodu vardı! İyi haber şu: dondurulmuş LZR-9 örneği, geniş spektrumlu antibiyotik ve kimyasal çözücüyü doğru oranda sentezlersem kandaki mutajeni parçalayan bir Panzehir Serumu (Antidote) üretebilirim. Ama buradaki sedyeler devrildi, koridor enfekte kaynıyor. Güvenli bir üsse ve tıbbi istasyona ihtiyacım var."));

            list.Add(new JournalEntry(
                "jrn_31_nurse_triage",
                31,
                "Acil Triyaj Defteri — Poliklinik Kayıtları",
                "Başhemşire Sevgi Duman",
                "00:22 — Hasta #1 (Güvenlik Görevlisi): B1 katından kaçarken kolundan ısırılmış. Sakinleştirici işe yaramadı, sedye kayışlarını kopardı.\n00:50 — Hasta #4 (29. Kat sakini): Havalandırma gazına maruz kalmış. Geniş spektrumlu antibiyotik dönüşümü yaklaşık 2 saat yavaşlattı ancak durdurmadı.\n01:30 — Dr. Selin kendini steril ameliyathane bölmesine kilitledi. Ben ilaç deposunun anahtarını çekmeceye sakladım. Eğer 38. kattaki o arama-kurtarmacı Aras telsiz anonsunu duyup gelirse, önce Dr. Selin'i buradan çıkarmalı!"));

            // Floor 30 — Konut Katı
            list.Add(new JournalEntry(
                "jrn_30_elevator_trap",
                30,
                "Asansör Boşluğundan Gelen Sesler",
                "Site Yöneticisi Kemalettin",
                "SAKINA ANA ASANSÖRLERİ KULLANMAYIN!\nB4 katındaki patlamadan sonra asansör kabinlerinden ikisi zemin kata çakıldı, şaftlar ise LZR-9 gazının ana bacası haline geldi. Sadece teknik servis asansörleri ve yangın merdivenleri güvenli, onlar da jeneratör gücü olmadan yarı kapasite çalışıyor.\n\n38. kattaki Aras Kaya'nın daire kapısını güçlendirdiğini duydum. Yukarı çıkabilen herkes 38-B'ye sığınsın."));

            // Floor 29 — Konut Katı
            list.Add(new JournalEntry(
                "jrn_29_couple_pact",
                29,
                "Balkon Masasındaki İki Kahve Fincanı",
                "Zeynep & Caner (Daire 29-A)",
                "Yağmur balkon demirlerine vuruyor. Karşıdaki gökdelenlerin ışıkları yanıyor, otoyoldan arabalar geçiyor... Şehir hayatına normal şekilde devam ederken sadece yüz metre ötede, bu kulenin içinde kıyamet kopuyor.\n\nCaner'in bacağındaki ısırık morarmaya başladı. Bana “Zeynep, merdivenlerden 27. kata in, oradaki telsizci kız Nehir dışarıyla bağlantı kurmaya çalışıyor” dedi. Onu yalnız bırakmak istemiyorum ama gözlerindeki bakış değişmeye başladı."));

            // Floor 28 — Konut Katı
            list.Add(new JournalEntry(
                "jrn_28_plumbing_contamination",
                28,
                "Şebeke Suyu Kesintisi ve Balkon Arıtması",
                "Tesisat Ustası İlyas",
                "Ordu binayı mühürledikten hemen sonra ana şehir şebeke vanasını kapattı. Borularda kalan durgun su ise B2 katındaki sızıntı yüzünden paslı ve bulanık akıyor; arıtmadan içenlerde mide krampı ve enfeksiyon belirtisi başlıyor.\n\n38. kattaki Aras'a geçen hafta balkon için karbon filtreli bir yağmur suyu toplama düzeneği vermiştim. Bu fırtınada temiz su bulmanın tek yolu o arıtıcıyı çalıştırmak ya da suyu ocakta kaynatmak."));

            // Floor 27 — Konut Katı (Nehir Özkan - 27-D)
            list.Add(new JournalEntry(
                "jrn_27_nehir_freq_log",
                27,
                "88.7 Kule Radyosu — Frekans Tarama Günlüğü",
                "Nehir Özkan (Daire 27-D)",
                "14 Kasım 2026 — 01:45\nAmatör telsizimle bütün bantları taradım. Ordu kulenin etrafına sinyal bozucu yerleştirdiği için cep telefonları ve internet tamamen ölü. Ancak FM 88.7 MHz üzerinden kısa dalga kırılmasıyla hem kule içi güvenlik telsizlerini hem de dışarıdaki Kule Karantina Komutanlığı'nın şifresiz konuşmalarını yakalayabiliyorum!\n\nDışarıdaki komutan Albay Sinan Erdem'in sesini bizzat kaydettim: “Sabah 06:00'a kadar sızıntı durmazsa termobarik temizlik protokolü uygulanacak” diyor! Bunu herkese duyurmam lazım ama 27-D'nin kapısına dayanmaya başladılar. Yedek transistörlerimi çantama koydum; biri beni 38. kattaki güvenli eve çıkarırsa oradaki büyük anten hattına bağlanıp gerçek bir radyo istasyonu kurabilirim!"));

            // Floor 26 — Konut Katı
            list.Add(new JournalEntry(
                "jrn_26_runner_behavior",
                26,
                "Gözlem Notu: Koşucular ve Tükürücüler",
                "Avcı Mehmet (Daire 26-C)",
                "25. katın altına indikçe enfektelerin türü değişiyor. Üst katlardaki sakinler yeni dönüştüğü için yavaş ve hantal yürüyüşçüler (Walker). Ama orta katlarda, özellikle ofis ve AVM katlarından yukarı tırmananlar çok daha uzun süre gaza maruz kalmış:\n\n1. KOŞUCULAR (Runner): Görme duyuları zayıf ama işitme duyuları korkunç. Koşarsanız veya tabanca ateşlerseniz iki koridor öteden üstünüze çullanıyorlar. Çömelerek yürümek hayat kurtarır.\n2. TÜKÜRÜCÜLER (Spitter): Boyunlarında yeşilimsi asit keseleri şişmiş. Uzaktan yakıcı mutajen tükürüyorlar; Elif'in asit korumalı zırhı olmadan yaklaşmayın."));

            // Floor 25 — Konut / Ofis Geçiş Katı
            list.Add(new JournalEntry(
                "jrn_25_transition_gate",
                25,
                "Konut-Ofis Ara Kapısı Güvenlik Kaydı",
                "Nöbetçi Memur Serkan",
                "Saat 00:55 itibarıyla 24. kat ve altındaki ticari ofis katlarından konut katlarına kaçan kalabalığı durdurmaya çalıştık. İçlerinde ısırılmış olanlar vardı! Kemal Usta 24. kattaki silah atölyesinin kepengini indirip içeriden barikat kurdu; bize de bir avuç 12 kalibre fişek verdi.\n\n“Çocuklar, gövdelerine sıkarak mermi harcamayın, omurilik bağlantısı kopmadan düşmüyorlar; sadece kafaya nişan alın!” diye bağırdı. Haklıymış."));

            // Floor 24 — Silah Atölyesi (Kemal Usta)
            list.Add(new JournalEntry(
                "jrn_24_kemal_ledger",
                24,
                "Usta Silah & Tamir — Kemal Usta'nın Tezgâh Defteri",
                "Kemal Çelik (Kemal Usta)",
                "14 Kasım 2026 — 02:10\nRahmetli eşim Fatma hep “Kemal, bu yaşta gökdelenin 24. katında silah tamir atölyesi mi olur, kapat artık şu dükkânı” derdi. Meğer bu gece içinmiş Fatma'm...\n\nElimde barut, boş pirinç kovan, kurşun ve çelik plakalar var. 9mm'den 7.62 keskin nişancı fişeğine kadar her türlü mühimmatı kendi presimde basarım; karbon çelikten pala bilerim, tabanca ve tüfeklerin namlu-yay takımlarını modifiye ederim. Ama atölyenin havalandırmasından asit tüküren o ucubeler sızmaya başladı. Tek başıma burada sabaha çıkamam. 38-B'deki Aras evladım beni buradan alırsa, bütün alet çantamı onun tezgâhına taşırım."));

            list.Add(new JournalEntry(
                "jrn_24_gun_mods_guide",
                24,
                "Silah Bakım ve Modifikasyon Kılavuzu",
                "Kemal Usta",
                "Kulede bulacağınız silahlar hakkında kısa notlarım:\n- Yavuz 9mm & Fırtına MP-9: Aynı 9mm mermiyi yakar. Koşuculara karşı MP-9 hayat kurtarır ama mermiyi su gibi içer.\n- Atmaca 12G Pompalı: Dar kapı ağzında tek atışla işi bitirir.\n- Bora 5.56 & Karayel 7.62: Alt katlardaki balistik yelekli Vanguard güvenlik zombilerini devirmek için şarttır.\n- Silah Modifikasyon Kiti: Hassas silah parçaları, çelik plaka ve epoksi getirirseniz silahınızın hasarını artırıp geri tepmesini düşürebilirim."));

            // Floor 23 — Ofis Katı
            list.Add(new JournalEntry(
                "jrn_23_hr_coverup",
                23,
                "Aethel-Vanguard İnsan Kaynakları: “Gönüllü” Sözleşmeleri",
                "İK Direktörü Pelin Sazak",
                "B4 katındaki Chronos Bio-Labs için işe aldığımız “Tıbbi Gözlem Gönüllüleri” dosyasını incelerken kanım dondu. Başta 41 yaşındaki Hasan Bozkurt olmak üzere toplam 19 ağır hastaya “ailenize 5 milyon TL nakit destek ve yeni nesil hücresel tedavi” vaadiyle feragatname imzalatılmış.\n\nHasan Bozkurt'un dosyasının üzerine kırmızı mühürle “DENEK-0: LZR-9 DOZ 9 — ÖLÜMSÜZ ASKER PROTOTİPİ” yazılmış. Bu insanlar tedavi edilmemiş; kas ve kemik dokularını çelik kadar sertleştiren bir biyolojik silaha dönüştürülmüş!"));

            // Floor 22 — Ofis Katı
            list.Add(new JournalEntry(
                "jrn_22_it_firewall",
                22,
                "Bilgi İşlem (IT) Sunucu Odası Logu",
                "Sistem Yöneticisi Tolga",
                "[00:01:12] UYARI: B4 Ana Sunucusu ile 44. Kat Güvenlik Süiti arasında yüksek hacimli şifreli veri aktarımı.\n[00:03:45] Dr. Leyla Aksoy (Kimlik #CHR-08) B3 terminalinden 5 parçalık “Project_Lazarus_Kanit” paketini yerel terminallere yedekledi.\n[00:05:10] Güvenlik Şefi Volkan Demirci tüm dış ağ geçitlerini fiziksel olarak kesti.\n\nNot: Leyla Hanım eğer hala hayattaysa B1 ile B4 arasındaki 5 kanıt terminalindeki verileri birinin toplayıp kule dışına çıkarması gerekiyor. Yoksa Chronos bu katliamı doğalgaz kazası diye kapatacak!"));

            // Floor 21 — Ofis Katı
            list.Add(new JournalEntry(
                "jrn_21_night_shift_coder",
                21,
                "Acil Çıkış Merdiveninde Bulunan Kırık Tablet",
                "Yazılımcı Onur",
                "Cuma gecesi mesaiye kaldığım güne lanet olsun. Masanın altına saklandım. Koridorda bir zamanlar muhasebe müdürü olan Bülent Bey dolaşıyor ama boynu 45 derece yana kırık ve boğazından yeşil, dumanı tüten bir sıvı damlıyor.\n\nAz önce cam bölmenin arkasından beni gördü ve ağzından fırlattığı asitli salgı kurşun geçirmez ofis camını saniyeler içinde eritti! Yangın merdivenine zor attım kendimi. 19. kattaki kimyager Elif'in laboratuvarında bu aside dayanıklı polimer zırh malzemesi olduğunu duymuştum."));

            // Floor 20 — Ofis Katı
            list.Add(new JournalEntry(
                "jrn_20_legal_shredder",
                20,
                "Hukuk Müşavirliği — Evrak İmha Makinesinden Kurtarılan Sayfa",
                "Av. Selim Kunter",
                "...işbu gizlilik protokolü uyarınca, Aethel-Vanguard Holding ve alt yüklenicisi Chronos Bio-Labs tarafından B2-B4 katlarında yürütülen “Project Lazarus” kapsamında olası bir biyo-güvenlik ihlali yaşanması halinde, Kule Karantina Komutanlığı (Harekât Kodu: KAPAK) binayı 60 dakika içinde dışarıdan mühürleme ve çatı hava savunma radarını otomatik kilitleme yetkisine sahiptir...\n\n(Sayfanın kenarına tükenmez kalemle yazılmış): Bizi en başından beri gözden çıkarmışlar. Sözleşmeyi kendi ellerimle hazırladım, kendi mezar fermanımı yazmışım."));
        }
    }
}
