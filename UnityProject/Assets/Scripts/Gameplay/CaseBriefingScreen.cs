using System.Collections;
using System.Collections.Generic;
using LearnAIGame.Bootstrap;
using LearnAIGame.Cards;
using UnityEngine;

namespace LearnAIGame.Gameplay
{
    /// The first two beats of Deep Trace's five-beat spine (Mission Briefing ->
    /// Cold Open -> Play -> Reveal -> Lock-In), PLAN.md §13.2. Deep Trace's own
    /// decision log explicitly rejected a single static text-heavy briefing screen
    /// in favor of two terse, sequential beats — a mission assignment, then a
    /// dramatic alert — so this is two short screens, not one paragraph card.
    public static class CaseBriefingScreen
    {
        public static IEnumerator Show(Transform canvasParent, CardBurstData burst)
        {
            yield return ShowMissionBriefing(canvasParent, burst);
            yield return ShowColdOpen(canvasParent, burst);
        }

        private static IEnumerator ShowMissionBriefing(Transform canvasParent, CardBurstData burst)
        {
            var panel = UIFactory.CreateFullScreenPanel(canvasParent, GamePalette.ScreenSurface, "MissionBriefingPanel");
            var halfHeight = Mathf.Max(panel.rect.height / 2f, 480f);

            var caseLabelY = Mathf.Min(120f, halfHeight - 100f);
            var skillY = Mathf.Min(0f, halfHeight - 260f);
            var missionY = Mathf.Min(-120f, halfHeight - 380f);
            var buttonY = Mathf.Max(-620f, -halfHeight + 90f);

            var caseLabel = string.IsNullOrEmpty(burst.caseCode) ? burst.topicTitle : $"{burst.caseCode} · {burst.caseTitle}";
            UIFactory.CreateLabel(panel, caseLabel.ToUpperInvariant(), 18, new Vector2(0, caseLabelY), new Vector2(780, 36),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.Lime);

            var skillText = string.IsNullOrEmpty(burst.skillLine) ? burst.topicTitle : burst.skillLine;
            UIFactory.CreateLabel(panel, skillText, 38, new Vector2(0, skillY), new Vector2(800, 140),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight, autoShrink: true, minFontSize: 24);

            if (!string.IsNullOrEmpty(burst.missionLine))
            {
                UIFactory.CreateLabel(panel, burst.missionLine, 22, new Vector2(0, missionY), new Vector2(760, 80),
                    TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextMuted, autoShrink: true, minFontSize: 16);
            }

            var tapped = false;
            UIFactory.CreateButton(panel, "Start Case →", new Vector2(0, buttonY), new Vector2(320, 90), () => tapped = true, GamePalette.Lime, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Object.Destroy(panel.gameObject);
            yield return null;
        }

        private static IEnumerator ShowColdOpen(Transform canvasParent, CardBurstData burst)
        {
            var panel = UIFactory.CreateFullScreenPanel(canvasParent, GamePalette.BackgroundDeep, "ColdOpenPanel");
            var halfHeight = Mathf.Max(panel.rect.height / 2f, 480f);

            var headlineY = Mathf.Min(160f, halfHeight - 180f);
            var stakesY = Mathf.Min(-20f, halfHeight - 420f);
            var meterY = Mathf.Min(-180f, halfHeight - 540f);
            var buttonY = Mathf.Max(-620f, -halfHeight + 90f);

            // Dramatic, alarming — no teaching copy on this screen per the Cold Open
            // beat's rule (that's what Reveal is for, after the player has played).
            var headline = string.IsNullOrEmpty(burst.incidentHeadline) ? burst.topicTitle : burst.incidentHeadline;
            UIFactory.CreateLabel(panel, headline, 34, new Vector2(0, headlineY), new Vector2(800, 200),
                TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.Rose, autoShrink: true, minFontSize: 22);

            if (burst.stakesLines != null && burst.stakesLines.Count > 0)
            {
                BuildStakesRow(panel, burst.stakesLines, stakesY);
            }

            if (!string.IsNullOrEmpty(burst.meterLabel))
            {
                UIFactory.CreateLabel(panel, $"{burst.meterLabel.ToUpperInvariant()} — TRACKING LIVE", 16, new Vector2(0, meterY), new Vector2(700, 34),
                    TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextMuted);
            }

            var tapped = false;
            UIFactory.CreateButton(panel, "Begin →", new Vector2(0, buttonY), new Vector2(320, 90), () => tapped = true, GamePalette.Rose, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Object.Destroy(panel.gameObject);
            yield return null;
        }

        private static void BuildStakesRow(Transform parent, List<string> stakesLines, float y)
        {
            const float chipWidth = 260f;
            const float spacing = 20f;
            var count = Mathf.Min(stakesLines.Count, 3);
            var totalWidth = count * chipWidth + (count - 1) * spacing;
            var startX = -totalWidth / 2f + chipWidth / 2f;

            for (var i = 0; i < count; i++)
            {
                var x = startX + i * (chipWidth + spacing);
                var chip = UIFactory.CreateSurface(parent, GamePalette.ChipSurface, new Vector2(x, y), new Vector2(chipWidth, 110), 20, $"StakeChip{i}");
                UIFactory.CreateLabel(chip.transform, stakesLines[i], 17, Vector2.zero, new Vector2(chipWidth - 32, 94),
                    TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextMuted, autoShrink: true, minFontSize: 13);
            }
        }
    }
}
