using System;
using System.Collections.Generic;

namespace LearnAIGame.Story
{
    public static class StoryFlow
    {
        public static int NextIndex(StoryChapter chapter, int current, StoryDecision decision)
        {
            var beat = chapter.beats[current];
            string target = decision == null ? null : beat.choices[decision.choice].nextBeatId;
            if (string.IsNullOrEmpty(target)) target = beat.nextBeatId;
            if (!string.IsNullOrEmpty(target))
            {
                int next = Array.FindIndex(chapter.beats, b => b.id == target);
                if (next < 0) throw new InvalidOperationException("Missing story branch: " + target);
                return next;
            }
            return current + 1 < chapter.beats.Length ? current + 1 : -1;
        }

        public static bool CorrectOrder(StoryBeat beat, IList<string> order)
        {
            if (order == null || beat.correctOrder == null || order.Count != beat.correctOrder.Length) return false;
            for (int i = 0; i < order.Count; i++)
                if (order[i] != beat.correctOrder[i]) return false;
            return true;
        }
    }
}
