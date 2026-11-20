// Runtime gameplay HUD built with UIFactory: vitals bars, ammo, crosshair, prompt, floor badge, objective, toast, radio, damage flash, and death screen.
using UnityEngine;
using UnityEngine.UI;

namespace LayeredHell
{
    public sealed class HUD : MonoBehaviour
    {
        public static HUD Instance { get; private set; }

        private Canvas _canvas;
        private GameObject _hudRoot;

        // Vitals bars
        private Image _healthFill;
        private Image _staminaFill;
        private Image _hungerFill;
        private Image _thirstFill;
        private Image _infectionFill;
        private Text _healthValueText;

        // Weapon & Ammo
        private Text _weaponNameText;
        private Text _ammoCountText;

        // Crosshair & Interaction prompt
        private Image _crosshairDot;
        private RectTransform _promptPanel;
        private Text _promptText;

        // Floor & Objective
        private Text _floorTitleText;
        private Text _floorSubText;
        private Text _objectiveText;

        // Toast & Radio
        private RectTransform _toastPanel;
        private Text _toastText;
        private float _toastTimer;

        private RectTransform _radioPanel;
        private Text _radioSpeakerText;
        private Text _radioBodyText;
        private float _radioTimer;

        // Damage flash & Death screen
        private Image _damageFlashImage;
        private float _damageFlashAlpha;
        private GameObject _deathPanel;

        private float _objectivePollTimer;
        private WeaponController _boundWeapons;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            BuildUI();
        }

        private void OnEnable()
        {
            GameEvents.Toast += ShowToast;
            GameEvents.RadioMessage += ShowRadio;
            GameEvents.FloorChanged += OnFloorChanged;
            GameEvents.PlayerDamaged += OnPlayerDamaged;
            GameEvents.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.Toast -= ShowToast;
            GameEvents.RadioMessage -= ShowRadio;
            GameEvents.FloorChanged -= OnFloorChanged;
            GameEvents.PlayerDamaged -= OnPlayerDamaged;
            GameEvents.StateChanged -= OnStateChanged;
            UnbindWeapons();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildUI()
        {
            _canvas = UIFactory.CreateCanvas("HUD_Canvas", 20, true);

            _hudRoot = new GameObject("HUDRoot", typeof(RectTransform));
            _hudRoot.transform.SetParent(_canvas.transform, false);
            UIFactory.Stretch(_hudRoot.GetComponent<RectTransform>());

            // Damage flash overlay
            RectTransform flashRt = UIFactory.CreatePanel(_hudRoot.transform, "DamageFlash", new Color(0.75f, 0.04f, 0.04f, 0f));
            UIFactory.Stretch(flashRt);
            _damageFlashImage = flashRt.GetComponent<Image>();
            _damageFlashImage.raycastTarget = false;

            // Top-left Vitals panel
            RectTransform vitalsPanel = UIFactory.CreatePanel(_hudRoot.transform, "VitalsPanel", UIFactory.PanelColor);
            UIFactory.Anchor(vitalsPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(360f, 178f));

            _healthFill = CreateBarRow(vitalsPanel, "CAN", 0, new Color(0.82f, 0.18f, 0.18f, 1f), out _healthValueText);
            _staminaFill = CreateBarRow(vitalsPanel, "NEFES", 1, new Color(0.22f, 0.68f, 0.88f, 1f), out _);
            _hungerFill = CreateBarRow(vitalsPanel, "TOKLUK", 2, new Color(0.85f, 0.58f, 0.18f, 1f), out _);
            _thirstFill = CreateBarRow(vitalsPanel, "SU", 3, new Color(0.25f, 0.52f, 0.92f, 1f), out _);
            _infectionFill = CreateBarRow(vitalsPanel, "ENFEKSİYON", 4, new Color(0.55f, 0.82f, 0.22f, 1f), out _);

            // Top-right Floor & Objective panel
            RectTransform infoPanel = UIFactory.CreatePanel(_hudRoot.transform, "InfoPanel", UIFactory.PanelColor);
            UIFactory.Anchor(infoPanel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(440f, 145f));

            _floorTitleText = UIFactory.CreateText(infoPanel, "38. Kat — Güvenli Ev", 26, TextAnchor.UpperRight, UIFactory.AccentColor);
            UIFactory.Anchor(_floorTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -10f), new Vector2(412f, 34f));

            _floorSubText = UIFactory.CreateText(infoPanel, "Daire 38-B", 18, TextAnchor.UpperRight, new Color(0.75f, 0.75f, 0.72f));
            UIFactory.Anchor(_floorSubText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -44f), new Vector2(412f, 24f));

