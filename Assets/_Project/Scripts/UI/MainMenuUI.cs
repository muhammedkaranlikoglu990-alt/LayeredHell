// Runtime Main Menu screen built with UIFactory: New Game, Continue, Settings, and Quit in Turkish.
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LayeredHell
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        public static MainMenuUI Instance { get; private set; }

        private Canvas _canvas;
        private GameObject _rootPanel;
        private GameObject _settingsPanel;
        private Button _continueButton;

        private Text _sensLabel;
        private Text _volLabel;
        private Text _fovLabel;

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
            GameEvents.StateChanged += OnStateChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildUI()
        {
            _canvas = UIFactory.CreateCanvas("MainMenu_Canvas", 40, true);

            RectTransform bg = UIFactory.CreatePanel(_canvas.transform, "MainMenuRoot", new Color(0.03f, 0.04f, 0.05f, 0.98f));
            UIFactory.Stretch(bg);
            _rootPanel = bg.gameObject;

            // Left accent stripe
            RectTransform stripe = UIFactory.CreatePanel(bg, "AccentStripe", UIFactory.AccentColor);
            UIFactory.Anchor(stripe, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(8f, 680f));

            Text title = UIFactory.CreateText(bg, "45: KATMANLI CEHENNEM", 64, TextAnchor.MiddleLeft, UIFactory.AccentColor);
            UIFactory.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(136f, -120f), new Vector2(1100f, 80f));

            Text subtitle = UIFactory.CreateText(bg, "Kule Karantinası — Daire 38-B'den Çatıya ve Derin Laboratuvara", 26, TextAnchor.MiddleLeft, UIFactory.TextColor);
            UIFactory.Anchor(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(136f, -200f), new Vector2(1100f, 44f));

            // Button column
            RectTransform btnCol = UIFactory.CreatePanel(bg, "ButtonCol", Color.clear);
            UIFactory.Anchor(btnCol, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(136f, -40f), new Vector2(420f, 380f));
            VerticalLayoutGroup vlg = btnCol.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 18f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            _continueButton = UIFactory.CreateButton(btnCol, "DEVAM ET", OnContinueClicked, new Vector2(400f, 68f), 28);
            UIFactory.CreateButton(btnCol, "YENİ OYUN", OnNewGameClicked, new Vector2(400f, 68f), 28);
            UIFactory.CreateButton(btnCol, "AYARLAR", OnSettingsClicked, new Vector2(400f, 68f), 28);
            UIFactory.CreateButton(btnCol, "ÇIKIŞ", OnQuitClicked, new Vector2(400f, 68f), 28);

            BuildSettingsSubPanel(bg);

            Text footer = UIFactory.CreateText(bg, "Chronos Bio-Labs Karantina Protokolü // Sürüm 1.0", 18, TextAnchor.LowerRight, new Color(0.55f, 0.55f, 0.52f));
            UIFactory.Anchor(footer.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 28f), new Vector2(700f, 30f));

            RefreshState();
        }

        private void BuildSettingsSubPanel(Transform parent)
        {
            RectTransform panel = UIFactory.CreatePanel(parent, "SettingsPanel", UIFactory.PanelColor);
            UIFactory.Anchor(panel, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, -20f), new Vector2(620f, 440f));
            _settingsPanel = panel.gameObject;

            Text header = UIFactory.CreateText(panel, "OYUN AYARLARI", 30, TextAnchor.MiddleLeft, UIFactory.AccentColor);
            UIFactory.Anchor(header.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -22f), new Vector2(560f, 40f));

            // Master Volume row
            _volLabel = UIFactory.CreateText(panel, string.Empty, 24, TextAnchor.MiddleLeft, UIFactory.TextColor);
            UIFactory.Anchor(_volLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -92f), new Vector2(320f, 44f));
            Button volMinus = UIFactory.CreateButton(panel, "-", () => AdjustVolume(-0.1f), new Vector2(72f, 48f), 28);
            UIFactory.Anchor(volMinus.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-116f, -90f), new Vector2(72f, 48f));
            Button volPlus = UIFactory.CreateButton(panel, "+", () => AdjustVolume(0.1f), new Vector2(72f, 48f), 28);
            UIFactory.Anchor(volPlus.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -90f), new Vector2(72f, 48f));

            // Look Sensitivity row
            _sensLabel = UIFactory.CreateText(panel, string.Empty, 24, TextAnchor.MiddleLeft, UIFactory.TextColor);
            UIFactory.Anchor(_sensLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -166f), new Vector2(320f, 44f));
            Button sensMinus = UIFactory.CreateButton(panel, "-", () => AdjustSensitivity(-0.15f), new Vector2(72f, 48f), 28);
            UIFactory.Anchor(sensMinus.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-116f, -164f), new Vector2(72f, 48f));
            Button sensPlus = UIFactory.CreateButton(panel, "+", () => AdjustSensitivity(0.15f), new Vector2(72f, 48f), 28);
            UIFactory.Anchor(sensPlus.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -164f), new Vector2(72f, 48f));

            // FOV row
            _fovLabel = UIFactory.CreateText(panel, string.Empty, 24, TextAnchor.MiddleLeft, UIFactory.TextColor);
            UIFactory.Anchor(_fovLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -240f), new Vector2(320f, 44f));
            Button fovMinus = UIFactory.CreateButton(panel, "-", () => AdjustFov(-5f), new Vector2(72f, 48f), 28);
            UIFactory.Anchor(fovMinus.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-116f, -238f), new Vector2(72f, 48f));
            Button fovPlus = UIFactory.CreateButton(panel, "+", () => AdjustFov(5f), new Vector2(72f, 48f), 28);
            UIFactory.Anchor(fovPlus.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -238f), new Vector2(72f, 48f));

            Button resetBtn = UIFactory.CreateButton(panel, "VARSAYILANASIFIRLA", () =>
            {
                GameSettings.ResetToDefaults();
                RefreshSettingsLabels();
                AudioManager.Play2D("ui_confirm", 0.7f);
            }, new Vector2(280f, 52f), 22);
            UIFactory.Anchor(resetBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(280f, 52f));

            RefreshSettingsLabels();
            _settingsPanel.SetActive(false);
        }

        private void AdjustVolume(float delta)
        {
            GameSettings.MasterVolume = Mathf.Clamp01(GameSettings.MasterVolume + delta);
            GameSettings.Save();
            RefreshSettingsLabels();
            AudioManager.Play2D("ui_click", 0.7f);
        }

        private void AdjustSensitivity(float delta)
        {
            GameSettings.LookSensitivity = Mathf.Clamp(GameSettings.LookSensitivity + delta, 0.2f, 4.0f);
            GameSettings.Save();
            RefreshSettingsLabels();
            AudioManager.Play2D("ui_click", 0.7f);
        }

        private void AdjustFov(float delta)
        {
            GameSettings.FieldOfView = Mathf.Clamp(GameSettings.FieldOfView + delta, 60f, 100f);
            GameSettings.Save();
            RefreshSettingsLabels();
            AudioManager.Play2D("ui_click", 0.7f);
        }

        private void RefreshSettingsLabels()
        {
            GameSettings.EnsureLoaded();
            if (_volLabel != null) _volLabel.text = "Ana Ses: %" + Mathf.RoundToInt(GameSettings.MasterVolume * 100f);
            if (_sensLabel != null) _sensLabel.text = "Bakış Hassasiyeti: " + GameSettings.LookSensitivity.ToString("0.00");
            if (_fovLabel != null) _fovLabel.text = "Görüş Açısı (FOV): " + Mathf.RoundToInt(GameSettings.FieldOfView);
        }

        private void OnContinueClicked()
        {
            AudioManager.Play2D("ui_confirm", 0.85f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ContinueGame();
            }
        }

        private void OnNewGameClicked()
        {
            AudioManager.Play2D("ui_confirm", 0.85f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
        }

        private void OnSettingsClicked()
        {
            AudioManager.Play2D("ui_click", 0.8f);
            if (_settingsPanel != null)
            {
                RefreshSettingsLabels();
                _settingsPanel.SetActive(!_settingsPanel.activeSelf);
            }
        }

        private void OnQuitClicked()
        {
            AudioManager.Play2D("ui_back", 0.8f);
            Application.Quit();
        }

        private void OnStateChanged(GameState state)
        {
            RefreshState();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu" && GameManager.Instance != null && GameManager.Instance.State != GameState.MainMenu)
            {
                GameManager.Instance.SetState(GameState.MainMenu);
            }
            RefreshState();
        }

        private void RefreshState()
        {
            bool show = (GameManager.Instance != null && GameManager.Instance.State == GameState.MainMenu) ||
                        SceneManager.GetActiveScene().name == "MainMenu";

            if (_rootPanel != null)
            {
                _rootPanel.SetActive(show);
            }

            if (show && _continueButton != null)
            {
                bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasSave(0);
                _continueButton.interactable = hasSave;
            }
        }
    }
}
