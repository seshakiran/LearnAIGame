using UnityEngine;
using UnityEngine.UI;

namespace LearnAIGame.Story
{
    /// Fits the illustration without distortion. Full-screen uses a centered crop;
    /// comic panels preserve the whole composition and size to their actual width.
    [RequireComponent(typeof(RawImage))]
    public class StoryArtwork : MonoBehaviour
    {
        public bool fullScreen;
        public float focusX = .5f;
        private RawImage picture;
        private RectTransform rect;
        private LayoutElement layout;

        private void Awake()
        {
            picture = GetComponent<RawImage>();
            rect = GetComponent<RectTransform>();
            layout = GetComponentInParent<LayoutElement>();
        }

        private void LateUpdate()
        {
            if (picture.texture == null || rect.rect.width <= 0) return;
            float aspect = (float)picture.texture.width / picture.texture.height;
            if (!fullScreen && layout != null)
            {
                float width = ((RectTransform)layout.transform).rect.width;
                float height = Mathf.Clamp(width / aspect, 140, 240);
                if (Mathf.Abs(layout.preferredHeight - height) > .5f) layout.preferredHeight = height;
                picture.uvRect = new Rect(0, 0, 1, 1);
            }
            else if (rect.rect.height > 0)
            {
                float viewAspect = rect.rect.width / rect.rect.height;
                if (viewAspect < aspect)
                {
                    float width = viewAspect / aspect;
                    picture.uvRect = new Rect(Mathf.Clamp(focusX - width / 2, 0, 1 - width), 0, width, 1);
                }
                else
                {
                    float height = aspect / viewAspect;
                    picture.uvRect = new Rect(0, (1 - height) / 2, 1, height);
                }
            }
        }
    }
}
