using UnityEngine;

namespace LearnAIGame.Gameplay
{
    /// Local, single-device investigator rank/XP progression (no backend yet — see
    /// PLAN.md §10.3). Deep-Trace-fusion pass: cases should feel like they build one
    /// investigator's career, not reset to zero each time — see PLAN.md §13.2.
    public static class RankTracker
    {
        private const string XpKey = "LearnAIGame_InvestigatorXp";

        private static readonly (int xpThreshold, string title)[] Ranks =
        {
            (0, "Rookie Analyst"),
            (150, "Field Analyst"),
            (400, "Senior Analyst"),
            (800, "Lead Investigator"),
            (1400, "Chief Investigator"),
        };

        public static int CurrentXp => PlayerPrefs.GetInt(XpKey, 0);

        public static string CurrentRank => RankForXp(CurrentXp);

        /// Call once per completed case. Correct answers earn more; a case is never
        /// worth zero, so finishing always feels like progress.
        public static int RegisterCaseClosed(int score, int totalCards)
        {
            var xpEarned = 60 + Mathf.RoundToInt(90f * score / Mathf.Max(totalCards, 1));
            var newXp = CurrentXp + xpEarned;
            PlayerPrefs.SetInt(XpKey, newXp);
            PlayerPrefs.Save();
            return xpEarned;
        }

        public static string RankForXp(int xp)
        {
            var title = Ranks[0].title;
            foreach (var rank in Ranks)
            {
                if (xp >= rank.xpThreshold) title = rank.title;
            }
            return title;
        }
    }
}
