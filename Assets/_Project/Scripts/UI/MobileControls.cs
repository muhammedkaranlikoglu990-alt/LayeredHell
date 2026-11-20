// Runtime-built on-screen mobile controls: virtual left joystick, multi-touch right look pad, and action buttons.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LayeredHell
{
    public sealed class MobileControls : MonoBehaviour
    {
        public static MobileControls Instance { get; private set; }
        public static bool IsTouchActive => Instance != null && Instance.IsShowing;

        public bool ForceShow
        {
            get => _forceShow;
            set
            {
                _forceShow = value;
                RefreshVisibility();
            }
        }

        public bool IsShowing => _rootPanel != null && _rootPanel.gameObject.activeSelf;

        private bool _forceShow;
        private Canvas _canvas;
        private RectTransform _rootPanel;
        private Image _aimButtonImg;
        private Image _sprintButtonImg;
        private bool _aimToggled;
        private bool _sprintToggled;

        private static readonly Color BtnDefaultColor = new Color(0.10f, 0.12f, 0.14f, 0.72f);
        private static readonly Color BtnActiveColor = new Color(0.85f, 0.35f, 0.12f, 0.88f);
        private static readonly Color FireBtnColor = new Color(0.68f, 0.18f, 0.12f, 0.82f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            BuildUI();
            GameEvents.StateChanged += OnStateChanged;
            RefreshVisibility();
        }

        private void OnDestroy()
        {
            GameEvents.StateChanged -= OnStateChanged;
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            RefreshVisibility();
        }

        private void OnStateChanged(GameState state)
        {
            RefreshVisibility();
        }

        private bool ShouldShowOnPlatform()
        {
            if (_forceShow || Application.isMobilePlatform) return true;
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return false;
#endif
        }

        private void RefreshVisibility()
        {
            if (_rootPanel == null) return;
            bool playing = GameManager.Instance != null && GameManager.Instance.State == GameState.Playing;
            bool menuOpen = GameMenuUI.Instance != null && GameMenuUI.Instance.IsOpen;
            bool show = ShouldShowOnPlatform() && playing && !menuOpen;

            if (_rootPanel.gameObject.activeSelf != show)
            {
                _rootPanel.gameObject.SetActive(show);
                if (!show)
                {
                    _aimToggled = false;
                    _sprintToggled = false;
                    InputRouter.SetTouchMove(Vector2.zero);
                    InputRouter.SetTouchFire(false);
                    InputRouter.SetTouchAim(false);
                    InputRouter.SetTouchSprint(false);
                }
            }
        }

        private void BuildUI()
        {
            _canvas = UIFactory.CreateCanvas("MobileControlsCanvas", 40, true);
            _rootPanel = UIFactory.CreatePanel(_canvas.transform, "MobileRoot", Color.clear);
            UIFactory.Stretch(_rootPanel);
            _rootPanel.GetComponent<Image>().raycastTarget = false;

            // 1. Right-half multi-touch Look Pad (behind buttons so buttons take priority)
            var lookPadRt = UIFactory.CreatePanel(_rootPanel, "LookPad", new Color(0f, 0f, 0f, 0f));
            lookPadRt.anchorMin = new Vector2(0.38f, 0f);
            lookPadRt.anchorMax = new Vector2(1f, 0.88f);
            lookPadRt.offsetMin = Vector2.zero;
            lookPadRt.offsetMax = Vector2.zero;
            lookPadRt.gameObject.AddComponent<TouchLookPad>();

            // 2. Left Virtual Joystick
            var joyBase = UIFactory.CreatePanel(_rootPanel, "JoystickBase", new Color(0.08f, 0.10f, 0.12f, 0.55f));
            UIFactory.Anchor(joyBase, new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(230f, 230f), new Vector2(260f, 260f));

            var joyRing = UIFactory.CreatePanel(joyBase, "JoystickInnerRing", new Color(1f, 1f, 1f, 0.08f));
            UIFactory.Anchor(joyRing, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(190f, 190f));
            joyRing.GetComponent<Image>().raycastTarget = false;

            var joyKnob = UIFactory.CreatePanel(joyBase, "JoystickKnob", new Color(0.85f, 0.35f, 0.12f, 0.85f));
            UIFactory.Anchor(joyKnob, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(96f, 96f));
            joyKnob.GetComponent<Image>().raycastTarget = false;

            var joyLabel = UIFactory.CreateText(joyKnob, "HAREKET", 16, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(joyLabel.rectTransform);

            var joystick = joyBase.gameObject.AddComponent<VirtualJoystick>();
            joystick.Setup(joyBase, joyKnob, 105f);

            // 3. Action Buttons (Right cluster & Top utility bar)
            // Fire button (large, holdable)
            var fireRt = CreateTouchButton(_rootPanel, "ATEŞ", new Vector2(1f, 0f), new Vector2(-240f, 220f), new Vector2(170f, 170f), 28, FireBtnColor);
            var fireHold = fireRt.gameObject.AddComponent<TouchHoldButton>();
            fireHold.OnDown = () => InputRouter.SetTouchFire(true, true);
            fireHold.OnUp = () => InputRouter.SetTouchFire(false, false);

            // Secondary Left Fire button (for index-finger claw grip shooting while aiming)
            var leftFireRt = CreateTouchButton(_rootPanel, "ATEŞ", new Vector2(0f, 1f), new Vector2(140f, -180f), new Vector2(140f, 90f), 22, FireBtnColor);
            var leftFireHold = leftFireRt.gameObject.AddComponent<TouchHoldButton>();
            leftFireHold.OnDown = () => InputRouter.SetTouchFire(true, true);
            leftFireHold.OnUp = () => InputRouter.SetTouchFire(false, false);

            // Aim button (toggle)
            var aimRt = CreateTouchButton(_rootPanel, "NİŞAN", new Vector2(1f, 0f), new Vector2(-110f, 385f), new Vector2(130f, 100f), 22, BtnDefaultColor);
            _aimButtonImg = aimRt.GetComponent<Image>();
            AddTapTrigger(aimRt.gameObject, () =>
            {
                _aimToggled = !_aimToggled;
                InputRouter.SetTouchAim(_aimToggled);
                if (_aimButtonImg != null) _aimButtonImg.color = _aimToggled ? BtnActiveColor : BtnDefaultColor;
            });

            // Reload button
            var reloadRt = CreateTouchButton(_rootPanel, "ŞARJÖR", new Vector2(1f, 0f), new Vector2(-415f, 150f), new Vector2(130f, 95f), 20, BtnDefaultColor);
            AddTapTrigger(reloadRt.gameObject, InputRouter.TriggerTouchReload);

            // Interact button
            var interactRt = CreateTouchButton(_rootPanel, "ETKİLEŞİM", new Vector2(1f, 0f), new Vector2(-415f, 275f), new Vector2(150f, 100f), 20, BtnActiveColor);
            AddTapTrigger(interactRt.gameObject, InputRouter.TriggerTouchInteract);

            // Jump button
            var jumpRt = CreateTouchButton(_rootPanel, "ZIPLA", new Vector2(1f, 0f), new Vector2(-105f, 240f), new Vector2(120f, 95f), 20, BtnDefaultColor);
            AddTapTrigger(jumpRt.gameObject, InputRouter.TriggerTouchJump);

            // Crouch button
            var crouchRt = CreateTouchButton(_rootPanel, "EĞİL", new Vector2(1f, 0f), new Vector2(-105f, 120f), new Vector2(120f, 95f), 20, BtnDefaultColor);
            AddTapTrigger(crouchRt.gameObject, InputRouter.TriggerTouchCrouch);

            // Next Weapon button
            var weaponRt = CreateTouchButton(_rootPanel, "SİLAH DEĞİŞ", new Vector2(1f, 0f), new Vector2(-265f, 395f), new Vector2(145f, 90f), 18, BtnDefaultColor);
            AddTapTrigger(weaponRt.gameObject, InputRouter.TriggerTouchNextWeapon);

            // Sprint toggle above joystick
            var sprintRt = CreateTouchButton(_rootPanel, "KOŞ", new Vector2(0f, 0f), new Vector2(230f, 415f), new Vector2(135f, 80f), 20, BtnDefaultColor);
            _sprintButtonImg = sprintRt.GetComponent<Image>();
            AddTapTrigger(sprintRt.gameObject, () =>
            {
                _sprintToggled = !_sprintToggled;
                InputRouter.SetTouchSprint(_sprintToggled);
                if (_sprintButtonImg != null) _sprintButtonImg.color = _sprintToggled ? BtnActiveColor : BtnDefaultColor;
            });

            // Quick Heal button
            var healRt = CreateTouchButton(_rootPanel, "SAĞLIK", new Vector2(0f, 0f), new Vector2(420f, 120f), new Vector2(130f, 85f), 20, new Color(0.18f, 0.45f, 0.22f, 0.80f));
            AddTapTrigger(healRt.gameObject, InputRouter.TriggerTouchQuickHeal);

            // Flashlight button
            var flashRt = CreateTouchButton(_rootPanel, "FENER", new Vector2(0f, 0f), new Vector2(420f, 225f), new Vector2(130f, 85f), 20, BtnDefaultColor);
            AddTapTrigger(flashRt.gameObject, InputRouter.TriggerTouchFlashlight);

            // Top-right Menu button
            var menuRt = CreateTouchButton(_rootPanel, "MENÜ / ÇANTA", new Vector2(1f, 1f), new Vector2(-135f, -58f), new Vector2(210f, 76f), 22, BtnDefaultColor);
            AddTapTrigger(menuRt.gameObject, InputRouter.TriggerTouchMenu);
        }

        private static RectTransform CreateTouchButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize, Color bgColor)
        {
            var panel = UIFactory.CreatePanel(parent, "TouchBtn_" + label, bgColor);
            UIFactory.Anchor(panel, anchor, new Vector2(0.5f, 0.5f), pos, size);
            var txt = UIFactory.CreateText(panel, label, fontSize, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(txt.rectTransform, 4, 4, 4, 4);
            return panel;
        }

        private static void AddTapTrigger(GameObject go, System.Action onTap)
        {
            var hold = go.AddComponent<TouchHoldButton>();
            hold.OnDown = onTap;
        }

        // --- Multi-touch helper components ---

        private sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            private RectTransform _baseRt;
            private RectTransform _knobRt;
            private float _radius = 100f;
            private int _activePointerId = int.MinValue;

            public void Setup(RectTransform baseRt, RectTransform knobRt, float radius)
            {
                _baseRt = baseRt;
                _knobRt = knobRt;
                _radius = Mathf.Max(20f, radius);
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                if (_activePointerId != int.MinValue) return;
                _activePointerId = eventData.pointerId;
                OnDrag(eventData);
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (eventData.pointerId != _activePointerId || _baseRt == null) return;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                {
                    Vector2 clamped = Vector2.ClampMagnitude(localPoint, _radius);
                    if (_knobRt != null) _knobRt.anchoredPosition = clamped;
                    InputRouter.SetTouchMove(clamped / _radius);
                }
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                if (eventData.pointerId != _activePointerId) return;
                _activePointerId = int.MinValue;
                if (_knobRt != null) _knobRt.anchoredPosition = Vector2.zero;
                InputRouter.SetTouchMove(Vector2.zero);
            }
        }

        private sealed class TouchLookPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            private int _primaryPointer = int.MinValue;
            private int _secondaryPointer = int.MinValue;

            public void OnPointerDown(PointerEventData eventData)
            {
                if (_primaryPointer == int.MinValue)
                {
                    _primaryPointer = eventData.pointerId;
                }
                else if (_secondaryPointer == int.MinValue)
                {
                    _secondaryPointer = eventData.pointerId;
                }
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (eventData.pointerId == _primaryPointer || eventData.pointerId == _secondaryPointer)
                {
                    InputRouter.AddTouchLook(eventData.delta);
                }
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                if (eventData.pointerId == _primaryPointer) _primaryPointer = int.MinValue;
                if (eventData.pointerId == _secondaryPointer) _secondaryPointer = int.MinValue;
            }
        }

        private sealed class TouchHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
        {
            public System.Action OnDown;
            public System.Action OnUp;
            private int _pointerId = int.MinValue;

            public void OnPointerDown(PointerEventData eventData)
            {
                if (_pointerId != int.MinValue) return;
                _pointerId = eventData.pointerId;
                OnDown?.Invoke();
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                if (eventData.pointerId != _pointerId) return;
                _pointerId = int.MinValue;
                OnUp?.Invoke();
            }
        }
    }
}
