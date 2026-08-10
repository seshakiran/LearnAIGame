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

        private const float SwipeThreshold = 160f;
        private const float MaxRotationDegrees = 12f;
        private static readonly Vector2 CardSize = new Vector2(620, 600);

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

            AddLabel(faceGo.transform, "Prompt", card.prompt, 30, new Vector2(0, 160), FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(560, 170), Color.white);

            BuildChoiceChip(faceGo.transform, "A", card.optionA, new Vector2(-150, -190), GamePalette.ChoiceA);
            BuildChoiceChip(faceGo.transform, "B", card.optionB, new Vector2(150, -190), GamePalette.ChoiceB);

            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            return view;
        }

        // Badge sits centered on top of the answer chip, like a Kahoot answer marker —
        // stays within the chip's own width so it can never spill past the card/canvas edge.
        private static void BuildChoiceChip(Transform parent, string letter, string text, Vector2 anchoredPos, Color badgeColor)
        {
            var chipSize = new Vector2(210, 110);
            var chip = UIFactory.CreateSurface(parent, GamePalette.CardSurface, anchoredPos, chipSize, 20, $"Chip{letter}");
            AddLabel(chip.transform, "ChipText", text, 18, new Vector2(0, -14), FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(180, 66), GamePalette.TextDark);

            const float badgeSize = 52f;
            var badgeY = anchoredPos.y + chipSize.y / 2f;
            var badge = UIFactory.CreateSurface(parent, badgeColor, new Vector2(anchoredPos.x, badgeY), new Vector2(badgeSize, badgeSize), (int)(badgeSize / 2f), $"Badge{letter}");
            AddLabel(badge.transform, "BadgeLabel", letter, 22, Vector2.zero, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(badgeSize - 4, badgeSize - 4), Color.white);
        }

        private static void AddLabel(Transform parent, string name, string text, int fontSize, Vector2 anchoredPos, FontStyle style, TextAnchor anchor, Vector2 size, Color color)
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
            label.verticalOverflow = VerticalWrapMode.Overflow;
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
            }
        }

        private void Commit(SwipeSide side)
        {
            _committed = true;
            OnSwiped?.Invoke(side);
        }
    }
}
