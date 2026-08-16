using System.Collections;
using LearnAIGame.Bootstrap;
using LearnAIGame.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LearnAIGame.Gameplay
{
    /// Runtime-built swipe card: drag left/right past a threshold to commit a judgment.
    /// Built entirely in code so it doesn't depend on a hand-authored prefab/scene.
    /// Visual layering follows the "LearnAIGame Swipe-Judgment Card — Visual System
    /// Specification" (Manus, 2026-08-16): nested inset rounded images simulate
    /// borders/shadows/gradients without needing a custom shader or art asset.
    public class SwipeCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event System.Action<SwipeSide> OnSwiped;

        private const float SwipeThreshold = 160f;
        private const float StampDragRange = 140f;
        private const float TiltDragRange = 240f;
        private const float MaxTiltDegrees = 7f;
        private const float SnapBackSeconds = 0.22f;
        private const float FlyOutSeconds = 0.2f;

        // Taller than the original trivia-card sizing — the AI-scenario prompts run
        // much longer than "When was the Eiffel Tower completed?" and need the room.
        private static readonly Vector2 CardSize = new Vector2(640, 760);

        private RectTransform _rect;
        private Vector2 _dragStartPointerPos;
        private Vector2 _cardStartAnchoredPos;
        private bool _committed;
        private bool _animatingSnapBack;

        private ChipRefs _chipA;
        private ChipRefs _chipB;
        private StampRefs _stampA;
        private StampRefs _stampB;

        private struct ChipRefs
        {
            public CanvasGroup group;
            public Transform scaleRoot;
            public Image contour;
            public Image glow;
        }

        private struct StampRefs
        {
            public CanvasGroup group;
            public RectTransform rect;
            public Image halo;
            public Vector2 basePos;
        }

        public static SwipeCardView Create(Transform parent, JudgmentCard card, string eyebrow = null)
        {
            var root = new GameObject($"Card_{card.id}", typeof(RectTransform));
            root.transform.SetParent(parent, false);

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = CardSize;
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;

            var view = root.AddComponent<SwipeCardView>();
            view._rect = rootRect;

            // Back to front: ambient color lift, two stepped contact shadows, then the
            // bordered card face itself (outer contour -> dark recess -> face fill).
            var ambient = UIFactory.CreateSurface(root.transform, GamePalette.WithAlpha(GamePalette.Blue, 0.15f),
                new Vector2(0, -4), new Vector2(664, 788), 50, "AmbientLift");
            ambient.raycastTarget = false;

            var farShadow = UIFactory.CreateSurface(root.transform, GamePalette.WithAlpha(GamePalette.BackgroundDeep, 0.68f),
                new Vector2(0, -26), new Vector2(660, 782), 46, "FarShadow");
            farShadow.raycastTarget = false;

            var nearShadow = UIFactory.CreateSurface(root.transform, GamePalette.WithAlpha(GamePalette.BackgroundDeep, 0.74f),
                new Vector2(0, -11), new Vector2(650, 772), 41, "NearShadow");
            nearShadow.raycastTarget = false;

            var outerContour = UIFactory.CreateSurface(root.transform, GamePalette.WithAlpha(GamePalette.TextLight, 0.28f),
                Vector2.zero, CardSize, 36, "OuterContour");
            outerContour.raycastTarget = false;

            var edgeRecess = UIFactory.CreateSurface(root.transform, GamePalette.ScreenSurface,
                Vector2.zero, new Vector2(637, 757), 35, "EdgeRecess");
            edgeRecess.raycastTarget = false;

            var faceGo = new GameObject("CardFace", typeof(RectTransform), typeof(Image));
            faceGo.transform.SetParent(root.transform, false);
            var faceRect = faceGo.GetComponent<RectTransform>();
            faceRect.anchorMin = faceRect.anchorMax = new Vector2(0.5f, 0.5f);
            faceRect.anchoredPosition = Vector2.zero;
            var faceSize = new Vector2(634, 754);
            faceRect.sizeDelta = faceSize;
            var faceImg = faceGo.GetComponent<Image>();
            faceImg.sprite = UIFactory.GetRoundedSprite(33);
            faceImg.type = Image.Type.Sliced;
            faceImg.color = GamePalette.CardSurface;

            // The spec's gradient approximation (clipped top/bottom wash overlays) sits
            // directly behind the prompt/chip text with no clip mask to soften it, so any
            // visible alpha step reads as a stray highlighted rectangle rather than a
            // gradient. A flat face color avoids that seam entirely and still matches the
            // reference mockup's actual (very subtle) contrast.
            var faceHalfHeight = faceSize.y / 2f;
            var faceHalfWidth = faceSize.x / 2f;

            // Lay out top-down from a cursor so the eyebrow/tick/prompt never overlap
            // regardless of whether the eyebrow is present.
            var cursorTop = faceHalfHeight - 34f;

            if (!string.IsNullOrEmpty(eyebrow))
            {
                const float eyebrowHeight = 22f;
                UIFactory.CreateLabel(faceGo.transform, eyebrow.ToUpperInvariant(), 16, new Vector2(0, cursorTop - eyebrowHeight / 2f), new Vector2(faceSize.x - 84, eyebrowHeight),
                    TextAnchor.MiddleLeft, FontStyle.Bold, GamePalette.WithAlpha(GamePalette.TextMuted, 0.72f));
                cursorTop -= eyebrowHeight + 10f;

                const float tickHeight = 3f;
                var eyebrowTick = UIFactory.CreateSurface(faceGo.transform, GamePalette.Lime,
                    new Vector2(-faceHalfWidth + 42f + 16f, cursorTop - tickHeight / 2f), new Vector2(32, tickHeight), 0, "EyebrowTick");
                eyebrowTick.raycastTarget = false;
                cursorTop -= tickHeight + 22f;
            }

            const float promptHeight = 220f;
            UIFactory.CreateLabel(faceGo.transform, card.prompt, 40, new Vector2(0, cursorTop - promptHeight / 2f), new Vector2(faceSize.x - 84, promptHeight),
                TextAnchor.UpperLeft, FontStyle.Bold, GamePalette.TextLight, autoShrink: true, minFontSize: 26);

            const float chipW = 270f, chipH = 146f;
            var chipY = -faceHalfHeight + 42f + chipH / 2f;
            var chipAX = -faceHalfWidth + 42f + chipW / 2f;
            var chipBX = faceHalfWidth - 42f - chipW / 2f;

            view._chipA = BuildChip(faceGo.transform, "A", card.optionA, new Vector2(chipAX, chipY), GamePalette.ChoiceA, badgeAtRightEdge: false);
            view._chipB = BuildChip(faceGo.transform, "B", card.optionB, new Vector2(chipBX, chipY), GamePalette.ChoiceB, badgeAtRightEdge: true);

            // Tinder-style drag stamps — hidden at rest, fade/scale/settle in toward
            // whichever side the card is dragged, confirming direction (never outcome).
            var stampAPos = new Vector2(-faceHalfWidth + 34f + 59f, faceHalfHeight - 34f - 41f);
            var stampBPos = new Vector2(faceHalfWidth - 34f - 59f, faceHalfHeight - 34f - 41f);
            view._stampA = BuildStamp(faceGo.transform, "A", "CHOOSE A", stampAPos, GamePalette.ChoiceA, -7f);
            view._stampB = BuildStamp(faceGo.transform, "B", "CHOOSE B", stampBPos, GamePalette.ChoiceB, 7f);

            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            return view;
        }

        private static ChipRefs BuildChip(Transform parent, string letter, string text, Vector2 anchoredPos, Color accent, bool badgeAtRightEdge)
        {
            const float chipW = 270f, chipH = 146f;

            var chipRoot = new GameObject($"Chip{letter}", typeof(RectTransform));
            chipRoot.transform.SetParent(parent, false);
            var chipRootRect = chipRoot.GetComponent<RectTransform>();
            chipRootRect.anchorMin = chipRootRect.anchorMax = new Vector2(0.5f, 0.5f);
            chipRootRect.anchoredPosition = anchoredPos;
            chipRootRect.sizeDelta = new Vector2(chipW, chipH);
            var chipGroup = chipRoot.AddComponent<CanvasGroup>();

            var shadow = UIFactory.CreateSurface(chipRoot.transform, GamePalette.WithAlpha(GamePalette.BackgroundDeep, 0.74f),
                new Vector2(0, -8), new Vector2(chipW + 4, chipH + 6), 24, "ChipShadow");
            shadow.raycastTarget = false;

            var glow = UIFactory.CreateSurface(chipRoot.transform, GamePalette.WithAlpha(accent, 0.16f),
                Vector2.zero, new Vector2(chipW + 8, chipH + 10), 26, "ChoiceGlow");
            glow.raycastTarget = false;

            var contour = UIFactory.CreateSurface(chipRoot.transform, GamePalette.WithAlpha(accent, 0.64f),
                Vector2.zero, new Vector2(chipW, chipH), 22, "AccentContour");
            contour.raycastTarget = false;

            var recess = UIFactory.CreateSurface(chipRoot.transform, GamePalette.ScreenSurface,
                Vector2.zero, new Vector2(chipW - 3, chipH - 3), 21, "ChipRecess");
            recess.raycastTarget = false;

            var face = UIFactory.CreateSurface(chipRoot.transform, GamePalette.CardSurface,
                Vector2.zero, new Vector2(chipW - 6, chipH - 6), 20, "ChipFace");
            face.raycastTarget = false;

            UIFactory.CreateLabel(face.transform, text, 25, new Vector2(12, 0), new Vector2(chipW - 66, chipH - 20),
                TextAnchor.MiddleLeft, FontStyle.Bold, GamePalette.WithAlpha(GamePalette.TextLight, 0.96f), autoShrink: true, minFontSize: 22);

            // Badge sits on the chip's own outer top corner (away from the other chip),
            // overlapping the top edge — matches the Manus reference mockup, not the
            // spec doc's "inner side" text, which the mockup itself doesn't follow.
            var badgeX = badgeAtRightEdge ? chipW / 2f - 26f : -(chipW / 2f - 26f);
            var badgeY = chipH / 2f + 4f;

            var badgeShadow = UIFactory.CreateSurface(chipRoot.transform, GamePalette.WithAlpha(GamePalette.BackgroundDeep, 0.88f),
                new Vector2(badgeX, badgeY - 5), new Vector2(58, 58), 29, "BadgeShadow");
            badgeShadow.raycastTarget = false;
            var badgeHalo = UIFactory.CreateSurface(chipRoot.transform, GamePalette.WithAlpha(accent, 0.18f),
                new Vector2(badgeX, badgeY), new Vector2(64, 64), 32, "BadgeHalo");
            badgeHalo.raycastTarget = false;
            // Hollow ring: an accent-filled circle with a slightly smaller dark circle
            // on top leaves only a ~3pt accent rim, with the glyph in accent (not dark).
            var badgeRing = UIFactory.CreateSurface(chipRoot.transform, accent,
                new Vector2(badgeX, badgeY), new Vector2(52, 52), 26, "BadgeRing");
            badgeRing.raycastTarget = false;
            var badgeFill = UIFactory.CreateSurface(chipRoot.transform, GamePalette.ScreenSurface,
                new Vector2(badgeX, badgeY), new Vector2(46, 46), 23, "BadgeFill");
            badgeFill.raycastTarget = false;
            UIFactory.CreateLabel(badgeFill.transform, letter, 24, Vector2.zero, new Vector2(42, 42),
                TextAnchor.MiddleCenter, FontStyle.Bold, accent);

            return new ChipRefs { group = chipGroup, scaleRoot = chipRoot.transform, contour = contour, glow = glow };
        }

        private static StampRefs BuildStamp(Transform parent, string letter, string caption, Vector2 anchoredPos, Color accent, float tiltDegrees)
        {
            var stampGo = new GameObject($"Stamp{letter}", typeof(RectTransform));
            stampGo.transform.SetParent(parent, false);
            var stampRect = stampGo.GetComponent<RectTransform>();
            stampRect.anchorMin = stampRect.anchorMax = new Vector2(0.5f, 0.5f);
            stampRect.anchoredPosition = anchoredPos;
            stampRect.sizeDelta = new Vector2(118, 82);
            stampRect.localRotation = Quaternion.Euler(0, 0, tiltDegrees);

            var canvasGroup = stampGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            var halo = UIFactory.CreateSurface(stampGo.transform, GamePalette.WithAlpha(accent, 0f),
                Vector2.zero, new Vector2(142, 106), 28, "StampHalo");
            halo.raycastTarget = false;

            var border = UIFactory.CreateSurface(stampGo.transform, accent, Vector2.zero, new Vector2(118, 82), 16, "StampBorder");
            border.raycastTarget = false;
            var interior = UIFactory.CreateSurface(stampGo.transform, GamePalette.WithAlpha(GamePalette.ScreenSurface, 0.76f),
                Vector2.zero, new Vector2(112, 76), 15, "StampInterior");
            interior.raycastTarget = false;

            UIFactory.CreateLabel(interior.transform, letter, 42, new Vector2(0, 8), new Vector2(100, 50),
                TextAnchor.MiddleCenter, FontStyle.Bold, accent);
            UIFactory.CreateLabel(interior.transform, caption, 11, new Vector2(0, -24), new Vector2(100, 20),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.WithAlpha(GamePalette.TextLight, 0.78f));

            return new StampRefs { group = canvasGroup, rect = stampRect, halo = halo, basePos = anchoredPos };
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_committed) return;
            _animatingSnapBack = false;
            StopAllCoroutines();
            _dragStartPointerPos = eventData.position;
            _cardStartAnchoredPos = _rect.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_committed) return;
            var delta = eventData.position - _dragStartPointerPos;
            ApplyDragVisuals(delta.x);
        }

        private void ApplyDragVisuals(float dragX)
        {
            var tiltT = Mathf.Clamp(dragX / TiltDragRange, -1f, 1f);
            _rect.anchoredPosition = _cardStartAnchoredPos + new Vector2(dragX, Mathf.Lerp(0f, 8f, Mathf.Abs(tiltT)));
            _rect.localRotation = Quaternion.Euler(0, 0, -tiltT * MaxTiltDegrees);

            var stampT = Mathf.Clamp01(Mathf.Abs(dragX) / StampDragRange);
            var smooth = stampT * stampT * (3f - 2f * stampT);
            var draggingLeft = dragX < 0f;

            ApplyStamp(_stampA, draggingLeft ? smooth : 0f);
            ApplyStamp(_stampB, !draggingLeft && dragX > 0f ? smooth : 0f);

            ApplyChip(_chipA, GamePalette.ChoiceA, draggingLeft ? smooth : 0f, isDimmed: !draggingLeft && dragX != 0f ? smooth : 0f);
            ApplyChip(_chipB, GamePalette.ChoiceB, !draggingLeft && dragX > 0f ? smooth : 0f, isDimmed: draggingLeft ? smooth : 0f);
        }

        private static void ApplyStamp(StampRefs stamp, float t)
        {
            stamp.group.alpha = Mathf.Lerp(0f, 0.96f, t);
            stamp.rect.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, t);
            stamp.rect.anchoredPosition = stamp.basePos + new Vector2(0, Mathf.Lerp(8f, 0f, t));
            var haloColor = stamp.halo.color;
            haloColor.a = Mathf.Lerp(0f, 0.18f, t);
            stamp.halo.color = haloColor;
        }

        private static void ApplyChip(ChipRefs chip, Color accent, float matchT, float isDimmed)
        {
            var contourColor = chip.contour.color;
            contourColor.a = Mathf.Lerp(0.64f, 1f, matchT);
            chip.contour.color = contourColor;

            var glowColor = chip.glow.color;
            glowColor.a = Mathf.Lerp(0.16f, 0.28f, matchT);
            chip.glow.color = glowColor;

            chip.scaleRoot.localScale = Vector3.one * Mathf.Lerp(1f, 1.025f, matchT);
            chip.group.alpha = Mathf.Lerp(1f, 0.72f, isDimmed);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_committed) return;
            var xOffset = _rect.anchoredPosition.x - _cardStartAnchoredPos.x;

            if (Mathf.Abs(xOffset) >= SwipeThreshold)
            {
                Commit(xOffset > 0 ? SwipeSide.Right : SwipeSide.Left);
            }
            else
            {
                StartCoroutine(SnapBack());
            }
        }

        private IEnumerator SnapBack()
        {
            _animatingSnapBack = true;
            var startDragX = _rect.anchoredPosition.x - _cardStartAnchoredPos.x;
            var elapsed = 0f;

            while (elapsed < SnapBackSeconds)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / SnapBackSeconds);
                var eased = 1f - (1f - t) * (1f - t);
                ApplyDragVisuals(Mathf.Lerp(startDragX, 0f, eased));
                yield return null;
            }

            ApplyDragVisuals(0f);
            _animatingSnapBack = false;
        }

        private void Commit(SwipeSide side)
        {
            _committed = true;
            StopAllCoroutines();
            StartCoroutine(FlyOut(side));
        }

        private IEnumerator FlyOut(SwipeSide side)
        {
            var startPos = _rect.anchoredPosition;
            var direction = side == SwipeSide.Right ? 1f : -1f;
            var targetX = startPos.x + direction * Screen.width * 1.25f;
            var lockedRotation = _rect.localRotation;
            var elapsed = 0f;

            while (elapsed < FlyOutSeconds)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / FlyOutSeconds);
                _rect.anchoredPosition = new Vector2(Mathf.Lerp(startPos.x, targetX, t), _rect.anchoredPosition.y);
                _rect.localRotation = lockedRotation;
                yield return null;
            }

            OnSwiped?.Invoke(side);
        }
    }
}
