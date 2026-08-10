using System;

namespace LearnAIGame.Cards
{
    [Serializable]
    public class JudgmentCard
    {
        public string id;
        public string prompt;
        public string optionA;
        public string optionB;
        public string correctOption; // "A" or "B"
        public string explanation;

        public bool IsCorrectSwipe(SwipeSide side)
        {
            var chosen = side == SwipeSide.Left ? "A" : "B";
            return string.Equals(chosen, correctOption, StringComparison.OrdinalIgnoreCase);
        }
    }

    public enum SwipeSide
    {
        Left,  // maps to Option A
        Right  // maps to Option B
    }
}
