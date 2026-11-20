// Runtime uGUI helpers: every panel in the game is built from code so no scene/prefab wiring is required.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LayeredHell
{
    public static class UIFactory
    {
        private static Font _font;
        public static Font DefaultFont
        {
            get
            {
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static readonly Color PanelColor = new Color(0.04f, 0.05f, 0.06f, 0.88f);
        public static readonly Color AccentColor = new Color(0.85f, 0.35f, 0.12f, 1f);
        public static readonly Color TextColor = new Color(0.9f, 0.9f, 0.88f, 1f);

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            Object.DontDestroyOnLoad(go);
        }

        public static Canvas CreateCanvas(string name, int sortOrder, bool dontDestroy = true)
        {
            EnsureEventSystem();
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            if (dontDestroy) Object.DontDestroyOnLoad(go);
            return canvas;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        public static RectTransform Stretch(RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        public static RectTransform Anchor(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = pivot;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            return rt;
        }

        public static Text CreateText(Transform parent, string text, int size = 28, TextAnchor anchor = TextAnchor.MiddleLeft, Color? color = null)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = DefaultFont; t.text = text; t.fontSize = size; t.alignment = anchor;
            t.color = color ?? TextColor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button CreateButton(Transform parent, string label, UnityAction onClick, Vector2 size, int fontSize = 28)
        {
            var go = new GameObject("Button_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = size;
            var img = go.GetComponent<Image>(); img.color = new Color(0.14f, 0.16f, 0.18f, 0.95f);
            var btn = go.GetComponent<Button>(); btn.targetGraphic = img;
            var cb = btn.colors; cb.highlightedColor = AccentColor; cb.pressedColor = AccentColor * 0.7f; btn.colors = cb;
            var txt = CreateText(go.transform, label, fontSize, TextAnchor.MiddleCenter);
            Stretch(txt.rectTransform, 6, 2, 6, 2);
            if (onClick != null) btn.onClick.AddListener(onClick);
            var le = go.AddComponent<LayoutElement>(); le.minHeight = size.y; le.preferredHeight = size.y;
            return btn;
        }

        public static ScrollRect CreateScrollList(Transform parent, out RectTransform content, float spacing = 8f)
        {
            var root = CreatePanel(parent, "Scroll", new Color(0, 0, 0, 0.25f));
            var sr = root.gameObject.AddComponent<ScrollRect>();
            var viewport = CreatePanel(root, "Viewport", new Color(1, 1, 1, 0.02f));
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(0.5f, 1);
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing; vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.viewport = viewport; sr.content = content; sr.horizontal = false; sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic; sr.scrollSensitivity = 30f;
            return sr;
        }

        public static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) Object.Destroy(t.GetChild(i).gameObject);
        }
    }
}
