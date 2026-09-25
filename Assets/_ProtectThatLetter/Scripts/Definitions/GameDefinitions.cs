namespace ProtectThatLetter.Definitions {
    /// <summary>
    /// Centralized game-wide constants, organized by category.
    /// </summary>
    public static class GameDefinitions {
        /// <summary>
        /// Language codes and default language.
        /// </summary>
        public static class Languages {
            public const string VIETNAMESE = "vi";           // Vietnamese language code
            public const string ENGLISH = "en";              // English language code

            public const string DEFAULT_LANGUAGE = ENGLISH; // Default language
        }

        /// <summary>
        /// Localization-related constants.
        /// </summary>
        public static class Localization {
            public const string STRING_TABLE_NAME = "PTL_String_Tables"; // String table name
        }
    }
}