            _objectiveText = UIFactory.CreateText(infoPanel, "Görev: Üssü güçlendir ve telsizi onar.", 20, TextAnchor.UpperRight, UIFactory.TextColor);
            UIFactory.Anchor(_objectiveText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -74f), new Vector2(412f, 60f));

            // Bottom-right Weapon & Ammo panel
            RectTransform ammoPanel = UIFactory.CreatePanel(_hudRoot.transform, "AmmoPanel", UIFactory.PanelColor);
            UIFactory.Anchor(ammoPanel, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(280f, 96f));

            _weaponNameText = UIFactory.CreateText(ammoPanel, "SİLAH YOK", 22, TextAnchor.UpperRight, UIFactory.AccentColor);
            UIFactory.Anchor(_weaponNameText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -10f), new Vector2(256f, 28f));

            _ammoCountText = UIFactory.CreateText(ammoPanel, "-- / --", 34, TextAnchor.LowerRight, UIFactory.TextColor);
            UIFactory.Anchor(_ammoCountText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(12f, 10f), new Vector2(256f, 44f));

            // Center Crosshair & Prompt
            RectTransform crossRt = UIFactory.CreatePanel(_hudRoot.transform, "Crosshair", new Color(0.95f, 0.95f, 0.9f, 0.85f));
            UIFactory.Anchor(crossRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 6f));
            _crosshairDot = crossRt.GetComponent<Image>();
            _crosshairDot.raycastTarget = false;

            _promptPanel = UIFactory.CreatePanel(_hudRoot.transform, "PromptPanel", new Color(0.03f, 0.04f, 0.05f, 0.82f));
            UIFactory.Anchor(_promptPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -64f), new Vector2(460f, 46f));
            _promptPanel.GetComponent<Image>().raycastTarget = false;
            _promptText = UIFactory.CreateText(_promptPanel, string.Empty, 24, TextAnchor.MiddleCenter, UIFactory.TextColor);
            UIFactory.Stretch(_promptText.rectTransform, 10f, 4f, 10f, 4f);
            _promptPanel.gameObject.SetActive(false);

            // Top-center Toast panel
            _toastPanel = UIFactory.CreatePanel(_hudRoot.transform, "ToastPanel", new Color(0.08f, 0.09f, 0.11f, 0.92f));
            UIFactory.Anchor(_toastPanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(640f, 52f));
            _toastPanel.GetComponent<Image>().raycastTarget = false;
            _toastText = UIFactory.CreateText(_toastPanel, string.Empty, 23, TextAnchor.MiddleCenter, UIFactory.AccentColor);
            UIFactory.Stretch(_toastText.rectTransform, 14f, 4f, 14f, 4f);
            _toastPanel.gameObject.SetActive(false);

            // Bottom-center Radio Subtitle panel
            _radioPanel = UIFactory.CreatePanel(_hudRoot.transform, "RadioPanel", new Color(0.03f, 0.06f, 0.05f, 0.92f));
            UIFactory.Anchor(_radioPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(780f, 104f));
            _radioPanel.GetComponent<Image>().raycastTarget = false;

            _radioSpeakerText = UIFactory.CreateText(_radioPanel, "[TELSİZ]", 20, TextAnchor.UpperLeft, new Color(0.4f, 0.92f, 0.55f));
            UIFactory.Anchor(_radioSpeakerText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -8f), new Vector2(748f, 26f));

            _radioBodyText = UIFactory.CreateText(_radioPanel, string.Empty, 22, TextAnchor.UpperLeft, UIFactory.TextColor);
            UIFactory.Anchor(_radioBodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -36f), new Vector2(748f, 60f));
            _radioPanel.gameObject.SetActive(false);

            // Death Overlay Screen
            BuildDeathPanel();

            UpdateVisibility(GameManager.Instance != null ? GameManager.Instance.State : GameState.Boot);
        }

        private static Image CreateBarRow(Transform parent, string label, int index, Color barColor, out Text valueLabel)
        {
            float y = -12f - index * 32f;
            Text lbl = UIFactory.CreateText(parent, label, 17, TextAnchor.MiddleLeft, UIFactory.TextColor);
            UIFactory.Anchor(lbl.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, y), new Vector2(118f, 24f));

            RectTransform bg = UIFactory.CreatePanel(parent, "BarBG_" + label, new Color(0.14f, 0.15f, 0.17f, 0.95f));
            UIFactory.Anchor(bg, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(132f, y - 3f), new Vector2(212f, 18f));

            RectTransform fill = UIFactory.CreatePanel(bg, "Fill", barColor);
            UIFactory.Stretch(fill);
            Image fillImg = fill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 1f;

            valueLabel = UIFactory.CreateText(bg, string.Empty, 14, TextAnchor.MiddleRight, Color.white);
            UIFactory.Stretch(valueLabel.rectTransform, 4f, 0f, 6f, 0f);

            return fillImg;
        }

        private void BuildDeathPanel()
        {
            RectTransform deathRt = UIFactory.CreatePanel(_canvas.transform, "DeathScreen", new Color(0.08f, 0.01f, 0.01f, 0.94f));
            UIFactory.Stretch(deathRt);
            _deathPanel = deathRt.gameObject;

            Text title = UIFactory.CreateText(deathRt, "ÖLDÜN", 72, TextAnchor.MiddleCenter, new Color(0.88f, 0.16f, 0.14f));
            UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), new Vector2(600f, 90f));

            Text sub = UIFactory.CreateText(deathRt, "Kulenin karanlığı seni yuttu.", 26, TextAnchor.MiddleCenter, UIFactory.TextColor);
            UIFactory.Anchor(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(600f, 40f));

            Button respawnBtn = UIFactory.CreateButton(deathRt, "YENİDEN DOĞ", OnRespawnClicked, new Vector2(360f, 62f), 26);
            UIFactory.Anchor(respawnBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(360f, 62f));

            Button loadBtn = UIFactory.CreateButton(deathRt, "SON KAYDI YÜKLE", OnLoadSaveClicked, new Vector2(360f, 62f), 26);
            UIFactory.Anchor(loadBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -86f), new Vector2(360f, 62f));

            Button menuBtn = UIFactory.CreateButton(deathRt, "ANA MENÜ", OnMainMenuClicked, new Vector2(360f, 62f), 26);
            UIFactory.Anchor(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -162f), new Vector2(360f, 62f));

            _deathPanel.SetActive(false);
        }

        private void OnRespawnClicked()
        {
            AudioManager.Play2D("ui_confirm", 0.8f);
            if (PlayerController.Instance != null && PlayerController.Instance.Stats != null)
            {
                PlayerController.Instance.Stats.Respawn();
            }
        }

        private void OnLoadSaveClicked()
        {
            AudioManager.Play2D("ui_confirm", 0.8f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ContinueGame();
            }
        }

        private void OnMainMenuClicked()
        {
            AudioManager.Play2D("ui_back", 0.8f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMainMenu();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null)
            {
                GameState st = GameManager.Instance.State;
                if (st != GameState.Playing && st != GameState.Paused && st != GameState.Dead)
                {
                    return;
                }
            }

            UpdateVitalsAndAmmo();
            UpdateTimers();
            PollObjective();
        }

        private void UpdateVitalsAndAmmo()
        {
            PlayerController pc = PlayerController.Instance;
            if (pc != null && pc.Stats != null)
            {
                PlayerStats s = pc.Stats;
                _healthFill.fillAmount = Mathf.Clamp01(s.Health / Mathf.Max(1f, s.MaxHealth));
                _staminaFill.fillAmount = Mathf.Clamp01(s.Stamina / Mathf.Max(1f, s.MaxStamina));
                _hungerFill.fillAmount = Mathf.Clamp01(s.Hunger / 100f);
                _thirstFill.fillAmount = Mathf.Clamp01(s.Thirst / 100f);
                _infectionFill.fillAmount = Mathf.Clamp01(s.Infection / 100f);
                if (_healthValueText != null)
                {
                    _healthValueText.text = Mathf.CeilToInt(s.Health) + "/" + Mathf.RoundToInt(s.MaxHealth);
                }
            }

            if (pc != null && pc.Weapons != null)
            {
                if (_boundWeapons != pc.Weapons)
                {
                    UnbindWeapons();
                    _boundWeapons = pc.Weapons;
                    _boundWeapons.AmmoChanged += RefreshWeaponHUD;
                    RefreshWeaponHUD();
                }
            }
        }

        private void UnbindWeapons()
        {
            if (_boundWeapons != null)
            {
                _boundWeapons.AmmoChanged -= RefreshWeaponHUD;
                _boundWeapons = null;
            }
        }

        private void RefreshWeaponHUD()
        {
            if (_boundWeapons == null || string.IsNullOrEmpty(_boundWeapons.CurrentWeaponId))
            {
                _weaponNameText.text = "SİLAH YOK";
                _ammoCountText.text = "-- / --";
                return;
            }

            WeaponDef def = ItemDatabase.GetWeapon(_boundWeapons.CurrentWeaponId);
            _weaponNameText.text = def != null ? def.displayName.ToUpperInvariant() : _boundWeapons.CurrentWeaponId.ToUpperInvariant();

            if (def != null && def.melee)
            {
                _ammoCountText.text = "YAKIN";
            }
            else if (_boundWeapons.IsReloading)
            {
                _ammoCountText.text = "DOLUYOR...";
            }
            else
            {
                _ammoCountText.text = _boundWeapons.AmmoInMag + " / " + _boundWeapons.ReserveAmmo;
            }
        }

        private void UpdateTimers()
        {
            float dt = Time.unscaledDeltaTime;

            if (_toastTimer > 0f)
            {
                _toastTimer -= dt;
                if (_toastTimer <= 0f && _toastPanel != null)
                {
                    _toastPanel.gameObject.SetActive(false);
                }
            }

            if (_radioTimer > 0f)
            {
                _radioTimer -= dt;
                if (_radioTimer <= 0f && _radioPanel != null)
                {
                    _radioPanel.gameObject.SetActive(false);
                }
            }

            if (_damageFlashImage != null)
            {
                float lowHealthPulse = 0f;
                if (PlayerController.Instance != null && PlayerController.Instance.Stats != null)
                {
                    PlayerStats s = PlayerController.Instance.Stats;
                    if (!s.IsDead && s.Health < 25f)
                    {
                        lowHealthPulse = (1f - s.Health / 25f) * (0.14f + 0.08f * Mathf.Sin(Time.unscaledTime * 5f));
                    }
                }

                _damageFlashAlpha = Mathf.MoveTowards(_damageFlashAlpha, 0f, dt * 1.4f);
                float alpha = Mathf.Max(_damageFlashAlpha, lowHealthPulse);
                Color c = _damageFlashImage.color;
                c.a = alpha;
                _damageFlashImage.color = c;
            }
        }

        private void PollObjective()
        {
            _objectivePollTimer -= Time.unscaledDeltaTime;
            if (_objectivePollTimer > 0f) return;
            _objectivePollTimer = 0.5f;

            if (QuestManager.Instance != null && !string.IsNullOrEmpty(QuestManager.Instance.CurrentObjectiveText))
            {
                SetObjective(QuestManager.Instance.CurrentObjectiveText);
            }
        }

        public void SetPrompt(string text)
        {
            if (_promptPanel == null || _promptText == null) return;
            bool show = !string.IsNullOrEmpty(text);
            if (_promptPanel.gameObject.activeSelf != show)
            {
                _promptPanel.gameObject.SetActive(show);
            }
            if (show)
            {
                _promptText.text = "[E] " + text;
            }
        }

        public void ShowToast(string msg)
        {
            if (string.IsNullOrEmpty(msg) || _toastPanel == null || _toastText == null) return;
            _toastText.text = msg;
            _toastPanel.gameObject.SetActive(true);
            _toastTimer = 3.5f;
            AudioManager.Play2D("ui_toast", 0.5f);
        }

        public void ShowRadio(string speaker, string text)
        {
            if (string.IsNullOrEmpty(text) || _radioPanel == null) return;
            _radioSpeakerText.text = "[TELSİZ] " + (string.IsNullOrEmpty(speaker) ? "Bilinmeyen" : speaker);
            _radioBodyText.text = text;
            _radioPanel.gameObject.SetActive(true);
            _radioTimer = Mathf.Clamp(3.5f + text.Length * 0.05f, 4.5f, 9.5f);
        }

        public void SetObjective(string text)
        {
            if (_objectiveText == null) return;
            _objectiveText.text = string.IsNullOrEmpty(text) ? string.Empty : ("Görev: " + text);
        }

        private void OnFloorChanged(int oldFloor, int newFloor)
        {
            if (_floorTitleText == null) return;
            FloorInfo info = FloorCatalog.Get(newFloor);
            string label = GameConstants.FloorLabel(newFloor);
            if (info != null)
            {
                _floorTitleText.text = label + " — " + info.name;
                _floorSubText.text = "Tehlike Seviyesi: " + info.danger + "/5";
            }
            else
            {
                _floorTitleText.text = label;
                _floorSubText.text = string.Empty;
            }
        }

        private void OnPlayerDamaged(float amount)
        {
            _damageFlashAlpha = Mathf.Clamp(_damageFlashAlpha + Mathf.Clamp(amount / 45f, 0.18f, 0.65f), 0f, 0.72f);
        }

        private void OnStateChanged(GameState state)
        {
            UpdateVisibility(state);
        }

        private void UpdateVisibility(GameState state)
        {
            if (_hudRoot != null)
            {
                _hudRoot.SetActive(state == GameState.Playing || state == GameState.Paused);
            }
            if (_deathPanel != null)
            {
                _deathPanel.SetActive(state == GameState.Dead);
            }
        }
    }
}
