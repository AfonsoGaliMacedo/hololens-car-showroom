using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aeroflux
{
    /// <summary>
    /// Builds a tidy world-space button panel entirely in code and exposes a
    /// simple <see cref="AddButton"/> API. Standard uGUI is used on purpose:
    /// MRTK3 drives ordinary Canvas buttons through its canvas interactor, so the
    /// same panel is pokable on HoloLens and clickable with a mouse in the Editor.
    /// </summary>
    public class AerofluxControlPanel : MonoBehaviour
    {
        private RectTransform _content;
        private readonly List<Button> _buttons = new List<Button>();

        // Aeroflux accent palette – a cool "pit-lane blue".
        private static readonly Color PanelColor = new Color(0.06f, 0.08f, 0.12f, 0.92f);
        private static readonly Color ButtonColor = new Color(0.10f, 0.45f, 0.85f, 1f);
        private static readonly Color ButtonHover = new Color(0.16f, 0.58f, 1f, 1f);
        private static readonly Color AccentColor = new Color(0.20f, 0.85f, 1f, 1f);

        /// <summary>
        /// Create a panel with the given title. The panel is a child of
        /// <paramref name="parent"/> and sized for HoloLens (about 24 cm wide).
        /// </summary>
        public static AerofluxControlPanel Create(Transform parent, string title)
        {
            EnsureEventSystem();

            var go = new GameObject("Aeroflux Control Panel", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 200f;
            scaler.referencePixelsPerUnit = 100f;

            go.AddComponent<GraphicRaycaster>();

            var panel = go.AddComponent<AerofluxControlPanel>();
            panel.BuildLayout(title);
            return panel;
        }

        private void BuildLayout(string title)
        {
            var rect = (RectTransform)transform;
            // 240 mm x 320 mm panel, 1 unit = 1 metre -> 0.24 x 0.32, drawn at 1000 px/m.
            const float pxPerM = 1000f;
            rect.sizeDelta = new Vector2(0.24f * pxPerM, 0.34f * pxPerM);
            rect.localScale = Vector3.one / pxPerM;

            // Background.
            var bg = AddImage(rect, "Background", PanelColor);
            Stretch(bg.rectTransform);
            AddRoundedLook(bg);

            // Accent bar along the top.
            var accent = AddImage(rect, "AccentBar", AccentColor);
            accent.rectTransform.anchorMin = new Vector2(0f, 1f);
            accent.rectTransform.anchorMax = new Vector2(1f, 1f);
            accent.rectTransform.pivot = new Vector2(0.5f, 1f);
            accent.rectTransform.sizeDelta = new Vector2(0f, 14f);
            accent.rectTransform.anchoredPosition = Vector2.zero;

            // Title.
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(rect, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 34;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            var tr = titleText.rectTransform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(-24f, 56f);
            tr.anchoredPosition = new Vector2(0f, -20f);

            // Vertical list that holds the buttons.
            var contentGo = new GameObject("Buttons", typeof(RectTransform));
            contentGo.transform.SetParent(rect, false);
            _content = (RectTransform)contentGo.transform;
            Stretch(_content);
            _content.offsetMin = new Vector2(16f, 16f);
            _content.offsetMax = new Vector2(-16f, -84f);

            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.padding = new RectOffset(0, 0, 0, 0);
        }

        /// <summary>Append a button with a label and a click handler.</summary>
        public Button AddButton(string label, Action onClick)
        {
            var go = new GameObject(label + " Button", typeof(RectTransform));
            go.transform.SetParent(_content, false);

            var img = go.AddComponent<Image>();
            img.color = ButtonColor;
            AddRoundedLook(img);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 64f;
            le.preferredHeight = 64f;

            var button = go.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = ButtonHover;
            colors.pressedColor = AccentColor;
            colors.selectedColor = ButtonHover;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(() => onClick());

            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 26;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            Stretch(text.rectTransform);

            _buttons.Add(button);
            return button;
        }

        // ------------------------------------------------------------- helpers

        private static Image AddImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static void AddRoundedLook(Image img)
        {
            // Use Unity's built-in rounded UI sprite if available for soft corners.
            var sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
            }
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
#if UNITY_2023_1_OR_NEWER
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
#else
            if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null) return;
#endif
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            es.hideFlags = HideFlags.DontSave;
        }
    }
}
