using UnityEngine;
using UnityEngine.UI;

namespace LearnAIGame.Story
{
    /// Logical reading units, independent of the original 900-unit prototype.
    /// Phone DPI is normalized to density-independent units; desktop uses a bounded
    /// column. Re-evaluated on resize, rotation, and changes in the device safe area.
    public class StoryResponsiveLayout : MonoBehaviour
    {
        private RectTransform rect;
        private Canvas canvas;
        private CanvasScaler scaler;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            scaler = canvas.GetComponent<CanvasScaler>();
            Apply();
        }

        private void LateUpdate() => Apply();

        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            bool phone = Application.isMobilePlatform;
            float density = Screen.dpi > 0 ? Screen.dpi / (phone ? 160f : 96f) : 1f;
            float scale = phone ? Mathf.Clamp(density, 1f, 4f) : Mathf.Clamp(density, 1f, 2f);
            if (Application.isEditor) scale = 1f;
            scale = Mathf.Min(scale, Mathf.Max(1f, Screen.width / 320f));
            if (phone && Screen.dpi <= 0) scale = Mathf.Max(1f, Screen.width / 390f);
            // Very small windows must scroll vertically rather than shrinking type.
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = scale;
            var safe = Screen.safeArea;
            float columnPixels = Mathf.Min(safe.width, 680f * scale);
            float x = safe.x + (safe.width - columnPixels) / 2;
            var min = new Vector2(x / Screen.width, safe.yMin / Screen.height);
            var max = new Vector2((x + columnPixels) / Screen.width, safe.yMax / Screen.height);
            if (rect.anchorMin != min) rect.anchorMin = min;
            if (rect.anchorMax != max) rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
