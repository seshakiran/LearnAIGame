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

        // Display names for the path map (§7/§3 "skill-tree / map") — kept in the
        // same order as TopicResourceNames rather than parsed from all 4 JSON files
        // up front, since the map only needs to render, not load, the other topics.
        private static readonly string[] TopicTitles =
        {
            "Why AI Lies With Confidence",
            "When the Data Has Favorites",
            "When the Search Result Is Wrong",
            "When the Document Talks Back",
        };

        private Canvas _canvas;
        private CardBurstData _burst;
        private BackgroundMusicPlayer _music;
        private int _score;
        private int _totalCards;
        private int _topicIndex;
        private int _topicsCompletedThisSession;

        private void Start()
        {
            EnsureMainCamera();

            _canvas = UIFactory.CreateRootCanvas();
            UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.BackgroundDeep, "Backdrop");
            _music = BackgroundMusicPlayer.CreateAndPlay(transform);

            if (!LoadTopic(_topicIndex)) return;

            StartCoroutine(Bootstrap());
        }

        private IEnumerator Bootstrap()
        {
            yield return TopicPathScreen.Show(_canvas.transform, TopicTitles, _topicsCompletedThisSession, _topicIndex);
            yield return RunSession();
        }

        /// The scene is built entirely at runtime with no authored Camera. Without one,
        /// the Editor Game View has nothing to render and shows a "No cameras rendering"
        /// watermark that can bleed through translucent UI, and VideoPlayer's Direct audio
        /// output is unreliable with zero AudioListeners present in the scene.
        private static void EnsureMainCamera()
        {
            if (Camera.main != null) return;

            var cameraGo = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = GamePalette.BackgroundDeep;
            camera.cullingMask = 0;
            camera.orthographic = true;
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

            var header = BurstHeader.Create(_canvas.transform, _burst.topicTitle);
            for (var i = 0; i < _burst.cards.Count; i++)
            {
                header.SetProgress(i + 1, _burst.cards.Count);
                yield return PlayCard(_burst.cards[i], isCheckpoint: false);
            }
            header.Destroy();
            yield return null;

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
            yield return null;

            yield return ShowRevealPanel(card, correct, isCheckpoint);
        }

        private IEnumerator ShowRevealPanel(JudgmentCard card, bool correct, bool isCheckpoint)
        {
            var accent = correct ? GamePalette.CorrectAccent : GamePalette.IncorrectAccent;
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.ScreenSurface, "RevealPanel");

            var icon = UIFactory.CreateSurface(panel, accent, new Vector2(0, 310), new Vector2(110, 110), 55, "ResultIcon");
            UIFactory.CreateLabel(icon.transform, correct ? "✓" : "✕", 60, Vector2.zero, new Vector2(100, 100), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextDark);

            var headline = correct ? "Correct!" : "Not quite";
            UIFactory.CreateLabel(panel, headline, 44, new Vector2(0, 200), new Vector2(700, 80), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight);

            var explanationCard = UIFactory.CreateSurface(panel, GamePalette.CardSurface, new Vector2(0, 20), new Vector2(820, 260), 28, "ExplanationCard");
            UIFactory.CreateLabel(explanationCard.transform, card.explanation, 25, Vector2.zero, new Vector2(740, 220), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextLight, autoShrink: true, minFontSize: 18);

            var tapped = false;
            UIFactory.CreateButton(panel, isCheckpoint ? "Continue →" : "Next →", new Vector2(0, -240), new Vector2(300, 90), () => tapped = true, accent, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
            yield return null;
        }

        private IEnumerator ShowExplanationScreen()
        {
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.ScreenSurface, "ExplanationPanel");

            UIFactory.CreateLabel(panel, _burst.topicTitle, 36, new Vector2(0, 260), new Vector2(760, 60), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight);

            var scriptCard = UIFactory.CreateSurface(panel, GamePalette.CardSurface, new Vector2(0, 20), new Vector2(800, 380), 28, "ScriptCard");
            UIFactory.CreateLabel(scriptCard.transform, _burst.feynmanScript, 26, Vector2.zero, new Vector2(720, 340), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextLight, autoShrink: true, minFontSize: 18);

            var tapped = false;
            UIFactory.CreateButton(panel, "Continue →", new Vector2(0, -320), new Vector2(300, 90), () => tapped = true, GamePalette.Lime, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
            yield return null;
        }

        private IEnumerator ShowLeaderboardScreen()
        {
            var streak = StreakTracker.RegisterSessionCompleted();
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.ScreenSurface, "LeaderboardPanel");

            UIFactory.CreateLabel(panel, "Leaderboard", 42, new Vector2(0, 350), new Vector2(600, 70), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight);
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
                    UIFactory.CreateSurface(board.transform, GamePalette.Lime, new Vector2(0, y), new Vector2(660, 52), 16, "YouRow");
                }

                var rowTextColor = entry.isPlayer ? GamePalette.TextDark : GamePalette.TextMuted;
                var rowStyle = entry.isPlayer ? FontStyle.Bold : FontStyle.Normal;

                UIFactory.CreateLabel(board.transform, $"{i + 1}. {entry.name}", 22, new Vector2(-190, y), new Vector2(320, 46), TextAnchor.MiddleLeft, rowStyle, rowTextColor);
                UIFactory.CreateLabel(board.transform, $"{entry.score}/{_totalCards}", 22, new Vector2(230, y), new Vector2(140, 46), TextAnchor.MiddleRight, rowStyle, rowTextColor);
            }

            UIFactory.CreateLabel(panel, $"Streak: {streak} day{(streak == 1 ? "" : "s")}", 22, new Vector2(0, -280), new Vector2(500, 40), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.Lime);

            var tapped = false;
            UIFactory.CreateButton(panel, "Continue →", new Vector2(0, -360), new Vector2(300, 90), () => tapped = true, GamePalette.Lime, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
            yield return null;
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
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.ScreenSurface, "PayoffPanel");

            UIFactory.CreateLabel(panel, "Topic complete!", 44, new Vector2(0, 200), new Vector2(700, 70), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.Lime);
            UIFactory.CreateLabel(panel, "Skill-tree tile lit up (stub)", 26, new Vector2(0, 100), new Vector2(700, 50), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextMuted);

            var tapped = false;
            UIFactory.CreateButton(panel, "Next Topic →", new Vector2(0, -260), new Vector2(300, 90), () => tapped = true, GamePalette.Lime, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
            yield return null;

            _topicsCompletedThisSession++;
            _topicIndex = (_topicIndex + 1) % TopicResourceNames.Length;

            yield return TopicPathScreen.Show(_canvas.transform, TopicTitles, _topicsCompletedThisSession, _topicIndex);

            if (!LoadTopic(_topicIndex)) yield break;

            StartCoroutine(RunSession());
        }
    }
}
