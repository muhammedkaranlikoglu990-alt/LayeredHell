// 45: Layered Hell — GameMenuUI: 6-tab Turkish pause & safe-house management UI (Envanter, Üretim, Günlük, Görevler, Üs, Sistem).
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace LayeredHell
{
    public class GameMenuUI : MonoBehaviour
    {
        public static GameMenuUI Instance { get; private set; }

        private static readonly string[] TabNames =
        {
            "Envanter",
            "Üretim",
            "Günlük",
            "Görevler",
            "Üs",
            "Sistem"
        };

        private Canvas _canvas;
        private RectTransform _rootPanel;
        private RectTransform _bodyArea;
        private Text _headerStatusText;
        private readonly Dictionary<string, Button> _tabButtons = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);

        private string _activeTab = "Envanter";
        private string _activeStationFilter = "all";
        private string _selectedJournalId = "jrn_38_aras_manifesto";
        private bool _journalShowRadioSubtab;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildShell();
        }

        private void OnEnable()
        {
            GameEvents.StateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.StateChanged -= OnGameStateChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (GameManager.Instance != null)
            {
                var st = GameManager.Instance.State;
                if (st != GameState.Playing && st != GameState.Paused)
                    return;
            }

            if (InputRouter.MenuPressed)
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (IsOpen)
                Close();
            else
                Open(_activeTab);
        }

        public void Open(string tab, string station = null)
        {
            if (!string.IsNullOrEmpty(tab))
            {
                _activeTab = NormalizeTabName(tab);
            }

            if (!string.IsNullOrEmpty(station))
            {
                if (_activeTab == "Üretim")
                {
                    _activeStationFilter = station;
                }
                else if (_activeTab == "Günlük")
                {
                    if (station == "radio")
                    {
                        _journalShowRadioSubtab = true;
                    }
                    else
                    {
                        _journalShowRadioSubtab = false;
                        _selectedJournalId = station;
                    }
                }
            }

            IsOpen = true;
            if (_rootPanel != null)
                _rootPanel.gameObject.SetActive(true);

            if (GameManager.Instance != null)
                GameManager.Instance.Pause(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            AudioManager.Play2D("ui_open", 0.85f);
            RefreshCurrentTab();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;

            if (_rootPanel != null)
                _rootPanel.gameObject.SetActive(false);

            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Paused)
                GameManager.Instance.Pause(false);

            AudioManager.Play2D("ui_close", 0.85f);
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.MainMenu || state == GameState.Dead || state == GameState.Ending)
            {
                IsOpen = false;
                if (_rootPanel != null)
                    _rootPanel.gameObject.SetActive(false);
            }
        }

        private static string NormalizeTabName(string raw)
        {
            for (int i = 0; i < TabNames.Length; i++)
            {
                if (string.Equals(TabNames[i], raw, StringComparison.OrdinalIgnoreCase))
                    return TabNames[i];
            }
            return "Envanter";
        }

        private void BuildShell()
        {
            _canvas = UIFactory.CreateCanvas("GameMenuCanvas", 60, true);
            _rootPanel = UIFactory.CreatePanel(_canvas.transform, "MenuRoot", new Color(0.02f, 0.03f, 0.04f, 0.93f));
            UIFactory.Stretch(_rootPanel);

            var window = UIFactory.CreatePanel(_rootPanel, "Window", UIFactory.PanelColor);
            UIFactory.Stretch(window, 48f, 36f, 48f, 36f);

            // Top header bar (tabs + close button)
            var topBar = UIFactory.CreatePanel(window, "TopBar", new Color(0.08f, 0.09f, 0.11f, 0.98f));
            UIFactory.Anchor(topBar, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(1824f, 84f));
            topBar.anchorMin = new Vector2(0f, 1f);
            topBar.anchorMax = new Vector2(1f, 1f);
            topBar.offsetMin = new Vector2(0f, -84f);
            topBar.offsetMax = Vector2.zero;

            float tabWidth = 220f;
            for (int i = 0; i < TabNames.Length; i++)
            {
                string tabName = TabNames[i];
                var btn = UIFactory.CreateButton(topBar, tabName.ToUpperInvariant(), () =>
                {
                    AudioManager.Play2D("ui_tab", 0.9f);
                    _activeTab = tabName;
                    RefreshCurrentTab();
                }, new Vector2(tabWidth, 64f), 25);

                var rt = btn.GetComponent<RectTransform>();
                UIFactory.Anchor(rt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f + i * (tabWidth + 10f), 0f), new Vector2(tabWidth, 64f));
                _tabButtons[tabName] = btn;
            }

            var closeBtn = UIFactory.CreateButton(topBar, "KAPAT [ESC]", Close, new Vector2(210f, 64f), 24);
            UIFactory.Anchor(closeBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(210f, 64f));

            // Status strip under top bar
            var statusStrip = UIFactory.CreatePanel(window, "StatusStrip", new Color(0.11f, 0.12f, 0.14f, 0.95f));
            statusStrip.anchorMin = new Vector2(0f, 1f);
            statusStrip.anchorMax = new Vector2(1f, 1f);
            statusStrip.offsetMin = new Vector2(0f, -132f);
            statusStrip.offsetMax = new Vector2(0f, -84f);

            _headerStatusText = UIFactory.CreateText(statusStrip, "", 22, TextAnchor.MiddleLeft, new Color(0.92f, 0.84f, 0.65f, 1f));
            UIFactory.Stretch(_headerStatusText.rectTransform, 20f, 4f, 20f, 4f);

            // Body content area
            _bodyArea = UIFactory.CreatePanel(window, "BodyArea", new Color(0f, 0f, 0f, 0f));
            UIFactory.Stretch(_bodyArea, 18f, 18f, 18f, 142f);

            _rootPanel.gameObject.SetActive(false);
        }

        public void RefreshCurrentTab()
        {
            if (_bodyArea == null) return;
            UIFactory.ClearChildren(_bodyArea);

            foreach (var kv in _tabButtons)
            {
                var img = kv.Value.GetComponent<Image>();
                if (img != null)
                {
                    bool active = string.Equals(kv.Key, _activeTab, StringComparison.OrdinalIgnoreCase);
                    img.color = active ? UIFactory.AccentColor : new Color(0.14f, 0.16f, 0.18f, 0.95f);
                }
            }

            UpdateHeaderStatusStrip();

            switch (_activeTab)
            {
                case "Envanter":
                    BuildInventoryTab();
                    break;
                case "Üretim":
                    BuildCraftingTab();
                    break;
                case "Günlük":
                    BuildJournalTab();
                    break;
                case "Görevler":
                    BuildQuestsTab();
                    break;
                case "Üs":
                    BuildBaseTab();
                    break;
                case "Sistem":
                    BuildSystemTab();
                    break;
                default:
                    BuildInventoryTab();
                    break;
            }
        }

        private void UpdateHeaderStatusStrip()
        {
            if (_headerStatusText == null) return;

            int floor = FloorStreamer.Instance != null ? FloorStreamer.Instance.CurrentFloor : GameConstants.BaseFloor;
            float weight = Inventory.Instance != null ? Inventory.Instance.Weight : 0f;
            float maxWeight = Inventory.Instance != null ? Inventory.Instance.MaxWeight : 40f;
            string wpn = Inventory.Instance != null && !string.IsNullOrEmpty(Inventory.Instance.EquippedWeaponId)
                ? ItemDatabase.Get(Inventory.Instance.EquippedWeaponId).name
                : "Yok";
            string arm = Inventory.Instance != null && !string.IsNullOrEmpty(Inventory.Instance.EquippedArmorId)
                ? ItemDatabase.Get(Inventory.Instance.EquippedArmorId).name + " (%" + Mathf.RoundToInt(Inventory.Instance.ArmorReduction * 100f) + ")"
                : "Yok";

            var stats = PlayerController.Instance != null ? PlayerController.Instance.Stats : null;
            string vitals = stats != null
                ? "Can: " + Mathf.CeilToInt(stats.Health) + "/" + Mathf.CeilToInt(stats.MaxHealth) +
                  "  |  Açlık: %" + Mathf.RoundToInt(stats.Hunger) +
                  "  |  Susuzluk: %" + Mathf.RoundToInt(stats.Thirst) +
                  "  |  Enfeksiyon: %" + Mathf.RoundToInt(stats.Infection)
                : "Can: 100/100";

            _headerStatusText.text =
                "KONUM: " + GameConstants.FloorLabel(floor) +
                "   |   " + vitals +
                "   |   AĞIRLIK: " + weight.ToString("0.0") + "/" + maxWeight.ToString("0.0") + " kg" +
                "   |   SİLAH: " + wpn +
                "   |   ZIRH: " + arm;
        }

        // =====================================================================
        // TAB 1: ENVANTER & 38-B ERZAK SANDIĞI
        // =====================================================================
        private void BuildInventoryTab()
        {
            var leftPanel = UIFactory.CreatePanel(_bodyArea, "InvLeft", new Color(0.06f, 0.07f, 0.08f, 0.9f));
            leftPanel.anchorMin = new Vector2(0f, 0f);
            leftPanel.anchorMax = new Vector2(0.58f, 1f);
            leftPanel.offsetMin = Vector2.zero;
            leftPanel.offsetMax = new Vector2(-8f, 0f);

            var rightPanel = UIFactory.CreatePanel(_bodyArea, "InvRight", new Color(0.06f, 0.07f, 0.08f, 0.9f));
            rightPanel.anchorMin = new Vector2(0.58f, 0f);
            rightPanel.anchorMax = new Vector2(1f, 1f);
            rightPanel.offsetMin = new Vector2(8f, 0f);
            rightPanel.offsetMax = Vector2.zero;

            float w = Inventory.Instance != null ? Inventory.Instance.Weight : 0f;
            float mw = Inventory.Instance != null ? Inventory.Instance.MaxWeight : 40f;
            var leftTitle = UIFactory.CreateText(leftPanel, "ÇANTA ENVANTERİ (" + w.ToString("0.1") + " / " + mw.ToString("0.0") + " kg)", 26, TextAnchor.MiddleLeft, UIFactory.AccentColor);
            UIFactory.Anchor(leftTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(900f, 44f));

            var invScroll = UIFactory.CreateScrollList(leftPanel, out var invContent, 8f);
            UIFactory.Stretch(invScroll.GetComponent<RectTransform>(), 12f, 12f, 12f, 60f);

            if (Inventory.Instance != null)
            {
                var items = Inventory.Instance.Items;
                if (items.Count == 0)
                {
                    UIFactory.CreateText(invContent, "Envanterinizde hiç eşya yok.", 24, TextAnchor.MiddleCenter);
                }
                for (int i = 0; i < items.Count; i++)
                {
                    var stack = items[i];
                    var def = ItemDatabase.Get(stack.id);
                    BuildInventoryRow(invContent, stack, def, false);
                }
            }

            bool onBaseFloor = FloorStreamer.Instance == null || FloorStreamer.Instance.CurrentFloor == GameConstants.BaseFloor;
            string storageHeader = onBaseFloor
                ? "38-B GÜVENLİ EV ERZAK SANDIĞI (DEPO)"
                : "38-B ERZAK SANDIĞI (Aktarım İçin 38. Katta Olmalısınız)";
            var rightTitle = UIFactory.CreateText(rightPanel, storageHeader, 25, TextAnchor.MiddleLeft, new Color(0.45f, 0.78f, 0.95f, 1f));
            UIFactory.Anchor(rightTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(720f, 44f));

            var stScroll = UIFactory.CreateScrollList(rightPanel, out var stContent, 8f);
            UIFactory.Stretch(stScroll.GetComponent<RectTransform>(), 12f, 12f, 12f, 60f);

            if (Inventory.Instance != null)
            {
                var storage = Inventory.Instance.Storage;
                if (storage.Count == 0)
                {
                    UIFactory.CreateText(stContent, "38-B Erzak Sandığı şu anda boş.\nYağmur suyu arıtıcısı kurulduğunda pasif üretilen sular burada birikir.", 22, TextAnchor.MiddleCenter);
                }
                for (int i = 0; i < storage.Count; i++)
                {
                    var stack = storage[i];
                    var def = ItemDatabase.Get(stack.id);
                    BuildInventoryRow(stContent, stack, def, true);
                }
            }
        }

        private void BuildInventoryRow(RectTransform parent, ItemStack stack, ItemDef def, bool isStorageRow)
        {
            var row = UIFactory.CreatePanel(parent, "Item_" + stack.id, new Color(0.11f, 0.13f, 0.15f, 0.95f));
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 96f;
            le.preferredHeight = 96f;

            bool isEquippedWpn = !isStorageRow && Inventory.Instance != null && Inventory.Instance.EquippedWeaponId == stack.id;
            bool isEquippedArm = !isStorageRow && Inventory.Instance != null && Inventory.Instance.EquippedArmorId == stack.id;
            string badge = isEquippedWpn ? " [KUŞANILDI - SİLAH]" : (isEquippedArm ? " [KUŞANILDI - ZIRH]" : "");

            string titleLine = def.name + "  x" + stack.count + "   (" + (def.weight * stack.count).ToString("0.1") + " kg)" + badge;
            var titleTxt = UIFactory.CreateText(row, titleLine, 23, TextAnchor.UpperLeft, isEquippedWpn || isEquippedArm ? UIFactory.AccentColor : UIFactory.TextColor);
            UIFactory.Anchor(titleTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -8f), new Vector2(600f, 34f));

            var descTxt = UIFactory.CreateText(row, def.description, 18, TextAnchor.UpperLeft, new Color(0.72f, 0.74f, 0.76f, 1f));
            UIFactory.Anchor(descTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -44f), new Vector2(580f, 44f));

            bool onBaseFloor = FloorStreamer.Instance == null || FloorStreamer.Instance.CurrentFloor == GameConstants.BaseFloor;

            if (!isStorageRow)
            {
                string useLabel = (def.type == ItemType.Weapon || def.type == ItemType.Throwable || def.type == ItemType.Armor)
                    ? "KUŞAN"
                    : "KULLAN";

                var useBtn = UIFactory.CreateButton(row, useLabel, () =>
                {
                    if (Inventory.Instance != null)
                    {
                        Inventory.Instance.Use(stack.id);
                        RefreshCurrentTab();
                    }
                }, new Vector2(145f, 64f), 22);
                UIFactory.Anchor(useBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-175f, 0f), new Vector2(145f, 64f));

                var storeBtn = UIFactory.CreateButton(row, "DEPOYA", () =>
                {
                    if (!onBaseFloor)
                    {
                        GameEvents.RaiseToast("Sandığa eşya koymak için 38. Kat Güvenli Ev'de olmalısınız.");
                        return;
                    }
                    if (Inventory.Instance != null)
                    {
                        Inventory.Instance.MoveToStorage(stack.id, 1);
                        AudioManager.Play2D("ui_click", 0.85f);
                        RefreshCurrentTab();
                    }
                }, new Vector2(150f, 64f), 22);
                UIFactory.Anchor(storeBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(150f, 64f));
            }
            else
            {
                var takeBtn = UIFactory.CreateButton(row, "ÇANTAYA AL", () =>
                {
                    if (!onBaseFloor)
                    {
                        GameEvents.RaiseToast("Sandıktan eşya almak için 38. Kat Güvenli Ev'de olmalısınız.");
                        return;
                    }
                    if (Inventory.Instance != null)
                    {
                        Inventory.Instance.MoveFromStorage(stack.id, 1);
                        AudioManager.Play2D("ui_click", 0.85f);
                        RefreshCurrentTab();
                    }
                }, new Vector2(180f, 64f), 22);
                UIFactory.Anchor(takeBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(180f, 64f));
            }
        }

        // =====================================================================
        // TAB 2: ÜRETİM (CRAFTING)
        // =====================================================================
        private void BuildCraftingTab()
        {
            var filterBar = UIFactory.CreatePanel(_bodyArea, "StationFilterBar", new Color(0.08f, 0.09f, 0.11f, 0.95f));
            filterBar.anchorMin = new Vector2(0f, 1f);
            filterBar.anchorMax = new Vector2(1f, 1f);
            filterBar.offsetMin = new Vector2(0f, -66f);
            filterBar.offsetMax = Vector2.zero;

            string[,] stations =
            {
                { "all", "TÜMÜ" },
                { "workbench", "TEZGÂH" },
                { "stove", "OCAK" },
                { "purifier", "ARITICI" },
                { "med_station", "TIBBİ (SELİN)" },
                { "weapon_bench", "SİLAH (KEMAL)" },
                { "chem_station", "KİMYA (ELİF)" }
            };

            float btnW = 240f;
            for (int i = 0; i < stations.GetLength(0); i++)
            {
                string stId = stations[i, 0];
                string stLabel = stations[i, 1];
                var btn = UIFactory.CreateButton(filterBar, stLabel, () =>
                {
                    AudioManager.Play2D("ui_click", 0.85f);
                    _activeStationFilter = stId;
                    RefreshCurrentTab();
                }, new Vector2(btnW, 52f), 21);

                var rt = btn.GetComponent<RectTransform>();
                UIFactory.Anchor(rt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f + i * (btnW + 8f), 0f), new Vector2(btnW, 52f));
                if (string.Equals(_activeStationFilter, stId, StringComparison.OrdinalIgnoreCase))
                {
                    btn.GetComponent<Image>().color = UIFactory.AccentColor;
                }
            }

            var scroll = UIFactory.CreateScrollList(_bodyArea, out var content, 10f);
            UIFactory.Stretch(scroll.GetComponent<RectTransform>(), 0f, 0f, 0f, 76f);

            if (CraftingSystem.Instance == null) return;

            foreach (var r in CraftingSystem.Instance.GetRecipes(_activeStationFilter))
            {
                BuildRecipeRow(content, r);
            }
        }

        private void BuildRecipeRow(RectTransform parent, RecipeDef r)
        {
            var resDef = ItemDatabase.Get(r.resultId);
            bool canCraft = CraftingSystem.Instance.CanCraft(r, out string reason);

            var row = UIFactory.CreatePanel(parent, "Recipe_" + r.id, new Color(0.10f, 0.12f, 0.14f, 0.95f));
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 108f;
            le.preferredHeight = 108f;

            string title = resDef.name + "  x" + r.resultCount + "   [" + CraftingSystem.StationDisplayName(r.station) +
                           (r.requiresUpgradeLevel > 0 ? " Sv." + r.requiresUpgradeLevel : "") + "]";
            var titleTxt = UIFactory.CreateText(row, title, 24, TextAnchor.UpperLeft, canCraft ? UIFactory.AccentColor : UIFactory.TextColor);
            UIFactory.Anchor(titleTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(1250f, 34f));

            var ingBuilder = new StringBuilder("Gerekli Malzemeler: ");
            for (int i = 0; i < r.ingredients.Length; i++)
            {
                if (i > 0) ingBuilder.Append("   |   ");
                var ing = r.ingredients[i];
                var ingDef = ItemDatabase.Get(ing.id);
                int have = Inventory.Instance != null ? Inventory.Instance.Count(ing.id) : 0;
                ingBuilder.Append(ingDef.name).Append(" (").Append(have).Append("/").Append(ing.count).Append(")");
            }

            var ingTxt = UIFactory.CreateText(row, ingBuilder.ToString(), 20, TextAnchor.UpperLeft, new Color(0.80f, 0.84f, 0.88f, 1f));
            UIFactory.Anchor(ingTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -44f), new Vector2(1300f, 30f));

            var statusTxt = UIFactory.CreateText(row, resDef.description + "  —  Durum: " + reason, 18, TextAnchor.UpperLeft,
                canCraft ? new Color(0.45f, 0.88f, 0.52f, 1f) : new Color(0.85f, 0.48f, 0.42f, 1f));
            UIFactory.Anchor(statusTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -74f), new Vector2(1300f, 28f));

            var craftBtn = UIFactory.CreateButton(row, canCraft ? "ÜRET" : "EKSİK", () =>
            {
                if (CraftingSystem.Instance != null)
                {
                    CraftingSystem.Instance.Craft(r);
                    RefreshCurrentTab();
                }
            }, new Vector2(210f, 72f), 25);
            UIFactory.Anchor(craftBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(210f, 72f));
            if (canCraft)
            {
                craftBtn.GetComponent<Image>().color = new Color(0.20f, 0.52f, 0.28f, 0.98f);
            }
        }

        // =====================================================================
        // TAB 3: GÜNLÜK & 88.7 TELSİZ KAYITLARI
        // =====================================================================
        private void BuildJournalTab()
        {
            var subBar = UIFactory.CreatePanel(_bodyArea, "JournalSubBar", new Color(0.08f, 0.09f, 0.11f, 0.95f));
            subBar.anchorMin = new Vector2(0f, 1f);
            subBar.anchorMax = new Vector2(1f, 1f);
            subBar.offsetMin = new Vector2(0f, -64f);
            subBar.offsetMax = Vector2.zero;

            int collectedCount = JournalSystem.Instance != null ? JournalSystem.Instance.Collected.Count : 0;
            int totalJournals = StoryDatabase.Journals.Count;
            int heardRadio = RadioSystem.Instance != null ? RadioSystem.Instance.HeardLogs.Count : 0;

            var jBtn = UIFactory.CreateButton(subBar, "BULUNAN GÜNLÜKLER (" + collectedCount + " / " + totalJournals + ")", () =>
            {
                AudioManager.Play2D("ui_click", 0.85f);
                _journalShowRadioSubtab = false;
                RefreshCurrentTab();
            }, new Vector2(520f, 50f), 23);
            UIFactory.Anchor(jBtn.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(520f, 50f));

            var rBtn = UIFactory.CreateButton(subBar, "88.7 KULE RADYOSU KAYITLARI (" + heardRadio + ")", () =>
            {
                AudioManager.Play2D("ui_click", 0.85f);
                _journalShowRadioSubtab = true;
                RefreshCurrentTab();
            }, new Vector2(520f, 50f), 23);
            UIFactory.Anchor(rBtn.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(548f, 0f), new Vector2(520f, 50f));

            if (!_journalShowRadioSubtab)
                jBtn.GetComponent<Image>().color = UIFactory.AccentColor;
            else
                rBtn.GetComponent<Image>().color = UIFactory.AccentColor;

            if (_journalShowRadioSubtab)
            {
                BuildRadioHistorySubtab();
                return;
            }

            var leftList = UIFactory.CreatePanel(_bodyArea, "JournalListPanel", new Color(0.06f, 0.07f, 0.08f, 0.9f));
            leftList.anchorMin = new Vector2(0f, 0f);
            leftList.anchorMax = new Vector2(0.40f, 1f);
            leftList.offsetMin = Vector2.zero;
            leftList.offsetMax = new Vector2(-8f, -72f);

            var rightReader = UIFactory.CreatePanel(_bodyArea, "JournalReaderPanel", new Color(0.06f, 0.07f, 0.08f, 0.95f));
            rightReader.anchorMin = new Vector2(0.40f, 0f);
            rightReader.anchorMax = new Vector2(1f, 1f);
            rightReader.offsetMin = new Vector2(8f, 0f);
            rightReader.offsetMax = new Vector2(0f, -72f);

            var listScroll = UIFactory.CreateScrollList(leftList, out var listContent, 8f);
            UIFactory.Stretch(listScroll.GetComponent<RectTransform>(), 8f, 8f, 8f, 8f);

            var collected = JournalSystem.Instance != null ? JournalSystem.Instance.Collected : new List<string>();
            if (collected.Count > 0 && (string.IsNullOrEmpty(_selectedJournalId) || StoryDatabase.GetJournal(_selectedJournalId) == null))
            {
                _selectedJournalId = collected[collected.Count - 1];
            }

            for (int i = collected.Count - 1; i >= 0; i--)
            {
                string jId = collected[i];
                var entry = StoryDatabase.GetJournal(jId);
                if (entry == null) continue;

                string btnLabel = "[" + GameConstants.FloorLabel(entry.floor) + "] " + entry.title;
                var entryBtn = UIFactory.CreateButton(listContent, btnLabel, () =>
                {
                    AudioManager.Play2D("ui_journal", 0.85f);
                    _selectedJournalId = jId;
                    RefreshCurrentTab();
                }, new Vector2(640f, 64f), 21);

                if (jId == _selectedJournalId)
                {
                    entryBtn.GetComponent<Image>().color = UIFactory.AccentColor;
                }
            }

            var selected = StoryDatabase.GetJournal(_selectedJournalId);
            if (selected != null)
            {
                var titleTxt = UIFactory.CreateText(rightReader, selected.title, 30, TextAnchor.UpperLeft, UIFactory.AccentColor);
                UIFactory.Anchor(titleTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -18f), new Vector2(1000f, 44f));

                string meta = "Yazar: " + selected.author + "   |   Bulunduğu Kat: " + GameConstants.FloorLabel(selected.floor);
                var metaTxt = UIFactory.CreateText(rightReader, meta, 22, TextAnchor.UpperLeft, new Color(0.75f, 0.78f, 0.82f, 1f));
                UIFactory.Anchor(metaTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -66f), new Vector2(1000f, 34f));

                var bodyTxt = UIFactory.CreateText(rightReader, selected.text, 24, TextAnchor.UpperLeft, UIFactory.TextColor);
                UIFactory.Stretch(bodyTxt.rectTransform, 24f, 24f, 24f, 118f);
            }
        }

        private void BuildRadioHistorySubtab()
        {
            var scroll = UIFactory.CreateScrollList(_bodyArea, out var content, 10f);
            UIFactory.Stretch(scroll.GetComponent<RectTransform>(), 0f, 0f, 0f, 72f);

            var heard = RadioSystem.Instance != null ? RadioSystem.Instance.HeardLogs : null;
            if (heard == null || heard.Count == 0)
            {
                UIFactory.CreateText(content, "Henüz kaydedilmiş bir telsiz yayını yok.", 24, TextAnchor.MiddleCenter);
                return;
            }

            for (int i = heard.Count - 1; i >= 0; i--)
            {
                var log = heard[i];
                var row = UIFactory.CreatePanel(content, "Radio_" + log.id, new Color(0.10f, 0.12f, 0.14f, 0.95f));
                var le = row.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 112f;
                le.preferredHeight = 112f;

                var spk = UIFactory.CreateText(row, "88.7 FM — " + log.speaker, 23, TextAnchor.UpperLeft, UIFactory.AccentColor);
                UIFactory.Anchor(spk.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(1300f, 32f));

                var txt = UIFactory.CreateText(row, "“" + log.text + "”", 20, TextAnchor.UpperLeft, UIFactory.TextColor);
                UIFactory.Anchor(txt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -42f), new Vector2(1420f, 64f));

                var replayBtn = UIFactory.CreateButton(row, "TEKRAR DİNLE", () =>
                {
                    if (RadioSystem.Instance != null)
                        RadioSystem.Instance.Replay(log.id);
                }, new Vector2(210f, 68f), 21);
                UIFactory.Anchor(replayBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(210f, 68f));
            }
        }

        // =====================================================================
        // TAB 4: GÖREVLER (QUESTS)
        // =====================================================================
        private void BuildQuestsTab()
        {
            var scroll = UIFactory.CreateScrollList(_bodyArea, out var content, 12f);
            UIFactory.Stretch(scroll.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);

            if (QuestManager.Instance == null) return;

            var banner = UIFactory.CreatePanel(content, "ActiveBanner", new Color(0.18f, 0.12f, 0.08f, 0.95f));
            var bannerLe = banner.gameObject.AddComponent<LayoutElement>();
            bannerLe.minHeight = 64f;
            bannerLe.preferredHeight = 64f;

            var bannerTxt = UIFactory.CreateText(banner, "AKTİF HEDEF: " + QuestManager.Instance.CurrentObjectiveText, 25, TextAnchor.MiddleLeft, UIFactory.AccentColor);
            UIFactory.Stretch(bannerTxt.rectTransform, 18f, 6f, 18f, 6f);

            var active = QuestManager.Instance.Active;
            for (int i = 0; i < active.Count; i++)
            {
                BuildQuestCard(content, active[i], false);
            }

            var completed = QuestManager.Instance.Completed;
            for (int i = 0; i < completed.Count; i++)
            {
                BuildQuestCard(content, completed[i], true);
            }
        }

        private static void BuildQuestCard(RectTransform parent, QuestDef q, bool completed)
        {
            int stepCount = q.steps != null ? q.steps.Length : 0;
            float height = 104f + stepCount * 32f;

            var card = UIFactory.CreatePanel(parent, "Quest_" + q.id, new Color(0.10f, 0.12f, 0.14f, 0.95f));
            var le = card.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            string tag = q.main ? "[ANA GÖREV] " : "[YAN GÖREV] ";
            string status = completed ? " — [TAMAMLANDI]" : "";
            Color headerColor = completed
                ? new Color(0.45f, 0.88f, 0.52f, 1f)
                : (q.main ? UIFactory.AccentColor : new Color(0.48f, 0.78f, 0.95f, 1f));

            var titleTxt = UIFactory.CreateText(card, tag + q.title + status, 25, TextAnchor.UpperLeft, headerColor);
            UIFactory.Anchor(titleTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -10f), new Vector2(1600f, 34f));

            var descTxt = UIFactory.CreateText(card, q.description, 19, TextAnchor.UpperLeft, new Color(0.75f, 0.78f, 0.80f, 1f));
            UIFactory.Anchor(descTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -44f), new Vector2(1620f, 44f));

            var sb = new StringBuilder();
            if (q.steps != null)
            {
                for (int i = 0; i < q.steps.Length; i++)
                {
                    var s = q.steps[i];
                    string check = s.IsDone ? "[✓] " : "[  ] ";
                    string countSuffix = s.count > 1 ? " (" + s.progress + "/" + s.count + ")" : "";
                    sb.AppendLine(check + s.text + countSuffix);
                }
            }

            var stepsTxt = UIFactory.CreateText(card, sb.ToString(), 20, TextAnchor.UpperLeft, UIFactory.TextColor);
            UIFactory.Anchor(stepsTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -92f), new Vector2(1600f, stepCount * 32f));
        }

        // =====================================================================
        // TAB 5: 38-B GÜVENLİ EV (ÜS & UZMANLAR)
        // =====================================================================
        private void BuildBaseTab()
        {
            var leftPanel = UIFactory.CreatePanel(_bodyArea, "BaseUpgradesPanel", new Color(0.06f, 0.07f, 0.08f, 0.9f));
            leftPanel.anchorMin = new Vector2(0f, 0f);
            leftPanel.anchorMax = new Vector2(0.64f, 1f);
            leftPanel.offsetMin = Vector2.zero;
            leftPanel.offsetMax = new Vector2(-8f, 0f);

            var rightPanel = UIFactory.CreatePanel(_bodyArea, "BaseSurvivorsPanel", new Color(0.06f, 0.07f, 0.08f, 0.9f));
            rightPanel.anchorMin = new Vector2(0.64f, 0f);
            rightPanel.anchorMax = new Vector2(1f, 1f);
            rightPanel.offsetMin = new Vector2(8f, 0f);
            rightPanel.offsetMax = Vector2.zero;

            var upTitle = UIFactory.CreateText(leftPanel, "38-B GÜVENLİ EV İSTASYON & BARİKAT GELİŞTİRMELERİ", 25, TextAnchor.MiddleLeft, UIFactory.AccentColor);
            UIFactory.Anchor(upTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(1000f, 42f));

            var upScroll = UIFactory.CreateScrollList(leftPanel, out var upContent, 8f);
            UIFactory.Stretch(upScroll.GetComponent<RectTransform>(), 10f, 10f, 10f, 58f);

            if (BaseManager.Instance != null)
            {
                var upgrades = BaseManager.Instance.Upgrades;
                for (int i = 0; i < upgrades.Count; i++)
                {
                    BuildUpgradeRow(upContent, upgrades[i]);
                }
            }

            var survTitle = UIFactory.CreateText(rightPanel, "HAYATTA KALAN UZMANLAR (6)", 25, TextAnchor.MiddleLeft, new Color(0.45f, 0.78f, 0.95f, 1f));
            UIFactory.Anchor(survTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(600f, 42f));

            var survScroll = UIFactory.CreateScrollList(rightPanel, out var survContent, 8f);
            UIFactory.Stretch(survScroll.GetComponent<RectTransform>(), 10f, 10f, 10f, 58f);

            var survivors = StoryDatabase.Survivors;
            for (int i = 0; i < survivors.Count; i++)
            {
                var s = survivors[i];
                bool rescued = BaseManager.Instance != null && BaseManager.Instance.IsRescued(s.id);

                var card = UIFactory.CreatePanel(survContent, "Surv_" + s.id, new Color(0.11f, 0.13f, 0.15f, 0.95f));
                var le = card.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 110f;
                le.preferredHeight = 110f;

                string status = rescued ? "[38-B GÜVENLİ EVDE]" : "[MAHSUR: " + GameConstants.FloorLabel(s.floor) + "]";
                Color col = rescued ? new Color(0.45f, 0.88f, 0.52f, 1f) : new Color(0.92f, 0.72f, 0.32f, 1f);

                var nameTxt = UIFactory.CreateText(card, s.name + "  " + status, 22, TextAnchor.UpperLeft, col);
                UIFactory.Anchor(nameTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -10f), new Vector2(580f, 30f));

                var roleTxt = UIFactory.CreateText(card, "Uzmanlık: " + s.profession + "   |   Bulunduğu Kat: " + GameConstants.FloorLabel(s.floor), 19, TextAnchor.UpperLeft, UIFactory.TextColor);
                UIFactory.Anchor(roleTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -42f), new Vector2(580f, 28f));

                string hint = rescued && s.rescuedLines != null && s.rescuedLines.Length > 0
                    ? "“" + s.rescuedLines[0] + "”"
                    : "Kurtarıldığında 38-B'de yeni üretim ve istihbarat kilidini açar.";
                var hintTxt = UIFactory.CreateText(card, hint, 17, TextAnchor.UpperLeft, new Color(0.72f, 0.75f, 0.78f, 1f));
                UIFactory.Anchor(hintTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -70f), new Vector2(580f, 36f));
            }
        }

        private void BuildUpgradeRow(RectTransform parent, UpgradeDef def)
        {
            int lvl = BaseManager.Instance.GetUpgradeLevel(def.id);
            bool canUp = BaseManager.Instance.CanUpgrade(def.id, out string reason);

            var row = UIFactory.CreatePanel(parent, "Upgrade_" + def.id, new Color(0.11f, 0.13f, 0.15f, 0.95f));
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 108f;
            le.preferredHeight = 108f;

            string title = def.name + "   [Seviye " + lvl + " / " + def.maxLevel + "]";
            var titleTxt = UIFactory.CreateText(row, title, 23, TextAnchor.UpperLeft, lvl > 0 ? UIFactory.AccentColor : UIFactory.TextColor);
            UIFactory.Anchor(titleTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -8f), new Vector2(820f, 32f));

            var descTxt = UIFactory.CreateText(row, def.description, 18, TextAnchor.UpperLeft, new Color(0.75f, 0.78f, 0.82f, 1f));
            UIFactory.Anchor(descTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -40f), new Vector2(850f, 28f));

            var costs = def.GetCostForNextLevel(lvl);
            var costSb = new StringBuilder();
            if (lvl >= def.maxLevel)
            {
                costSb.Append("Maksimum seviyeye ulaşıldı.");
            }
            else
            {
                costSb.Append("Sonraki Seviye Maliyeti: ");
                for (int i = 0; i < costs.Length; i++)
                {
                    if (i > 0) costSb.Append("  |  ");
                    var c = costs[i];
                    var itemDef = ItemDatabase.Get(c.id);
                    int have = Inventory.Instance != null ? Inventory.Instance.Count(c.id) : 0;
                    costSb.Append(itemDef.name).Append(" (").Append(have).Append("/").Append(c.count).Append(")");
                }
            }

            var costTxt = UIFactory.CreateText(row, costSb.ToString(), 18, TextAnchor.UpperLeft,
                canUp ? new Color(0.45f, 0.88f, 0.52f, 1f) : new Color(0.88f, 0.58f, 0.42f, 1f));
            UIFactory.Anchor(costTxt.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -72f), new Vector2(860f, 28f));

            string btnLabel = lvl >= def.maxLevel ? "MAKSİMUM" : (lvl == 0 ? "İNŞA ET" : "GELİŞTİR");
            var upBtn = UIFactory.CreateButton(row, btnLabel, () =>
            {
                if (BaseManager.Instance.TryUpgrade(def.id, out string failReason))
                {
                    RefreshCurrentTab();
                }
                else
                {
                    GameEvents.RaiseToast(failReason);
                }
            }, new Vector2(190f, 68f), 22);
            UIFactory.Anchor(upBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(190f, 68f));
            if (canUp)
            {
                upBtn.GetComponent<Image>().color = new Color(0.20f, 0.52f, 0.28f, 0.98f);
            }
        }

        // =====================================================================
        // TAB 6: SİSTEM (SAVE / LOAD / RESUME / MAIN MENU)
        // =====================================================================
        private void BuildSystemTab()
        {
            var centerCard = UIFactory.CreatePanel(_bodyArea, "SystemCard", new Color(0.07f, 0.08f, 0.10f, 0.96f));
            UIFactory.Anchor(centerCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 640f));

            var title = UIFactory.CreateText(centerCard, "45: LAYERED HELL — SİSTEM MENÜSÜ", 32, TextAnchor.MiddleCenter, UIFactory.AccentColor);
            UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(660f, 48f));

            var resumeBtn = UIFactory.CreateButton(centerCard, "OYUNA DÖN", Close, new Vector2(520f, 76f), 28);
            UIFactory.Anchor(resumeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(520f, 76f));

            var saveBtn = UIFactory.CreateButton(centerCard, "OYUNU KAYDET (SLOT 0)", () =>
            {
                if (SaveSystem.Instance != null && SaveSystem.Instance.Save(0))
                {
                    AudioManager.Play2D("ui_save", 1f);
                    GameEvents.RaiseToast("Oyun başarıyla kaydedildi.");
                }
            }, new Vector2(520f, 76f), 28);
            UIFactory.Anchor(saveBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(520f, 76f));

            var loadBtn = UIFactory.CreateButton(centerCard, "KAYITTAN YÜKLE (SLOT 0)", () =>
            {
                if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave(0))
                {
                    AudioManager.Play2D("ui_confirm", 1f);
                    Close();
                    SaveSystem.Instance.Load(0);
                }
                else
                {
                    AudioManager.Play2D("ui_denied", 0.9f);
                    GameEvents.RaiseToast("Kayıtlı oyun bulunamadı.");
                }
            }, new Vector2(520f, 76f), 28);
            UIFactory.Anchor(loadBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -320f), new Vector2(520f, 76f));

            var mainMenuBtn = UIFactory.CreateButton(centerCard, "ANA MENÜYE DÖN", () =>
            {
                AudioManager.Play2D("ui_click", 1f);
                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.Save(0);
                Close();
                if (GameManager.Instance != null)
                    GameManager.Instance.ReturnToMainMenu();
            }, new Vector2(520f, 76f), 28);
            UIFactory.Anchor(mainMenuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -420f), new Vector2(520f, 76f));

            var tipTxt = UIFactory.CreateText(centerCard,
                "İpucu: Oyun her 120 saniyede bir, kat geçişlerinde ve 38-B'deki yatağı kullandığınızda otomatik kaydedilir.",
                20, TextAnchor.MiddleCenter, new Color(0.72f, 0.75f, 0.78f, 1f));
            UIFactory.Anchor(tipTxt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(620f, 64f));
        }
    }
}
