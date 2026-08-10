using System;
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

        public static SwipeCardView Create(Transform parent, JudgmentCard card)
        {
            var go = new GameObject($"Card_{card.id}", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 760);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.14f, 0.18f, 1f);

            var view = go.AddComponent<SwipeCardView>();
            view._rect = rect;

            AddLabel(go.transform, "Prompt", card.prompt, 34, new Vector2(0, 260), FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(520, 200));
            AddLabel(go.transform, "OptionA", $"←  {card.optionA}", 26, new Vector2(-260, -40), FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(300, 300));
            AddLabel(go.transform, "OptionB", $"{card.optionB}  →", 26, new Vector2(260, -40), FontStyle.Normal, TextAnchor.MiddleRight, new Vector2(300, 300));
            AddLabel(go.transform, "Hint", "swipe toward the one you trust", 18, new Vector2(0, -330), FontStyle.Italic, TextAnchor.MiddleCenter, new Vector2(500, 40));

            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            return view;
        }

        private static void AddLabel(Transform parent, string name, string text, int fontSize, Vector2 anchoredPos, FontStyle style, TextAnchor anchor, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var label = go.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = anchor;
            label.color = Color.white;
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
