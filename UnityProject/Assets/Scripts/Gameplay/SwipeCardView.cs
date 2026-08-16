using System;
using LearnAIGame.Bootstrap;
using LearnAIGame.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LearnAIGame.Gameplay
{
    /// Runtime-built swipe card: drag left/right past a threshold to commit a judgment.
    /// Built entirely in code so it doesn't depend on a hand-authored prefab/scene.
    public class SwipeCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action<SwipeSide> OnSwiped;

        private RectTransform _rect;
        private Vector2 _dragStartPos;
        private Vector2 _cardStartAnchoredPos;
        private bool _committed;
        private CanvasGroup _leftStamp;
        private CanvasGroup _rightStamp;

        private const float SwipeThreshold = 160f;
        private const float MaxRotationDegrees = 12f;

        // Taller than the original trivia-card sizing — the AI-scenario prompts run
        // much longer than "When was the Eiffel Tower completed?" and need the room.
        private static readonly Vector2 CardSize = new Vector2(640, 760);

        public static SwipeCardView Create(Transform parent, JudgmentCard card)
        {
            var root = new GameObject($"Card_{card.id}", typeof(RectTransform));
            root.transform.SetParent(parent, false);

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = CardSize;
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;

            // Drop shadow, offset behind the card, for pop against the deep background.
            var shadow = UIFactory.CreateSurface(root.transform, GamePalette.ShadowDark, new Vector2(0, -14), CardSize, 36, "CardShadow");
            shadow.raycastTarget = false;

            var faceGo = new GameObject("CardFace", typeof(RectTransform));
            faceGo.transform.SetParent(root.transform, false);
            var faceRect = faceGo.GetComponent<RectTransform>();
            faceRect.anchorMin = faceRect.anchorMax = new Vector2(0.5f, 0.5f);
            faceRect.anchoredPosition = Vector2.zero;
            faceRect.sizeDelta = CardSize;

            var themeColor = GamePalette.CardThemeFor(card.id);

            var bg = faceGo.AddComponent<Image>();
            bg.sprite = UIFactory.GetRoundedSprite(36);
            bg.type = Image.Type.Sliced;
            bg.color = themeColor;

            var view = root.AddComponent<SwipeCardView>();
            view._rect = rootRect;

            AddLabel(faceGo.transform, "Prompt", card.prompt, 30, new Vector2(0, 200), FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(580, 260), GamePalette.TextLight, autoShrink: true, minFontSize: 20);

            BuildChoiceChip(faceGo.transform, "A", card.optionA, new Vector2(-150, -260), GamePalette.ChoiceA);
            BuildChoiceChip(faceGo.transform, "B", card.optionB, new Vector2(150, -260), GamePalette.ChoiceB);

            // Tinder-style drag stamps — hidden at rest, fade in toward whichever side
            // the card is being dragged, live proof the gesture is registering and which
            // option a release would commit to.
            view._leftStamp = BuildDragStamp(faceGo.transform, "A", new Vector2(-210, 260), GamePalette.ChoiceA, tiltDegrees: 14f);
            view._rightStamp = BuildDragStamp(faceGo.transform, "B", new Vector2(210, 260), GamePalette.ChoiceB, tiltDegrees: -14f);

            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            return view;
        }

        private static CanvasGroup BuildDragStamp(Transform parent, string letter, Vector2 anchoredPos, Color accent, float tiltDegrees)
        {
            var stampGo = new GameObject($"Stamp{letter}", typeof(RectTransform));
            stampGo.transform.SetParent(parent, false);
            var stampRect = stampGo.GetComponent<RectTransform>();
            stampRect.anchorMin = stampRect.anchorMax = new Vector2(0.5f, 0.5f);
            stampRect.anchoredPosition = anchoredPos;
            stampRect.sizeDelta = new Vector2(120, 70);
            stampRect.localRotation = Quaternion.Euler(0, 0, tiltDegrees);

            var canvasGroup = stampGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            var border = stampGo.AddComponent<Image>();
            border.sprite = UIFactory.GetRoundedSprite(12);
            border.type = Image.Type.Sliced;
            border.color = accent;

            var inset = new GameObject("Inset", typeof(RectTransform));
            inset.transform.SetParent(stampGo.transform, false);
            var insetRect = inset.GetComponent<RectTransform>();
            insetRect.anchorMin = insetRect.anchorMax = new Vector2(0.5f, 0.5f);
            insetRect.anchoredPosition = Vector2.zero;
            insetRect.sizeDelta = new Vector2(112, 62);
            var insetImg = inset.AddComponent<Image>();
            insetImg.sprite = UIFactory.GetRoundedSprite(10);
            insetImg.type = Image.Type.Sliced;
            insetImg.color = GamePalette.CardSurface;

            AddLabel(inset.transform, "StampLabel", letter, 32, Vector2.zero, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(100, 56), accent);

            return canvasGroup;
        }

        // Badge sits centered on top of the answer chip, like a Kahoot answer marker —
        // stays within the chip's own width so it can never spill past the card/canvas edge.
        private static void BuildChoiceChip(Transform parent, string letter, string text, Vector2 anchoredPos, Color badgeColor)
        {
            var chipSize = new Vector2(240, 190);
            var chip = UIFactory.CreateSurface(parent, GamePalette.ChipSurface, anchoredPos, chipSize, 20, $"Chip{letter}");

            // Text box is shifted down and shrunk from the full chip size so it never
            // grows up into the badge — long answers auto-shrink to fit instead of overflowing.
            AddLabel(chip.transform, "ChipText", text, 20, new Vector2(0, -18), FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(210, 130), GamePalette.TextLight, autoShrink: true, minFontSize: 13);

            const float badgeSize = 52f;
            var badgeY = anchoredPos.y + chipSize.y / 2f;
            var badge = UIFactory.CreateSurface(parent, badgeColor, new Vector2(anchoredPos.x, badgeY), new Vector2(badgeSize, badgeSize), (int)(badgeSize / 2f), $"Badge{letter}");
            AddLabel(badge.transform, "BadgeLabel", letter, 22, Vector2.zero, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(badgeSize - 4, badgeSize - 4), GamePalette.TextDark);
        }

        private static void AddLabel(Transform parent, string name, string text, int fontSize, Vector2 anchoredPos, FontStyle style, TextAnchor anchor, Vector2 size, Color color,
            bool autoShrink = false, int minFontSize = 14)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var label = go.AddComponent<Text>();
            label.text = text;
            label.font = UIFactory.GetPlayfulFont();
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = anchor;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;

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
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_committed) return;
            _dragStartPos = eventData.position;
            _cardStartAnchoredPos = _rect.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_committed) return;
            var delta = eventData.position - _dragStartPos;
            _rect.anchoredPosition = _cardStartAnchoredPos + delta;
            var rotationT = Mathf.Clamp(delta.x / SwipeThreshold, -1.5f, 1.5f);
            _rect.localRotation = Quaternion.Euler(0, 0, -rotationT * MaxRotationDegrees);

            var stampT = Mathf.Clamp01(Mathf.Abs(delta.x) / SwipeThreshold);
            _leftStamp.alpha = delta.x < 0 ? stampT : 0f;
            _rightStamp.alpha = delta.x > 0 ? stampT : 0f;
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
                _rect.anchoredPosition = _cardStartAnchoredPos;
                _rect.localRotation = Quaternion.identity;
                _leftStamp.alpha = 0f;
                _rightStamp.alpha = 0f;
            }
        }

        private void Commit(SwipeSide side)
        {
            _committed = true;
            OnSwiped?.Invoke(side);
        }
    }
}
