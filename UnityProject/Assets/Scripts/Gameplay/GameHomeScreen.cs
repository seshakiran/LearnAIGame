using System.Collections;
using LearnAIGame.Bootstrap;
using LearnAIGame.Cards;
using UnityEngine;

namespace LearnAIGame.Gameplay
{
    /// One-time app-open title beat, shown once per app launch before the first
    /// TopicPathScreen. LearnAIGame previously dropped a player straight into "Your
    /// Path" with no sense of arrival — Deep Trace's actual Home screen (wordmark,
    /// one-line mission, a single featured case, one CTA) is the fix: PLAN.md §13.2.
    /// Not shown again mid-session — this is a front door, not a per-topic beat.
    public static class GameHomeScreen
    {
        public static IEnumerator Show(Transform canvasParent, string firstCaseTitle, CardBurstData firstBurst)
        {
            var panel = UIFactory.CreateFullScreenPanel(canvasParent, GamePalette.BackgroundDeep, "HomePanel");
            var halfHeight = Mathf.Max(panel.rect.height / 2f, 480f);

            var wordmarkY = Mathf.Min(360f, halfHeight - 120f);
            var taglineY = Mathf.Min(295f, halfHeight - 185f);
            var rankY = Mathf.Min(240f, halfHeight - 240f);
            var cardY = Mathf.Min(-20f, halfHeight - 500f);
            var buttonY = Mathf.Max(-620f, -halfHeight + 90f);

            UIFactory.CreateLabel(panel, "LEARN AI GAME", 44, new Vector2(0, wordmarkY), new Vector2(760, 70),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight);
            UIFactory.CreateLabel(panel, "Investigate failing AI systems under pressure.", 20, new Vector2(0, taglineY), new Vector2(700, 50),
                TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextMuted, autoShrink: true, minFontSize: 15);

            var rankLine = $"{RankTracker.CurrentRank} · {RankTracker.CurrentXp} XP";
            UIFactory.CreateLabel(panel, rankLine, 16, new Vector2(0, rankY), new Vector2(500, 34),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.Lime);

            var card = UIFactory.CreateSurface(panel, GamePalette.CardSurface, new Vector2(0, cardY), new Vector2(760, 380), 28, "FeaturedCaseCard");

            var caseLabel = string.IsNullOrEmpty(firstBurst?.caseCode) ? "FEATURED CASE" : $"FEATURED CASE — {firstBurst.caseCode.ToUpperInvariant()}";
            UIFactory.CreateLabel(card.transform, caseLabel, 16, new Vector2(0, 150), new Vector2(680, 30),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.Amber);

            var caseTitle = string.IsNullOrEmpty(firstBurst?.caseTitle) ? firstCaseTitle : $"{firstBurst.caseCode}: {firstBurst.caseTitle}";
            UIFactory.CreateLabel(card.transform, caseTitle, 30, new Vector2(0, 95), new Vector2(680, 70),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight, autoShrink: true, minFontSize: 20);

            var briefLine = firstBurst != null && !string.IsNullOrEmpty(firstBurst.missionLine) ? firstBurst.missionLine : firstCaseTitle;
            UIFactory.CreateLabel(card.transform, briefLine, 18, new Vector2(0, 10), new Vector2(660, 130),
                TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextMuted, autoShrink: true, minFontSize: 14);

            UIFactory.CreateLabel(card.transform, "You've been assigned to the Investigations Desk.", 16, new Vector2(0, -130), new Vector2(660, 50),
                TextAnchor.MiddleCenter, FontStyle.Italic, GamePalette.TextMuted, autoShrink: true, minFontSize: 12);

            var tapped = false;
            UIFactory.CreateButton(panel, "Open Case →", new Vector2(0, buttonY), new Vector2(320, 90), () => tapped = true, GamePalette.Lime, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Object.Destroy(panel.gameObject);
            yield return null;
        }
    }
}
