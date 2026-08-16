using UnityEngine;

namespace LearnAIGame.Bootstrap
{
    /// Deep, editorial palette shared with the marketing site (marketing/index.html) —
    /// muted lime/blue/rose accents on navy, instead of a saturated game-show rainbow.
    public static class GamePalette
    {
        // Backgrounds — layered navy, darkest at the root.
        public static readonly Color BackgroundDeep = FromHex("#0a1119");
        public static readonly Color ScreenSurface = FromHex("#101d29");
        public static readonly Color CardSurface = FromHex("#16232f");
        public static readonly Color ChipSurface = FromHex("#1f303e");

        // Text — light ink on dark surfaces, dark navy on bright accent surfaces.
        public static readonly Color TextLight = FromHex("#f1ede4");
        public static readonly Color TextMuted = FromHex("#bbb6ac");
        public static readonly Color TextDark = FromHex("#0a1119");

        public static readonly Color Lime = FromHex("#cce761");
        public static readonly Color LimeDeep = FromHex("#99b527");
        public static readonly Color Blue = FromHex("#6ea8ff");
        public static readonly Color Rose = FromHex("#e5a0b2");
        public static readonly Color Amber = FromHex("#d9b26a");

        // Left/right swipe choice colors — deliberately not the correct/incorrect
        // accent hues, so dragging never leaks the correct answer.
        public static readonly Color ChoiceA = Blue;
        public static readonly Color ChoiceB = Amber;

        public static readonly Color CorrectAccent = Lime;
        public static readonly Color IncorrectAccent = Rose;

        public static readonly Color ShadowDark = new Color(0f, 0f, 0f, 0.45f);

        // Each card gets one of these as its own subtle background — picked by a
        // stable hash of the card id so it has quiet texture, not a rainbow carnival.
        public static readonly Color[] CardThemes =
        {
            FromHex("#0d151d"),
            FromHex("#101a22"),
            FromHex("#0b131b"),
            FromHex("#122029"),
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
