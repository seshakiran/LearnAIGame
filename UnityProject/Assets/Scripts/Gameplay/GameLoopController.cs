using System.Collections;
using System.Collections.Generic;
using LearnAIGame.Audio;
using LearnAIGame.Bootstrap;
using LearnAIGame.Cards;
using LearnAIGame.Video;
using UnityEngine;

namespace LearnAIGame.Gameplay
{
    /// Spike A entry point (§12.1 of PLAN.md). Attach to a single empty GameObject
    /// in an otherwise empty scene — everything else is built at runtime.
    /// Drives: swipe burst -> video -> explanation -> leaderboard -> checkpoint -> payoff -> loop.
    public class GameLoopController : MonoBehaviour
    {
        // Micro-loop topic order per PLAN.md §3.2 — cycles 1-4, then wraps for the
        // next boss-level cluster once a boss level exists (§11 Phase 3).
        private static readonly string[] TopicResourceNames =
        {
            "hallucination_cards",
            "bias_training_data_cards",
            "rag_basics_cards",
            "prompt_injection_cards",
        };

        private Canvas _canvas;
        private CardBurstData _burst;
        private BackgroundMusicPlayer _music;
        private int _score;
        private int _totalCards;
        private int _topicIndex;

        private void Start()
        {
            _canvas = UIFactory.CreateRootCanvas();
            UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.BackgroundDeep, "Backdrop");
            _music = BackgroundMusicPlayer.CreateAndPlay(transform);

            if (!LoadTopic(_topicIndex)) return;

            StartCoroutine(RunSession());
        }

        private bool LoadTopic(int index)
        {
            var resourceName = TopicResourceNames[index];
            var json = Resources.Load<TextAsset>(resourceName);
            _burst = CardBurstData.LoadFromStreamingJson(json);

            if (_burst == null || _burst.cards == null || _burst.cards.Count == 0)
            {
                Debug.LogError($"GameLoopController: card burst data failed to load. Check Assets/Resources/{resourceName}.json");
                return false;
            }

            return true;
        }

        private IEnumerator RunSession()
        {
            _score = 0;
            _totalCards = _burst.cards.Count;

            foreach (var card in _burst.cards)
            {
                yield return PlayCard(card, isCheckpoint: false);
            }

            _music.Pause();
            yield return TopicVideoPlayer.Play(_canvas.transform);
            _music.Resume();

            yield return ShowExplanationScreen();
            yield return ShowLeaderboardScreen();
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
            var flashColor = correct ? GamePalette.CorrectGreen : GamePalette.IncorrectRed;
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, flashColor, "RevealPanel");

            var icon = UIFactory.CreateSurface(panel, Color.white, new Vector2(0, 310), new Vector2(110, 110), 55, "ResultIcon");
            UIFactory.CreateLabel(icon.transform, correct ? "✓" : "✕", 60, Vector2.zero, new Vector2(100, 100), TextAnchor.MiddleCenter, FontStyle.Bold, flashColor);

            var headline = correct ? "Correct!" : "Not quite";
            UIFactory.CreateLabel(panel, headline, 44, new Vector2(0, 200), new Vector2(700, 80), TextAnchor.MiddleCenter, FontStyle.Bold, Color.white);

            var explanationCard = UIFactory.CreateSurface(panel, GamePalette.CardSurface, new Vector2(0, 20), new Vector2(820, 260), 28, "ExplanationCard");
            UIFactory.CreateLabel(explanationCard.transform, card.explanation, 25, Vector2.zero, new Vector2(740, 220), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextDark);

