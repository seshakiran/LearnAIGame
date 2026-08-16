using System.Collections;
using LearnAIGame.Bootstrap;
using UnityEngine;

namespace LearnAIGame.Gameplay
{
    /// A simple vertical curriculum map — topics as nodes (done/current/upcoming) —
    /// shown before the first topic and after each payoff. Playtesting showed users
    /// had no sense of where they were in the curriculum or how much was left; PLAN.md
    /// §3/§7 already calls for a "skill-tree / map" but Spike A only ever stubbed it
    /// as a one-line "tile lit up" text. This is a lightweight first pass at that.
    public static class TopicPathScreen
    {
        public static IEnumerator Show(Transform canvasParent, string[] topicTitles, int topicsCompletedThisSession, int upcomingTopicIndex)
        {
            var panel = UIFactory.CreateFullScreenPanel(canvasParent, GamePalette.ScreenSurface, "TopicPathPanel");

            UIFactory.CreateLabel(panel, "Your Path", 40, new Vector2(0, 440), new Vector2(700, 60), TextAnchor.MiddleCenter, FontStyle.Bold, GamePalette.TextLight);

            var lap = topicsCompletedThisSession / topicTitles.Length + 1;
            var subtitle = lap > 1 ? $"Lap {lap} — reinforcing what you've already covered" : "4 topics in this Foundations path";
            UIFactory.CreateLabel(panel, subtitle, 18, new Vector2(0, 395), new Vector2(780, 40), TextAnchor.MiddleCenter, FontStyle.Normal, GamePalette.TextMuted);

            var completedInLap = topicsCompletedThisSession % topicTitles.Length;
            // A full lap finished right before this screen shows — treat all 4 as done
            // rather than snapping back to 0 mid-display while upcoming wraps to index 0.
            if (completedInLap == 0 && topicsCompletedThisSession > 0 && upcomingTopicIndex == 0)
            {
                completedInLap = topicTitles.Length;
            }

            const float nodeSize = 96f;
            const float spacing = 240f;
            const float nodeX = -280f;
            const float labelX = -20f;
            const float startY = 300f;

            for (var i = 0; i < topicTitles.Length; i++)
            {
                var y = startY - i * spacing;

                if (i > 0)
                {
                    var connectorDone = i <= completedInLap;
                    var connector = UIFactory.CreateSurface(panel, connectorDone ? GamePalette.Lime : GamePalette.CardSurface,
                        new Vector2(nodeX, y + spacing / 2f), new Vector2(6, spacing - nodeSize), 3, $"Connector{i}");
                    connector.raycastTarget = false;
                }

                var isDone = i < completedInLap;
                var isCurrent = i == upcomingTopicIndex && !isDone;
                var nodeColor = isDone ? GamePalette.Lime : isCurrent ? GamePalette.Blue : GamePalette.CardSurface;
                var node = UIFactory.CreateSurface(panel, nodeColor, new Vector2(nodeX, y), new Vector2(nodeSize, nodeSize), (int)(nodeSize / 2f), $"Node{i}");

                var nodeLabelColor = isDone || isCurrent ? GamePalette.TextDark : GamePalette.TextMuted;
                UIFactory.CreateLabel(node.transform, isDone ? "✓" : (i + 1).ToString(), 32, Vector2.zero, new Vector2(nodeSize - 8, nodeSize - 8), TextAnchor.MiddleCenter, FontStyle.Bold, nodeLabelColor);

                var titleColor = isDone || isCurrent ? GamePalette.TextLight : GamePalette.TextMuted;
                var titleStyle = isCurrent ? FontStyle.Bold : FontStyle.Normal;
                UIFactory.CreateLabel(panel, topicTitles[i], 22, new Vector2(labelX, y), new Vector2(380, 80), TextAnchor.MiddleLeft, titleStyle, titleColor, autoShrink: true, minFontSize: 16);

                if (isCurrent)
                {
                    UIFactory.CreateLabel(panel, "You are here", 15, new Vector2(labelX, y - 34), new Vector2(300, 30), TextAnchor.MiddleLeft, FontStyle.Italic, GamePalette.Blue);
                }
            }

            var tapped = false;
            var buttonLabel = topicsCompletedThisSession == 0 ? "Start →" : "Continue →";
            UIFactory.CreateButton(panel, buttonLabel, new Vector2(0, -620), new Vector2(300, 90), () => tapped = true, GamePalette.Lime, GamePalette.TextDark);

            yield return new WaitUntil(() => tapped);
            Object.Destroy(panel.gameObject);
            yield return null;
        }
    }
}
