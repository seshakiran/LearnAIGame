using UnityEngine;

namespace LearnAIGame.Bootstrap
{
    /// Bold, flat, high-saturation palette — Kahoot's game-show energy for
    /// backgrounds/buttons, Tinder's card-swipe language for choice color-coding.
    public static class GamePalette
    {
        public static readonly Color KahootPurple = FromHex("#46178F");
        public static readonly Color KahootBlue = FromHex("#1368CE");
        public static readonly Color KahootYellow = FromHex("#FFA602");

        public static readonly Color BackgroundDeep = FromHex("#1B1035");

        public static readonly Color CardSurface = FromHex("#FFFFFF");
        public static readonly Color TextDark = FromHex("#1A1A2E");
        public static readonly Color TextMuted = FromHex("#6B6B80");

        // Left/right swipe choice colors — Tinder-style, deliberately not
        // green/red so dragging never leaks the correct answer.
        public static readonly Color ChoiceA = FromHex("#FF6B6B");
        public static readonly Color ChoiceB = FromHex("#4ECDC4");

        public static readonly Color CorrectGreen = FromHex("#2ECC71");
        public static readonly Color IncorrectRed = FromHex("#FF4757");

        public static readonly Color ShadowDark = new Color(0f, 0f, 0f, 0.35f);

        // Each card gets one of these as its own bold background — picked by a
        // stable hash of the card id so it's colorful but not literally random every replay.
        public static readonly Color[] CardThemes =
        {
            FromHex("#46178F"), // purple
            FromHex("#E21B3C"), // red
            FromHex("#1368CE"), // blue
            FromHex("#D6249F"), // magenta
            FromHex("#1DB954"), // green
            FromHex("#FF8C42"), // orange
            FromHex("#0E7C7B"), // teal
            FromHex("#C2185B"), // pink
        };

        public static Color CardThemeFor(string cardId)
        {
            var hash = string.IsNullOrEmpty(cardId) ? 0 : cardId.GetHashCode();
            var index = Mathf.Abs(hash) % CardThemes.Length;
            return CardThemes[index];
        }

        public static Color FromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static Color Darken(Color c, float factor)
        {
            return new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
        }
    }
}
