using System.Collections;
using LearnAIGame.Bootstrap;
using LearnAIGame.Cards;
using UnityEngine;

namespace LearnAIGame.Gameplay
{
    /// Spike A entry point (§12.1 of PLAN.md). Attach to a single empty GameObject
    /// in an otherwise empty scene — everything else is built at runtime.
    /// Drives: swipe burst -> result -> Feynman video stub -> checkpoint -> payoff stub -> loop.
    public class GameLoopController : MonoBehaviour
    {
        private Canvas _canvas;
        private CardBurstData _burst;
        private int _score;
        private int _totalCards;

        private void Start()
        {
            _canvas = UIFactory.CreateRootCanvas();

            var json = Resources.Load<TextAsset>("hallucination_cards");
            _burst = CardBurstData.LoadFromStreamingJson(json);

            if (_burst == null || _burst.cards == null || _burst.cards.Count == 0)
            {
                Debug.LogError("GameLoopController: card burst data failed to load. Check Assets/Resources/hallucination_cards.json");
                return;
            }

            StartCoroutine(RunSession());
        }

        private IEnumerator RunSession()
        {
            _score = 0;
            _totalCards = _burst.cards.Count;

            foreach (var card in _burst.cards)
            {
                yield return PlayCard(card, isCheckpoint: false);
            }

            yield return ShowResultScreen();
            yield return ShowVideoStub();
            yield return PlayCard(_burst.checkpointCard, isCheckpoint: true);
            yield return ShowPayoffStub();
        }

        private IEnumerator PlayCard(JudgmentCard card, bool isCheckpoint)
        {
            var cardView = SwipeCardView.Create(_canvas.transform, card);
            SwipeSide? chosenSide = null;
            cardView.OnSwiped += side => chosenSide = side;

            yield return new WaitUntil(() => chosenSide.HasValue);

            var correct = card.IsCorrectSwipe(chosenSide.Value);
            if (!isCheckpoint && correct) _score++;

            Destroy(cardView.gameObject);

            yield return ShowRevealPanel(card, correct, isCheckpoint);
        }

        private IEnumerator ShowRevealPanel(JudgmentCard card, bool correct, bool isCheckpoint)
        {
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, new Color(0f, 0f, 0f, 0.85f), "RevealPanel");

            var headline = correct ? "Correct" : "Not quite";
            var headlineColor = correct ? new Color(0.3f, 0.85f, 0.4f) : new Color(0.9f, 0.35f, 0.3f);

            UIFactory.CreateLabel(panel, headline, 44, new Vector2(0, 200), new Vector2(700, 80), TextAnchor.MiddleCenter, FontStyle.Bold, headlineColor);
            UIFactory.CreateLabel(panel, card.explanation, 26, new Vector2(0, 40), new Vector2(760, 260));

            var tapped = false;
            UIFactory.CreateButton(panel, isCheckpoint ? "Continue" : "Next", new Vector2(0, -260), new Vector2(280, 90), () => tapped = true);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
        }

        private IEnumerator ShowResultScreen()
        {
            var streak = StreakTracker.RegisterSessionCompleted();
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, new Color(0.08f, 0.08f, 0.1f, 1f), "ResultPanel");

            UIFactory.CreateLabel(panel, $"{_score}/{_totalCards} correct", 48, new Vector2(0, 220), new Vector2(700, 90), TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.CreateLabel(panel, $"Streak: {streak} day{(streak == 1 ? "" : "s")}", 30, new Vector2(0, 120), new Vector2(700, 60));
            UIFactory.CreateLabel(panel, "(shareable result card — stub for Spike A)", 18, new Vector2(0, 60), new Vector2(700, 40), TextAnchor.MiddleCenter, FontStyle.Italic);

            var tapped = false;
            UIFactory.CreateButton(panel, "Continue", new Vector2(0, -260), new Vector2(280, 90), () => tapped = true);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
        }

        private IEnumerator ShowVideoStub()
        {
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, new Color(0.05f, 0.05f, 0.08f, 1f), "VideoStubPanel");

            UIFactory.CreateLabel(panel, "[ Feynman video plays here — Spike B not wired yet ]", 22, new Vector2(0, 300), new Vector2(800, 50), TextAnchor.MiddleCenter, FontStyle.Italic, new Color(0.6f, 0.6f, 0.65f));
            UIFactory.CreateLabel(panel, _burst.topicTitle, 36, new Vector2(0, 220), new Vector2(760, 60), TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.CreateLabel(panel, _burst.feynmanScript, 26, new Vector2(0, 20), new Vector2(780, 320));

            var tapped = false;
            UIFactory.CreateButton(panel, "Continue", new Vector2(0, -320), new Vector2(280, 90), () => tapped = true);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
        }

        private IEnumerator ShowPayoffStub()
        {
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, new Color(0.08f, 0.12f, 0.08f, 1f), "PayoffPanel");

            UIFactory.CreateLabel(panel, "Topic complete", 40, new Vector2(0, 200), new Vector2(700, 70), TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.CreateLabel(panel, "Skill-tree tile lit up (stub)", 26, new Vector2(0, 100), new Vector2(700, 50));

            var tapped = false;
            UIFactory.CreateButton(panel, "Play Again", new Vector2(0, -260), new Vector2(280, 90), () => tapped = true);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);

            StartCoroutine(RunSession());
        }
    }
}
