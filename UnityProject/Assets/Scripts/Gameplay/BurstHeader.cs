using LearnAIGame.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace LearnAIGame.Gameplay
{
    /// Persistent "where am I" chrome shown above the swipe cards during a burst —
    /// topic name, a card-position counter, a fill bar, and static swipe-direction
    /// hints. Playtesting showed users lost track of topic/progress with nothing
    /// but a bare card repeating on screen, and no indication swiping was the input.
    public class BurstHeader
    {
        private const float TrackWidth = 860f;

        private readonly GameObject _root;
        private readonly Text _counterLabel;
        private readonly RectTransform _fillRect;

        private BurstHeader(GameObject root, Text counterLabel, RectTransform fillRect)
        {
            _root = root;
            _counterLabel = counterLabel;
            _fillRect = fillRect;
        }

        public static BurstHeader Create(Transform canvasParent, string topicTitle)
        {
            // Full-screen, non-interactive container — everything below is positioned
            // in canvas-center-relative coordinates, same as any other full panel.
            var root = new GameObject("BurstHeader", typeof(RectTransform));
            root.transform.SetParent(canvasParent, false);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            UIFactory.CreateLabel(root.transform, topicTitle, 24, new Vector2(0, 870), new Vector2(TrackWidth - 40, 40),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight, autoShrink: true, minFontSize: 16);

            var counterLabel = UIFactory.CreateLabel(root.transform, "", 16, new Vector2(0, 840), new Vector2(TrackWidth - 40, 26),
                TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextMuted);

            var track = UIFactory.CreateSurface(root.transform, GamePalette.CardSurface, new Vector2(0, 810), new Vector2(TrackWidth, 10), 5, "ProgressTrack");

            var fillGo = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(track.transform, false);
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(0f, 0f);
            var fillImg = fillGo.GetComponent<Image>();
            fillImg.sprite = UIFactory.GetRoundedSprite(5);
            fillImg.type = Image.Type.Sliced;
            fillImg.color = GamePalette.Lime;

            // Static swipe-direction hints, pinned to the screen edges (not the card
            // itself) so they stay put while the card drags underneath them.
            var leftHint = UIFactory.CreateLabel(root.transform, "‹", 48, new Vector2(-480, -60), new Vector2(80, 100),
                TextAnchor.MiddleCenter, FontStyle.Bold, new Color(1f, 1f, 1f, 0.28f));
            leftHint.raycastTarget = false;
            var rightHint = UIFactory.CreateLabel(root.transform, "›", 48, new Vector2(480, -60), new Vector2(80, 100),
                TextAnchor.MiddleCenter, FontStyle.Bold, new Color(1f, 1f, 1f, 0.28f));
            rightHint.raycastTarget = false;
            var swipeCaption = UIFactory.CreateLabel(root.transform, "Swipe left or right to judge", 16, new Vector2(0, -770), new Vector2(700, 40),
                TextAnchor.MiddleCenter, FontStyle.Italic, GamePalette.TextMuted);
            swipeCaption.raycastTarget = false;

            return new BurstHeader(root, counterLabel, fillRect);
        }

        public void SetProgress(int current, int total)
        {
            _counterLabel.text = $"Card {current} of {total}";
            var t = total > 0 ? (float)current / total : 0f;
            _fillRect.sizeDelta = new Vector2(TrackWidth * t, 0f);
        }

        public void Destroy()
        {
            Object.Destroy(_root);
        }
    }
}
