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

        // Asymmetric on purpose (mirrors Deep Trace's "Queue Pressure" tuning) — a
        // wrong swipe should visibly cost more than a right swipe earns back.
        private const int MeterDeltaCorrect = -8;
        private const int MeterDeltaIncorrect = 15;

        private Canvas _canvas;
        private CardBurstData _burst;
        private BackgroundMusicPlayer _music;
        private BurstHeader _header;
        private int _score;
        private int _totalCards;
        private int _topicIndex;
        private int _topicsCompletedThisSession;
        private bool _escalationFired;

        private void Start()
        {
            EnsureMainCamera();

            // Story campaigns are the learner-facing entry point. Keep the original
            // prototype available as source while migrating its reusable components.
            gameObject.AddComponent<LearnAIGame.Story.StoryGameController>();
        }

        private void StartLegacyPrototype()
        {

            _canvas = UIFactory.CreateRootCanvas();
            UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.BackgroundDeep, "Backdrop");
            _music = BackgroundMusicPlayer.CreateAndPlay(transform);

            if (!LoadTopic(_topicIndex)) return;

            StartCoroutine(Bootstrap());
        }

        private IEnumerator Bootstrap()
        {
            yield return GameHomeScreen.Show(_canvas.transform, TopicTitles[_topicIndex], _burst);
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
            _escalationFired = false;

            yield return CaseBriefingScreen.Show(_canvas.transform, _burst);

            _header = BurstHeader.Create(_canvas.transform, _burst.topicTitle, _burst.meterLabel, _burst.meterStart);
            for (var i = 0; i < _burst.cards.Count; i++)
            {
                _header.SetProgress(i + 1, _burst.cards.Count);
                yield return PlayCard(_burst.cards[i], isCheckpoint: false);
            }
            _header.Destroy();
            _header = null;
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
            var cardView = SwipeCardView.Create(_canvas.transform, card, _burst.topicTitle);
            SwipeSide? chosenSide = null;
            cardView.OnSwiped += side => chosenSide = side;

            yield return new WaitUntil(() => chosenSide.HasValue);

            var correct = card.IsCorrectSwipe(chosenSide.Value);
            if (!isCheckpoint && correct) _score++;
            if (!isCheckpoint) _header?.ApplyMeterDelta(correct ? MeterDeltaCorrect : MeterDeltaIncorrect);

            Destroy(cardView.gameObject);
            yield return null;

            yield return ShowRevealPanel(card, correct, isCheckpoint);

            // The case interrupts once, only if the player's own choices drove the
            // meter into its danger zone — a scripted beat that fires regardless of
            // performance wouldn't feel like the case reacting to you.
            if (!isCheckpoint && !_escalationFired && _header != null && _header.MeterValue >= 75
                && !string.IsNullOrEmpty(_burst.escalationLine))
            {
                _escalationFired = true;
                yield return ShowCaseUpdatePanel(_burst.escalationLine);
            }
        }

        private IEnumerator ShowCaseUpdatePanel(string line)
        {
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.ScreenSurface, "CaseUpdatePanel");

            UIFactory.CreateLabel(panel, "CASE UPDATE", 22, new Vector2(0, 160), new Vector2(600, 40), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.Rose);
            UIFactory.CreateLabel(panel, line, 28, new Vector2(0, 40), new Vector2(760, 220), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextLight, autoShrink: true, minFontSize: 18);

            if (_header != null && !string.IsNullOrEmpty(_burst.meterLabel))
            {
                var meterColor = BurstHeader.ColorForMeterValue(_header.MeterValue);
                UIFactory.CreateLabel(panel, $"{_burst.meterLabel}: {_header.MeterValue}", 22, new Vector2(0, -100), new Vector2(500, 40), TextAnchor.MiddleCenter, FontStyle.Bold, meterColor);
            }

            var tapped = false;
            UIFactory.CreateButton(panel, "Keep going →", new Vector2(0, -220), new Vector2(300, 90), () => tapped = true, GamePalette.Rose, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
            yield return null;
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

            // The case talking back — a per-swipe consequence line, not just a
            // color-coded number, so the meter reads as a system reacting to this
            // specific decision rather than a static score sitting in the header.
            if (!isCheckpoint && _header != null && !string.IsNullOrEmpty(_burst.meterLabel))
            {
                var meterColor = BurstHeader.ColorForMeterValue(_header.MeterValue);
                var meterLine = correct
                    ? $"{_burst.meterLabel} holds at {_header.MeterValue}."
                    : $"{_burst.meterLabel} slips to {_header.MeterValue}.";
                UIFactory.CreateLabel(panel, meterLine, 20, new Vector2(0, -170), new Vector2(700, 40), TextAnchor.MiddleCenter, FontStyle.Bold, meterColor);
            }

            var tapped = false;
            UIFactory.CreateButton(panel, isCheckpoint ? "Continue →" : "Next →", new Vector2(0, -240), new Vector2(300, 90), () => tapped = true, accent, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
            yield return null;
        }

        private IEnumerator ShowExplanationScreen()
        {
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.ScreenSurface, "ExplanationPanel");

            UIFactory.CreateLabel(panel, $"You learned: {_burst.topicTitle}", 26, new Vector2(0, 300), new Vector2(780, 50), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextMuted, autoShrink: true, minFontSize: 18);

            if (!string.IsNullOrEmpty(_burst.memoryLine))
            {
                UIFactory.CreateLabel(panel, $"“{_burst.memoryLine}”", 32, new Vector2(0, 230), new Vector2(780, 100), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.Lime, autoShrink: true, minFontSize: 22);
            }

            var scriptCard = UIFactory.CreateSurface(panel, GamePalette.CardSurface, new Vector2(0, -20), new Vector2(800, 300), 28, "ScriptCard");
            UIFactory.CreateLabel(scriptCard.transform, _burst.feynmanScript, 25, Vector2.zero, new Vector2(720, 260), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextLight, autoShrink: true, minFontSize: 17);

            if (!string.IsNullOrEmpty(_burst.reflectionPrompt))
            {
                UIFactory.CreateLabel(panel, _burst.reflectionPrompt, 20, new Vector2(0, -220), new Vector2(760, 60), TextAnchor.MiddleCenter, FontStyle.Italic, GamePalette.TextMuted, autoShrink: true, minFontSize: 15);
            }

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

        /// Deep Trace's "Case Closed" beat (PLAN.md §13.2) — a stamp, not a stub. XP
        /// and rank persist across cases so the game reads as one investigator's
        /// career, not a topic list resetting to zero every time.
        private IEnumerator ShowPayoffStub()
        {
            var xpEarned = RankTracker.RegisterCaseClosed(_score, _totalCards);
            var panel = UIFactory.CreateFullScreenPanel(_canvas.transform, GamePalette.ScreenSurface, "PayoffPanel");

            var caseLabel = string.IsNullOrEmpty(_burst.caseCode) ? "CASE CLOSED" : $"{_burst.caseCode.ToUpperInvariant()}: CASE CLOSED";
            UIFactory.CreateLabel(panel, caseLabel, 22, new Vector2(0, 260), new Vector2(700, 40), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.Amber);
            UIFactory.CreateLabel(panel, "CASE CLOSED", 52, new Vector2(0, 180), new Vector2(700, 80), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.Lime);

            UIFactory.CreateLabel(panel, $"+{xpEarned} XP", 26, new Vector2(0, 90), new Vector2(500, 44), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight);
            UIFactory.CreateLabel(panel, $"{RankTracker.CurrentRank} · {RankTracker.CurrentXp} XP total", 18, new Vector2(0, 40), new Vector2(600, 36), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextMuted);

            var nextIndex = (_topicIndex + 1) % TopicResourceNames.Length;
            UIFactory.CreateLabel(panel, $"Next Case: {TopicTitles[nextIndex]}", 20, new Vector2(0, -60), new Vector2(700, 44), TextAnchor.MiddleCenter, FontStyle.Italic, GamePalette.TextMuted, autoShrink: true, minFontSize: 15);

            var tapped = false;
            UIFactory.CreateButton(panel, "Next Case →", new Vector2(0, -260), new Vector2(300, 90), () => tapped = true, GamePalette.Lime, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Destroy(panel.gameObject);
            yield return null;

            _topicsCompletedThisSession++;
            _topicIndex = nextIndex;

            yield return TopicPathScreen.Show(_canvas.transform, TopicTitles, _topicsCompletedThisSession, _topicIndex);

            if (!LoadTopic(_topicIndex)) yield break;

            StartCoroutine(RunSession());
        }
    }
}