            var tapped = false;
            UIFactory.CreateButton(panel, isCheckpoint ? "Continue →" : "Next →", new Vector2(0, -240), new Vector2(300, 90), () => tapped = true, Color.white, GamePalette.Darken(flashColor, 0.85f));

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
        }

        private IEnumerator ShowExplanationScreen()
        {
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.KahootBlue, "ExplanationPanel");

            UIFactory.CreateLabel(panel, _burst.topicTitle, 36, new Vector2(0, 260), new Vector2(760, 60), TextAnchor.MiddleCenter, FontStyle.Bold, Color.white);

            var scriptCard = UIFactory.CreateSurface(panel, GamePalette.CardSurface, new Vector2(0, 20), new Vector2(800, 380), 28, "ScriptCard");
            UIFactory.CreateLabel(scriptCard.transform, _burst.feynmanScript, 26, Vector2.zero, new Vector2(720, 340), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextDark);

            var tapped = false;
            UIFactory.CreateButton(panel, "Continue →", new Vector2(0, -320), new Vector2(300, 90), () => tapped = true, Color.white, GamePalette.KahootBlue);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
        }

        private IEnumerator ShowLeaderboardScreen()
        {
            var streak = StreakTracker.RegisterSessionCompleted();
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.KahootPurple, "LeaderboardPanel");

            UIFactory.CreateLabel(panel, "Leaderboard", 42, new Vector2(0, 350), new Vector2(600, 70), TextAnchor.MiddleCenter, FontStyle.Bold, Color.white);
            UIFactory.CreateLabel(panel, "(stub — no backend yet, mock ranks)", 16, new Vector2(0, 305), new Vector2(700, 30), TextAnchor.MiddleCenter, FontStyle.Italic, new Color(1f, 1f, 1f, 0.6f));

            var board = UIFactory.CreateSurface(panel, GamePalette.CardSurface, new Vector2(0, 60), new Vector2(720, 440), 28, "LeaderboardCard");

            var entries = BuildMockLeaderboard(_score, _totalCards);
            const float rowHeight = 62f;
            var startY = 170f;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var y = startY - i * rowHeight;

                if (entry.isPlayer)
                {
                    UIFactory.CreateSurface(board.transform, GamePalette.KahootYellow, new Vector2(0, y), new Vector2(660, 52), 16, "YouRow");
                }

                var rowTextColor = entry.isPlayer ? GamePalette.TextDark : GamePalette.TextMuted;
                var rowStyle = entry.isPlayer ? FontStyle.Bold : FontStyle.Normal;

                UIFactory.CreateLabel(board.transform, $"{i + 1}. {entry.name}", 22, new Vector2(-190, y), new Vector2(320, 46), TextAnchor.MiddleLeft, rowStyle, rowTextColor);
                UIFactory.CreateLabel(board.transform, $"{entry.score}/{_totalCards}", 22, new Vector2(230, y), new Vector2(140, 46), TextAnchor.MiddleRight, rowStyle, rowTextColor);
            }

            UIFactory.CreateLabel(panel, $"Streak: {streak} day{(streak == 1 ? "" : "s")}", 22, new Vector2(0, -280), new Vector2(500, 40), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.KahootYellow);

            var tapped = false;
            UIFactory.CreateButton(panel, "Continue →", new Vector2(0, -360), new Vector2(300, 90), () => tapped = true, Color.white, GamePalette.KahootPurple);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
        }

        private readonly struct LeaderboardEntry
        {
            public readonly string name;
            public readonly int score;
            public readonly bool isPlayer;

            public LeaderboardEntry(string name, int score, bool isPlayer)
            {
                this.name = name;
                this.score = score;
                this.isPlayer = isPlayer;
            }
        }

        private static List<LeaderboardEntry> BuildMockLeaderboard(int playerScore, int totalCards)
        {
            var mockNames = new[] { "Ava", "Marcus", "Priya", "Jordan" };
            var entries = new List<LeaderboardEntry>();

            for (var i = 0; i < mockNames.Length; i++)
            {
                var fraction = 1f - i * 0.18f;
                var score = Mathf.Clamp(Mathf.RoundToInt(totalCards * fraction), 0, totalCards);
                entries.Add(new LeaderboardEntry(mockNames[i], score, isPlayer: false));
            }

            entries.Add(new LeaderboardEntry("You", playerScore, isPlayer: true));
            entries.Sort((a, b) => b.score.CompareTo(a.score));
            return entries;
        }

        private IEnumerator ShowPayoffStub()
        {
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.CorrectGreen, "PayoffPanel");

            UIFactory.CreateLabel(panel, "Topic complete!", 44, new Vector2(0, 200), new Vector2(700, 70), TextAnchor.MiddleCenter, FontStyle.Bold, Color.white);
            UIFactory.CreateLabel(panel, "Skill-tree tile lit up (stub)", 26, new Vector2(0, 100), new Vector2(700, 50), TextAnchor.MiddleCenter, FontStyle.Normal, Color.white);

            var tapped = false;
            UIFactory.CreateButton(panel, "Next Topic →", new Vector2(0, -260), new Vector2(300, 90), () => tapped = true, Color.white, GamePalette.Darken(GamePalette.CorrectGreen, 0.75f));

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);

            _topicIndex = (_topicIndex + 1) % TopicResourceNames.Length;
            if (!LoadTopic(_topicIndex)) yield break;

            StartCoroutine(RunSession());
        }
    }
}
