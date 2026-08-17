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
        private readonly Text _meterLabel;
        private readonly string _meterName;
        private int _meterValue;

        private BurstHeader(GameObject root, Text counterLabel, RectTransform fillRect, Text meterLabel, string meterName, int meterStart)
        {
            _root = root;
            _counterLabel = counterLabel;
            _fillRect = fillRect;
            _meterLabel = meterLabel;
            _meterName = meterName;
            _meterValue = meterStart;
            RenderMeter();
        }

        /// meterName/meterStart drive the live incident meter (grounded-incident
        /// framing pass, PLAN.md §13 — Deep Trace's "Queue Pressure" pattern) —
        /// pass null/empty meterName to omit it for a burst that has none.
        public static BurstHeader Create(Transform canvasParent, string topicTitle, string meterName = null, int meterStart = 50)
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

            // Fixed offsets below were tuned for a tall phone aspect. Clamped against the
            // root's actual rendered height so the header/hints stay on-screen on a
            // shorter/wider viewport instead of rendering above/below the visible area
            // (see TopicPathScreen for the same fix and the reasoning).
            var halfHeight = Mathf.Max(rootRect.rect.height / 2f, 480f);
            var titleY = Mathf.Min(870f, halfHeight - 30f);
            var counterY = Mathf.Min(840f, halfHeight - 60f);
            var trackY = Mathf.Min(810f, halfHeight - 90f);
            var captionY = Mathf.Max(-770f, -halfHeight + 40f);

            UIFactory.CreateLabel(root.transform, topicTitle, 24, new Vector2(0, titleY), new Vector2(TrackWidth - 40, 40),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight, autoShrink: true, minFontSize: 16);

            // Counter and meter share one row (rather than each getting their own) since
            // tight/non-portrait aspects leave very little vertical room between this
            // header and the card below it — see the halfHeight clamp above.
            var counterLabel = UIFactory.CreateLabel(root.transform, "", 16, new Vector2(-(TrackWidth - 40) / 4f, counterY), new Vector2((TrackWidth - 40) / 2f, 26),
                TextAnchor.MiddleLeft, FontStyle.Normal, GamePalette.TextMuted);

            Text meterLabel = null;
            if (!string.IsNullOrEmpty(meterName))
            {
                meterLabel = UIFactory.CreateLabel(root.transform, "", 16, new Vector2((TrackWidth - 40) / 4f, counterY), new Vector2((TrackWidth - 40) / 2f, 26),
                    TextAnchor.MiddleRight, FontStyle.Bold, GamePalette.TextMuted);
            }

            var track = UIFactory.CreateSurface(root.transform, GamePalette.CardSurface, new Vector2(0, trackY), new Vector2(TrackWidth, 10), 5, "ProgressTrack");

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
            var halfWidth = Mathf.Max(rootRect.rect.width / 2f, 380f);
            var hintX = Mathf.Min(480f, halfWidth - 50f);

            var leftHint = UIFactory.CreateLabel(root.transform, "‹", 48, new Vector2(-hintX, -60), new Vector2(80, 100),
                TextAnchor.MiddleCenter, FontStyle.Bold, new Color(1f, 1f, 1f, 0.28f));
            leftHint.raycastTarget = false;
            var rightHint = UIFactory.CreateLabel(root.transform, "›", 48, new Vector2(hintX, -60), new Vector2(80, 100),
                TextAnchor.MiddleCenter, FontStyle.Bold, new Color(1f, 1f, 1f, 0.28f));
            rightHint.raycastTarget = false;
            var swipeCaption = UIFactory.CreateLabel(root.transform, "Swipe left or right to judge", 16, new Vector2(0, captionY), new Vector2(700, 40),
                TextAnchor.MiddleCenter, FontStyle.Italic, GamePalette.TextMuted);
            swipeCaption.raycastTarget = false;

            return new BurstHeader(root, counterLabel, fillRect, meterLabel, meterName, meterStart);
        }

        public void SetProgress(int current, int total)
        {
            _counterLabel.text = $"Card {current} of {total}";
            var t = total > 0 ? (float)current / total : 0f;
            _fillRect.sizeDelta = new Vector2(TrackWidth * t, 0f);
        }

        /// Applies a swipe-decision result to the live incident meter. Deltas are
        /// asymmetric on purpose (mirroring Deep Trace's tuning) — a wrong call should
        /// visibly cost more than a right call earns back, so the meter reads as
        /// "things you broke," not just a second score counter.
        public void ApplyMeterDelta(int delta)
        {
            if (_meterLabel == null) return;
            _meterValue = Mathf.Clamp(_meterValue + delta, 0, 100);
            RenderMeter();
        }

        private void RenderMeter()
        {
            if (_meterLabel == null) return;
            _meterLabel.text = $"{_meterName}: {_meterValue}";
            _meterLabel.color = _meterValue >= 70 ? GamePalette.Rose : _meterValue >= 40 ? GamePalette.Amber : GamePalette.Lime;
        }

        public void Destroy()
        {
            Object.Destroy(_root);
        }
    }
}
