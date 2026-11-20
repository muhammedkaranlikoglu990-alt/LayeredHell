// 45: Layered Hell — QuestManager: main & branching story quests (Helicopter Route A & Armored Vehicle Route B) + side quests.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    [Serializable]
    public class QuestStep
    {
        public string key;
        public string text;
        public int count;
        public int progress;

        public bool IsDone => progress >= count;

        public QuestStep() { }

        public QuestStep(string key, string text, int count = 1)
        {
            this.key = key;
            this.text = text;
            this.count = Mathf.Max(1, count);
            this.progress = 0;
        }
    }

    [Serializable]
    public class QuestDef
    {
        public string id;
        public string title;
        public string description;
        public QuestStep[] steps;
        public bool main;

        public QuestDef() { }

        public QuestDef(string id, string title, string description, QuestStep[] steps, bool main = true)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.steps = steps ?? new QuestStep[0];
            this.main = main;
        }

        public bool IsAllStepsDone()
        {
            if (steps == null || steps.Length == 0) return true;
            for (int i = 0; i < steps.Length; i++)
            {
                if (!steps[i].IsDone) return false;
            }
            return true;
        }

        public QuestStep GetCurrentStep()
        {
            if (steps == null) return null;
            for (int i = 0; i < steps.Length; i++)
            {
                if (!steps[i].IsDone) return steps[i];
            }
            return null;
        }
    }

    public class QuestManager : MonoBehaviour, ISaveable
    {
        public static QuestManager Instance { get; private set; }

        private readonly List<QuestDef> _allQuests = new List<QuestDef>();
        private readonly List<QuestDef> _active = new List<QuestDef>();
        private readonly List<QuestDef> _completed = new List<QuestDef>();
        private readonly HashSet<string> _flags = new HashSet<string>(StringComparer.Ordinal);

        public IReadOnlyList<QuestDef> Active => _active;
        public IReadOnlyList<QuestDef> Completed => _completed;
        public string SaveKey => "quest_manager";

        public string CurrentObjectiveText
        {
            get
            {
                for (int i = 0; i < _active.Count; i++)
                {
                    var q = _active[i];
                    if (!q.main) continue;
                    var step = q.GetCurrentStep();
                    if (step != null)
                    {
                        if (step.count > 1)
                            return q.title + ": " + step.text + " (" + step.progress + "/" + step.count + ")";
                        return q.title + ": " + step.text;
                    }
                }

                for (int i = 0; i < _active.Count; i++)
                {
                    var q = _active[i];
                    var step = q.GetCurrentStep();
                    if (step != null)
                        return q.title + ": " + step.text;
                }

                return "Kule 45'ten Çıkış Yolunu Tamamla";
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ResetForNewGame();
        }

        private void OnEnable()
        {
            GameEvents.ObjectiveTriggered += OnObjectiveTriggered;
            GameEvents.SurvivorRescued += OnSurvivorRescued;
            GameEvents.ZombieKilled += OnZombieKilled;
            GameEvents.ItemCrafted += OnItemCrafted;
            GameEvents.ItemAdded += OnItemAdded;
            GameEvents.JournalCollected += OnJournalCollected;
        }

        private void OnDisable()
        {
            GameEvents.ObjectiveTriggered -= OnObjectiveTriggered;
            GameEvents.SurvivorRescued -= OnSurvivorRescued;
            GameEvents.ZombieKilled -= OnZombieKilled;
            GameEvents.ItemCrafted -= OnItemCrafted;
            GameEvents.ItemAdded -= OnItemAdded;
            GameEvents.JournalCollected -= OnJournalCollected;
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Register(this);
        }

        private void OnDestroy()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Unregister(this);
            if (Instance == this)
                Instance = null;
        }

        public bool IsCompleted(string id)
        {
            for (int i = 0; i < _completed.Count; i++)
            {
                if (_completed[i].id == id) return true;
            }
            return false;
        }

        public bool HasFlag(string key)
        {
            return !string.IsNullOrEmpty(key) && _flags.Contains(key);
        }

        public void ResetForNewGame()
        {
            _flags.Clear();
            _allQuests.Clear();
            _active.Clear();
            _completed.Clear();

            BuildQuests();
            for (int i = 0; i < _allQuests.Count; i++)
            {
                _active.Add(_allQuests[i]);
            }
        }

        private void OnObjectiveTriggered(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            _flags.Add(key);

            AdvanceStepKey(key, 1);

            if (key.StartsWith("evidence_collected_", StringComparison.Ordinal))
            {
                AdvanceStepKey("any_evidence", 1);
            }
        }

        private void OnSurvivorRescued(string survivorId)
        {
            if (string.IsNullOrEmpty(survivorId)) return;
            string flag = survivorId + "_rescued";
            if (_flags.Add(flag))
            {
                AdvanceStepKey(flag, 1);
            }
        }

        private void OnZombieKilled(string archetype, int floor)
        {
            if (string.IsNullOrEmpty(archetype)) return;

            AdvanceStepKey("kill_" + archetype, 1);

            if (archetype == "goliath" && !_flags.Contains("goliath_defeated"))
            {
                GameEvents.RaiseObjective("goliath_defeated");
            }
        }

        private void OnItemCrafted(string itemId, int count)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            AdvanceStepKey("craft_" + itemId, 1);
        }

        private void OnItemAdded(string itemId, int count)
        {
            if (string.IsNullOrEmpty(itemId)) return;

            if (itemId == "radar_code" && !_flags.Contains("radar_code_acquired"))
            {
                _flags.Add("radar_code_acquired");
                AdvanceStepKey("radar_code_acquired", 1);
            }
            else if (itemId == "key_lab" && !_flags.Contains("key_lab_acquired"))
            {
                _flags.Add("key_lab_acquired");
                AdvanceStepKey("key_lab_acquired", 1);
            }
        }

        private void OnJournalCollected(string journalId)
        {
            AdvanceStepKey("collect_journal", 1);
        }

        private void AdvanceStepKey(string key, int delta)
        {
            if (string.IsNullOrEmpty(key) || delta <= 0) return;

            bool anyCompleted = false;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var q = _active[i];
                bool updated = false;
                for (int s = 0; s < q.steps.Length; s++)
                {
                    var step = q.steps[s];
                    if (step.key == key && !step.IsDone)
                    {
                        step.progress = Mathf.Min(step.count, step.progress + delta);
                        updated = true;
                    }
                }

                if (updated && q.IsAllStepsDone())
                {
                    _active.RemoveAt(i);
                    _completed.Add(q);
                    anyCompleted = true;
                    AudioManager.Play2D("ui_quest", 1f);
                    GameEvents.RaiseQuestCompleted(q.id);
                    GameEvents.RaiseToast("Görev Tamamlandı: " + q.title);
                }
            }

            if (anyCompleted && HUD.Instance != null)
            {
                HUD.Instance.SetObjective(CurrentObjectiveText);
            }
        }

        private void BuildQuests()
        {
            // =================================================================
            // MAIN QUESTS (20 main quest steps across 4 main quest arcs)
            // =================================================================

            // Main Quest 1: 38-B Güvenli Ev Kurulumu (4 steps)
            _allQuests.Add(new QuestDef(
                "mq_01_safehouse",
                "1. 38-B Güvenli Ev Kurulumu",
                "Gece yarısı B4 katındaki patlamadan sonra ordu Kule 45'i dışarıdan mühürledi. 38-B daireni tahkim et, yağmur suyu arıtıcısını kur, havalandırma şaftını kapat ve 88.7 Kule Radyosu'nu devreye al.",
                new[]
                {
                    new QuestStep("barricade_built", "38-B Kapı veya Pencerelerine Barikat Kur (Üs Menüsü)", 1),
                    new QuestStep("purifier_built", "Balkonda Yağmur Suyu Arıtıcısını İnşa Et (Üs Menüsü)", 1),
                    new QuestStep("vent_sealed", "38. Kat Havalandırma Şaftını Hurda Metal ve Bantla Mühürle", 1),
                    new QuestStep("radio_repaired", "88.7 Kule Radyosu'nu Onar veya 27. Kattan Nehir'i Kurtar", 1)
                },
                true));

            // Main Quest 2: Kuledeki Uzmanları ve İstihbaratı Kurtar (4 steps)
            _allQuests.Add(new QuestDef(
                "mq_02_specialists",
                "2. Kule 45 Uzmanlarını 38-B'ye Topla",
                "Hayatta kalmak ve çıkış yollarını açmak için kulede mahsur kalan uzmanları 38-B Güvenli Ev'e kurtar ve 8. kattaki güvenlik görevlisi Murat'tan şifreleri öğren.",
                new[]
                {
                    new QuestStep("selin_rescued", "31. Kat Arslan Polikliniği'nden Dr. Selin Arslan'ı Kurtar", 1),
                    new QuestStep("kemal_rescued", "24. Kat Silah Atölyesi'nden Kemal Usta'yı Kurtar", 1),
                    new QuestStep("elif_rescued", "19. Kat Kimya Laboratuvarı'ndan Elif Yıldız'ı Kurtar", 1),
                    new QuestStep("murat_rescued", "8. Kat Güvenlik Kabini'nden Murat Şen'i Kurtar (Şifreler & Lab Anahtarı)", 1)
                },
                true));

            // Main Quest 3 (Route A): Rota A — Çatı Helikopter Tahliyesi (6 steps)
            _allQuests.Add(new QuestDef(
                "mq_03_route_a_helicopter",
                "3A. Rota A: Fırtına Üstünde Helikopter Tahliyesi (39–45. Kat)",
                "Yönetici katlarındaki üç güvenlik kasasını açarak 45. kat çatı helipadine ulaş, hava savunma radarını devre dışı bırak, UPLINK-7 sinyalini gönder ve helikopter inene kadar çatıyı savun.",
                new[]
                {
                    new QuestStep("exec_keycard_1", "39. Kat Cemil Aethel Kasasından Yönetici Kartı 1'i Al (Şifre: 1704)", 1),
                    new QuestStep("exec_keycard_2", "41. Kat Chronos Kasasından Yönetici Kartı 2'yi Al (Şifre: 2611)", 1),
                    new QuestStep("radar_code_acquired", "43. Kat Balo Salonu Arşivinden UPLINK-7 Radar Kodunu İndir", 1),
                    new QuestStep("roof_unlocked", "44. Kat Kasasından Kart 3'ü Al (Şifre: 4538) ve Çatı Panelini Aç", 1),
                    new QuestStep("radar_disabled", "45. Kat Hava Savunma Radarını Devre Dışı Bırak (2 Devre, 2 Kablo)", 1),
                    new QuestStep("signal_sent", "45. Kat Uplink Terminalinden Kartal-3 Helikopterine Sinyal Gönder", 1)
                },
                true));

            // Main Quest 4 (Route B): Rota B — Chronos Laboratuvarı & Zırhlı Araç Yarma Harekâtı (6 steps)
            _allQuests.Add(new QuestDef(
                "mq_04_route_b_vehicle",
                "3B. Rota B: Project Lazarus & Zırhlı Araç Yarma Harekâtı (B1–B4 & Zemin)",
                "Bodrum katlarındaki Chronos Bio-Labs'a in, Denek-0 Goliath'ı durdur, Tarık Aydın ile 12 tonluk Zırhlı Müdahale Aracı'nı onar ve Zemin Kat'taki kaynaklı çelik lobi barikatını yık.",
                new[]
                {
                    new QuestStep("lab_unlocked", "B1 Otoparkındaki Chronos Bio-Labs Zırhlı Kapısını Aç (key_lab)", 1),
                    new QuestStep("tarik_rescued", "B1 Otopark Atölyesi'ndeki Tamirci Tarık Aydın'dan Araç Anahtarını Al", 1),
                    new QuestStep("evidence_collected_5", "B4 Project Lazarus Ana Sunucusundan Kanıt Dosyası #5'i İndir", 1),
                    new QuestStep("goliath_defeated", "B4 Merkez Laboratuvarı'nda Denek-0 “Goliath”ı Etkisiz Hale Getir", 1),
                    new QuestStep("vehicle_repaired", "B1'deki Zırhlı Aracı Onar (Motor Parçası, Akü, Yakıt, Araç Anahtarı)", 1),
                    new QuestStep("vehicle_breakout", "Zemin Kat (Kat 0) Çelik Lobi Barikatını Zırhlı Araçla Yık ve Kaç!", 1)
                },
                true));

            // =================================================================
            // SIDE QUESTS (9 side quests)
            // =================================================================

            _allQuests.Add(new QuestDef(
                "sq_01_generator",
                "Yan Görev: 38-B Kesintisiz Güç Hattı",
                "Güvenli evdeki istasyonların verimini ve aydınlatmayı artırmak için daire jeneratörünü kur.",
                new[]
                {
                    new QuestStep("generator_online", "38-B Daire Jeneratörünü İnşa Et (Üs Menüsü)", 1)
                },
                false));

            _allQuests.Add(new QuestDef(
                "sq_02_antidote",
                "Yan Görev: LZR-9 Panzehir Sentezi",
                "Dr. Selin Arslan'ın Tıbbi İstasyonu'nda dondurulmuş LZR-9 örneği ve antibiyotik kullanarak bir Panzehir Serumu üret.",
                new[]
                {
                    new QuestStep("craft_antidote", "Tıbbi İstasyonda LZR-9 Panzehir Serumu Üret", 1)
                },
                false));

            _allQuests.Add(new QuestDef(
                "sq_03_acid_armor",
                "Yan Görev: Aside Karşı Polimer Kalkan",
                "Elif Yıldız'ın Kimya İstasyonu'nda tükürücü mutantların asit bezlerini kullanarak Kimyasal & Asit Korumalı Zırh üret.",
                new[]
                {
                    new QuestStep("craft_armor_acid", "Kimya İstasyonunda Kimyasal & Asit Korumalı Zırh Üret", 1)
                },
                false));

            _allQuests.Add(new QuestDef(
                "sq_04_weapon_mod",
                "Yan Görev: Kemal Usta'nın Özel Modifikasyonu",
                "Kemal Usta'nın Silah Tezgâhı'nda hassas silah parçaları ve çelik plaka ile bir Silah Modifikasyon Kiti üret.",
                new[]
                {
                    new QuestStep("craft_weapon_upgrade_kit", "Silah Tezgâhında Silah Modifikasyon Kiti Üret", 1)
                },
                false));

            _allQuests.Add(new QuestDef(
                "sq_05_all_evidence",
                "Yan Görev: Dr. Leyla Aksoy'un Mirası (5 Kanıt)",
                "B1, B2, B3 ve B4 katlarındaki terminallerde bulunan 5 Chronos Kanıt Dosyası'nın tamamını toplayarak katliamın örtbas edilmesini önle.",
                new[]
                {
                    new QuestStep("evidence_collected_1", "B1 Güvenlik Terminalinden Kanıt #1'i İndir", 1),
                    new QuestStep("evidence_collected_2", "B2 Jeneratör Arşivinden Kanıt #2'yi İndir", 1),
                    new QuestStep("evidence_collected_3", "B3 Karantina Terminalinden Kanıt #3'ü İndir", 1),
                    new QuestStep("evidence_collected_4", "B3 Dr. Leyla Aksoy'un Masasından Kanıt #4'ü Al", 1),
                    new QuestStep("evidence_collected_5", "B4 Lazarus Ana Sunucusundan Kanıt #5'i İndir", 1)
                },
                false));

            _allQuests.Add(new QuestDef(
                "sq_06_runner_cull",
                "Yan Görev: Sessiz Koridorlar (Koşucu Avı)",
                "Spor salonu ve ofis katlarında pusuya yatan ses-duyarlı Koşucu (Runner) mutantlardan 8 tanesini temizle.",
                new[]
                {
                    new QuestStep("kill_runner", "Ses-Duyarlı Koşucu (Runner) Etkisiz Hale Getir", 8)
                },
                false));

            _allQuests.Add(new QuestDef(
                "sq_07_spitter_cull",
                "Yan Görev: Zehirli Nefes (Tükürücü Avı)",
                "Orta katları tehdit eden Asit Tükürücü (Spitter) mutantlardan 5 tanesini etkisiz hale getir.",
                new[]
                {
                    new QuestStep("kill_spitter", "Asit Tükürücü (Spitter) Mutant Etkisiz Hale Getir", 5)
                },
                false));

            _allQuests.Add(new QuestDef(
                "sq_08_armored_cull",
                "Yan Görev: Düşmüş Muhafızlar",
                "Alt katlarda balistik yelekle dolaşan enfekte Vanguard Güvenlik Zombilerinden 6 tanesini kafadan vurarak devir.",
                new[]
                {
                    new QuestStep("kill_armored", "Zırhlı Vanguard Güvenlik Zombisi Etkisiz Hale Getir", 6)
                },
                false));

            _allQuests.Add(new QuestDef(
                "sq_09_journals",
                "Yan Görev: Kule 45'in Tanıkları",
                "Kule sakinlerinin, güvenlik görevlilerinin ve Chronos bilim insanlarının bıraktığı günlüklerden 12 adet topla.",
                new[]
                {
                    new QuestStep("collect_journal", "Kule Günlük ve Kayıtlarını Topla", 12)
                },
                false));
        }

        [Serializable]
        private class StepSaveEntry
        {
            public string questId;
            public int stepIndex;
            public int progress;
        }

        [Serializable]
        private class SaveData
        {
            public List<string> flags = new List<string>();
            public List<string> completedIds = new List<string>();
            public List<StepSaveEntry> stepProgress = new List<StepSaveEntry>();
        }

        public string CaptureState()
        {
            var data = new SaveData
            {
                flags = new List<string>(_flags)
            };

            for (int i = 0; i < _completed.Count; i++)
                data.completedIds.Add(_completed[i].id);

            for (int i = 0; i < _allQuests.Count; i++)
            {
                var q = _allQuests[i];
                for (int s = 0; s < q.steps.Length; s++)
                {
                    data.stepProgress.Add(new StepSaveEntry
                    {
                        questId = q.id,
                        stepIndex = s,
                        progress = q.steps[s].progress
                    });
                }
            }

            return JsonUtility.ToJson(data);
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;

            ResetForNewGame();

            if (data.flags != null)
            {
                for (int i = 0; i < data.flags.Count; i++)
                {
                    if (!string.IsNullOrEmpty(data.flags[i]))
                        _flags.Add(data.flags[i]);
                }
            }

            if (data.stepProgress != null)
            {
                for (int i = 0; i < data.stepProgress.Count; i++)
                {
                    var sp = data.stepProgress[i];
                    var q = FindQuest(sp.questId);
                    if (q != null && sp.stepIndex >= 0 && sp.stepIndex < q.steps.Length)
                    {
                        q.steps[sp.stepIndex].progress = sp.progress;
                    }
                }
            }

            _active.Clear();
            _completed.Clear();
            var compSet = new HashSet<string>(data.completedIds ?? new List<string>(), StringComparer.Ordinal);

            for (int i = 0; i < _allQuests.Count; i++)
            {
                var q = _allQuests[i];
                if (compSet.Contains(q.id) || q.IsAllStepsDone())
                    _completed.Add(q);
                else
                    _active.Add(q);
            }
        }

        private QuestDef FindQuest(string id)
        {
            for (int i = 0; i < _allQuests.Count; i++)
            {
                if (_allQuests[i].id == id) return _allQuests[i];
            }
            return null;
        }
    }
}
