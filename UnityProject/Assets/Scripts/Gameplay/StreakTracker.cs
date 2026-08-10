using System;
using UnityEngine;

namespace LearnAIGame.Gameplay
{
    /// Local, single-device streak tracking for the spike (no backend yet — see PLAN.md §10.3).
    public static class StreakTracker
    {
        private const string LastPlayedKey = "LearnAIGame_LastPlayedDay";
        private const string StreakKey = "LearnAIGame_CurrentStreak";

        /// Call once per completed session. Returns the streak count after this session.
        public static int RegisterSessionCompleted()
        {
            var today = DateTime.UtcNow.Date;
            var todayOrdinal = today.ToOADate();

            var hasLastPlayed = PlayerPrefs.HasKey(LastPlayedKey);
            var lastPlayedOrdinal = PlayerPrefs.GetFloat(LastPlayedKey, 0f);
            var streak = PlayerPrefs.GetInt(StreakKey, 0);

            if (!hasLastPlayed)
            {
                streak = 1;
            }
            else
            {
                var daysSinceLastPlay = (int)Math.Round(todayOrdinal - lastPlayedOrdinal);
                if (daysSinceLastPlay == 0)
                {
                    // already played today, streak unchanged
                }
                else if (daysSinceLastPlay == 1)
                {
                    streak += 1;
                }
                else
                {
                    streak = 1;
                }
            }

            PlayerPrefs.SetFloat(LastPlayedKey, (float)todayOrdinal);
            PlayerPrefs.SetInt(StreakKey, streak);
            PlayerPrefs.Save();

            return streak;
        }

        public static int CurrentStreak => PlayerPrefs.GetInt(StreakKey, 0);
    }
}
