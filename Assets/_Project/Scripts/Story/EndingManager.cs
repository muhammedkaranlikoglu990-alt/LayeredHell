// 45: Layered Hell — EndingManager: roof horde defense on signal_sent & full-screen Turkish epilogue UI for both endings.
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace LayeredHell
{
    public class EndingManager : MonoBehaviour
    {
        public static EndingManager Instance { get; private set; }

        private Canvas _canvas;
        private RectTransform _rootPanel;
        private Text _titleText;
        private Text _subtitleText;
        private Text _bodyText;
        private Text _statsText;

        private bool _hordeDefenseActive;
        private bool _endingShown;
        private float _fallbackDefenseTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildEndingUI();
        }

        private void OnEnable()
        {
            GameEvents.ObjectiveTriggered += OnObjectiveTriggered;
            GameEvents.EndingTriggered += OnEndingTriggered;
            GameEvents.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.ObjectiveTriggered -= OnObjectiveTriggered;
            GameEvents.EndingTriggered -= OnEndingTriggered;
            GameEvents.StateChanged -= OnStateChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            // Safety fallback timer if ZombieDirector is not active or finishes wave timer
            if (_hordeDefenseActive && !_endingShown)
            {
                _fallbackDefenseTimer -= Time.deltaTime;
                if (_fallbackDefenseTimer <= 0f)
                {
                    _hordeDefenseActive = false;
                    GameEvents.RaiseObjective("helicopter_defense_complete");
                }
            }
        }

        private void OnStateChanged(GameState newState)
        {
            if (newState == GameState.MainMenu || newState == GameState.Playing)
            {
                if (newState == GameState.MainMenu)
                {
                    _hordeDefenseActive = false;
                    _endingShown = false;
                    if (_rootPanel != null)
                        _rootPanel.gameObject.SetActive(false);
                }
            }
        }

        private void OnObjectiveTriggered(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (key == "signal_sent" && !_hordeDefenseActive && !_endingShown)
            {
                StartRoofHordeDefense();
            }
            else if (key == "helicopter_defense_complete" && !_endingShown)
            {
                _hordeDefenseActive = false;
                GameEvents.RaiseEnding("helicopter");
            }
            else if (key == "vehicle_breakout" && !_endingShown)
            {
                GameEvents.RaiseEnding("armored_vehicle");
            }
        }

        private void StartRoofHordeDefense()
        {
            _hordeDefenseActive = true;
            _fallbackDefenseTimer = 45f;

            AudioManager.Play2D("alarm", 0.9f);
            AudioManager.PlayMusic("mus_combat", true, 1.0f);
            GameEvents.RaiseToast("KARTAL-3 YAKLAŞIYOR! 3 Dalga Sürüye Karşı 45. Kat Helipadini Savun!");

            if (ZombieDirector.Instance != null)
            {
                ZombieDirector.Instance.StartHordeDefense(3, () =>
                {
                    if (_hordeDefenseActive && !_endingShown)
                    {
                        _hordeDefenseActive = false;
                        GameEvents.RaiseObjective("helicopter_defense_complete");
                    }
                });
            }
        }

        private void OnEndingTriggered(string endingId)
        {
            if (_endingShown) return;
            _endingShown = true;
            _hordeDefenseActive = false;

            if (GameMenuUI.Instance != null && GameMenuUI.Instance.IsOpen)
                GameMenuUI.Instance.Toggle();

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Ending);

            if (PlayerController.Instance != null)
                PlayerController.Instance.SetInputLocked(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            bool isHelicopter = string.Equals(endingId, "helicopter", StringComparison.OrdinalIgnoreCase);
            AudioManager.PlayMusic(isHelicopter ? "mus_ending_helicopter" : "mus_ending_escape", false, 1.5f);

            PopulateEpilogueText(isHelicopter);
            if (_rootPanel != null)
                _rootPanel.gameObject.SetActive(true);
        }

        private void PopulateEpilogueText(bool isHelicopter)
        {
            int rescuedCount = CountRescuedSurvivors();
            int evidenceCount = CountCollectedEvidence();
            int journalCount = JournalSystem.Instance != null ? JournalSystem.Instance.Collected.Count : 1;
            float playSecs = GameManager.Instance != null ? GameManager.Instance.PlayTimeSeconds : 0f;
            int mins = Mathf.FloorToInt(playSecs / 60f);
            int secs = Mathf.FloorToInt(playSecs % 60f);

            if (isHelicopter)
            {
                _titleText.text = "SON A — FIRTINA ÜSTÜNDE KURTULUŞ";
                _subtitleText.text = "45. Kat Helipad Tahliyesi · 14 Kasım 2026, Saat 05:42";
            }
            else
            {
                _titleText.text = "SON B — ÇELİK MÜHRÜ YARANLAR";
                _subtitleText.text = "Zırhlı Müdahale Aracı Çıkışı · 14 Kasım 2026, Saat 05:48";
            }

            var sb = new StringBuilder();
            if (isHelicopter)
            {
                sb.AppendLine("45. kat çatısında hava savunma radarı kıvılcımlar saçarak sustuğunda, fırtına bulutlarının arasından bağımsız arama-kurtarma helikopteri Kartal-3'ün projektörleri belirdi. Çatı kapısından taşan enfekte sürüsüne karşı son şarjörünü boşaltan Aras Kaya, 38-B Güvenli Ev'de bir araya getirdiği hayatta kalanları helikopterin yan kapısından içeri çekti.");
                sb.AppendLine();
                sb.AppendLine("Helikopter Kule 45'in kanlı çatısından yükselirken aşağıda kırk beş katlık dev beton kule, gri LZR-9 sisinin ve yangınların içinde karanlık bir anıt gibi geride kaldı. Aras kabin telsizinden yıllardır sesini duyamadığı kardeşi Deniz'in frekansını sonunda yakaladı.");
            }
            else
            {
                sb.AppendLine("B4 katında Denek-0 Hasan Bozkurt'un — Goliath'ın — trajik öfkesine son verdikten sonra Aras Kaya ve başmakinist Tarık Aydın, 12 tonluk Vanguard Zırhlı Müdahale Aracı'nın dizel motorunu kükretti. 38-B Güvenli Ev ekibini zırhlı kabine alan araç, B1 rampasından Zemin Kat ana lobisine tam gaz çıktı.");
                sb.AppendLine();
                sb.AppendLine("KAPAK Harekâtı komutanı Albay Sinan Erdem'in dışarıdan kaynattığı dört santimlik çelik plakalar, zırhlı koçbaşı tamponunun darbesiyle kıvılcımlar saçarak yırtıldı. Kule 45'in mührü içeriden kırıldı ve zırhlı araç şafak yağmurunun altındaki şehir caddelerine çıktı.");
            }

            sb.AppendLine();
            if (evidenceCount >= 5)
            {
                sb.AppendLine("• GERÇEK ORTAYA ÇIKTI (5/5 KANIT): Dr. Leyla Aksoy'un B1-B4 terminallerine bıraktığı tüm Project Lazarus ve KAPAK Harekâtı dosyaları canlı yayında basına sızdırıldı. Chronos Bio-Labs CEO'su Baran Tuncel ve sorumlular tutuklandı; Dr. Selin Arslan'ın geliştirdiği LZR-9 Panzehiri karantina bölgesine dağıtıldı.");
            }
            else if (evidenceCount >= 2)
            {
                sb.AppendLine("• KISMİ İFŞA (" + evidenceCount + "/5 KANIT): Kurtarılan laboratuvar kayıtları resmi “doğalgaz patlaması” yalanını çökertmeye yetti; ancak Chronos Bio-Labs'ın üst düzey finansörleri karanlıkta izlerini kaybettirdi.");
            }
            else
            {
                sb.AppendLine("• KARARTILAN DOSYA (" + evidenceCount + "/5 KANIT): Bodrum katlarındaki Project Lazarus kanıtları kulede kaldığı için Aethel-Vanguard Holding felaketi bir “endüstriyel gaz kazası” olarak kapattı. Yine de Aras ve yanındakiler o geceden sağ çıkmayı başardı.");
            }

            _bodyText.text = sb.ToString();

            _statsText.text =
                "KURTARILAN UZMANLAR: " + rescuedCount + " / 6   |   " +
                "CHRONOS KANIT DOSYALARI: " + evidenceCount + " / 5   |   " +
                "TOPLANAN GÜNLÜKLER: " + journalCount + " / " + StoryDatabase.Journals.Count + "   |   " +
                "SÜRE: " + mins.ToString("00") + ":" + secs.ToString("00");
        }

        private static int CountRescuedSurvivors()
        {
            if (BaseManager.Instance == null) return 0;
            string[] ids = { "selin", "kemal", "elif", "nehir", "murat", "tarik" };
            int c = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (BaseManager.Instance.IsRescued(ids[i]))
                    c++;
            }
            return c;
        }

        private static int CountCollectedEvidence()
        {
            int c = 0;
            for (int i = 1; i <= 5; i++)
            {
                string itemKey = "evidence_" + i;
                string flagKey = "evidence_collected_" + i;
                bool has = (Inventory.Instance != null && (Inventory.Instance.Has(itemKey, 1) || Inventory.Instance.StorageCount(itemKey) > 0))
                           || (QuestManager.Instance != null && QuestManager.Instance.HasFlag(flagKey));
                if (has) c++;
            }
            return c;
        }

        private void BuildEndingUI()
        {
            _canvas = UIFactory.CreateCanvas("EndingCanvas", 90, true);
            _rootPanel = UIFactory.CreatePanel(_canvas.transform, "EndingRoot", new Color(0.02f, 0.02f, 0.03f, 0.97f));
            UIFactory.Stretch(_rootPanel);

            var card = UIFactory.CreatePanel(_rootPanel, "EndingCard", new Color(0.06f, 0.07f, 0.09f, 0.98f));
            UIFactory.Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1380f, 860f));

            var accentBar = UIFactory.CreatePanel(card, "TopAccent", UIFactory.AccentColor);
            UIFactory.Anchor(accentBar, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(1380f, 8f));

            _titleText = UIFactory.CreateText(card, "SON", 46, TextAnchor.MiddleCenter, UIFactory.AccentColor);
            UIFactory.Anchor(_titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(1260f, 64f));

            _subtitleText = UIFactory.CreateText(card, "", 26, TextAnchor.MiddleCenter, new Color(0.72f, 0.76f, 0.80f, 1f));
            UIFactory.Anchor(_subtitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(1260f, 40f));

            var bodyPanel = UIFactory.CreatePanel(card, "BodyBox", new Color(0.03f, 0.04f, 0.05f, 0.85f));
            UIFactory.Anchor(bodyPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1240f, 470f));

            _bodyText = UIFactory.CreateText(bodyPanel, "", 26, TextAnchor.UpperLeft, UIFactory.TextColor);
            UIFactory.Stretch(_bodyText.rectTransform, 32f, 24f, 32f, 24f);

            _statsText = UIFactory.CreateText(card, "", 24, TextAnchor.MiddleCenter, new Color(0.92f, 0.78f, 0.35f, 1f));
            UIFactory.Anchor(_statsText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 130f), new Vector2(1260f, 48f));

            var menuBtn = UIFactory.CreateButton(card, "ANA MENÜYE DÖN", () =>
            {
                AudioManager.Play2D("ui_click", 1f);
                _rootPanel.gameObject.SetActive(false);
                if (GameManager.Instance != null)
                    GameManager.Instance.ReturnToMainMenu();
            }, new Vector2(380f, 68f), 28);
            UIFactory.Anchor(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(380f, 68f));

            _rootPanel.gameObject.SetActive(false);
        }
    }
}
