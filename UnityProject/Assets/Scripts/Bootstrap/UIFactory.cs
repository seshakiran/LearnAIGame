using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace LearnAIGame.Bootstrap
{
    /// Small helpers for building the spike's fully code-driven UI —
    /// no hand-authored scenes/prefabs required to get the loop running.
    public static class UIFactory
    {
        private static readonly Dictionary<int, Sprite> RoundedSpriteCache = new Dictionary<int, Sprite>();
        private static Font _playfulFont;

        // Reverted from an OS-dynamic-font lookup (Chalkboard SE etc.) — combined with
        // FontStyle.Bold on a font with no real bold weight, Unity synthesizes bold by
        // smearing the outline, which turns to mush at these font sizes. A real playful
        // font needs to be a bundled OFL-licensed TTF (e.g. Baloo 2 / Fredoka) imported as
        // a project asset, not looked up dynamically from the OS.
        public static Font GetPlayfulFont()
        {
            if (_playfulFont != null) return _playfulFont;
            _playfulFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _playfulFont;
        }

        public static Canvas CreateRootCanvas()
        {
            var canvasGo = new GameObject("RootCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            var esGo = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            esGo.AddComponent<InputSystemUIInputModule>();
#else
            esGo.AddComponent<StandaloneInputModule>();
#endif
        }

        /// A rounded-rect sprite, sliced so any RectTransform size keeps a fixed
        /// corner radius. Generated procedurally and cached per radius — no art asset needed.
        public static Sprite GetRoundedSprite(int radius = 32)
        {
            if (RoundedSpriteCache.TryGetValue(radius, out var cached)) return cached;

            var size = radius * 4;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var cx = Mathf.Clamp(x, radius, size - radius - 1);
                    var cy = Mathf.Clamp(y, radius, size - radius - 1);
                    var dx = x - cx;
                    var dy = y - cy;
                    var inside = dx * dx + dy * dy <= radius * radius;
                    pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            RoundedSpriteCache[radius] = sprite;
            return sprite;
        }

        public static RectTransform CreateFullScreenPanel(Transform parent, Color color, string name = "Panel")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return rect;
        }

        /// A rounded, colored surface (card backs, chips, panels-within-panels).
        public static Image CreateSurface(Transform parent, Color color, Vector2 anchoredPos, Vector2 size, int cornerRadius = 32, string name = "Surface")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.sprite = GetRoundedSprite(cornerRadius);
            image.type = Image.Type.Sliced;
            image.color = color;
            return image;
        }

        public static Text CreateLabel(Transform parent, string text, int fontSize, Vector2 anchoredPos, Vector2 size,
            TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal, Color? color = null,
            bool autoShrink = false, int minFontSize = 14)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var label = go.AddComponent<Text>();
            label.text = text;
            label.font = GetPlayfulFont();
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = anchor;
            label.color = color ?? Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;

            // Long, AI-scenario-length text needs to shrink to fit its box rather than
            // overflow into neighboring UI (badges, buttons) — see UI Feedback screenshot.
            if (autoShrink)
            {
                label.verticalOverflow = VerticalWrapMode.Truncate;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = minFontSize;
                label.resizeTextMaxSize = fontSize;
            }
            else
            {
                label.verticalOverflow = VerticalWrapMode.Overflow;
            }

            return label;
        }

        /// Chunky, Kahoot/Duolingo-style button: flat rounded face over a darker
        /// "bevel" shadow layer so it reads as a big, pressable game button.
        public static Button CreateButton(Transform parent, string label, Vector2 anchoredPos, Vector2 size,
            System.Action onClick, Color? baseColor = null, Color? textColor = null)
        {
            var color = baseColor ?? GamePalette.Lime;
            var shadowColor = GamePalette.Darken(color, 0.6f);
            const float bevel = 10f;

            var root = new GameObject("ButtonRoot", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPos;
            rootRect.sizeDelta = size;

            var shadowGo = new GameObject("ButtonShadow", typeof(RectTransform), typeof(Image));
            shadowGo.transform.SetParent(root.transform, false);
            var shadowRect = shadowGo.GetComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = Vector2.zero;
            shadowRect.offsetMax = Vector2.zero;
            var shadowImg = shadowGo.GetComponent<Image>();
            shadowImg.sprite = GetRoundedSprite(24);
            shadowImg.type = Image.Type.Sliced;
            shadowImg.color = shadowColor;

            var faceGo = new GameObject("ButtonFace", typeof(RectTransform), typeof(Image), typeof(Button));
            faceGo.transform.SetParent(root.transform, false);
            var faceRect = faceGo.GetComponent<RectTransform>();
            faceRect.anchorMin = Vector2.zero;
            faceRect.anchorMax = Vector2.one;
            faceRect.offsetMin = new Vector2(0, bevel);
            faceRect.offsetMax = Vector2.zero;
            var faceImg = faceGo.GetComponent<Image>();
            faceImg.sprite = GetRoundedSprite(24);
            faceImg.type = Image.Type.Sliced;
            faceImg.color = color;

            var button = faceGo.GetComponent<Button>();
            button.targetGraphic = faceImg;
            button.onClick.AddListener(() => onClick?.Invoke());

            CreateLabel(faceGo.transform, label, 30, Vector2.zero, size, TextAnchor.MiddleCenter, FontStyle.Bold, textColor ?? Color.white);

            return button;
        }
    }
}
