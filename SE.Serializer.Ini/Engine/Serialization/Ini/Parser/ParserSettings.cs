namespace SE.Serialization.Ini.Parser
{
    public class ParserSettings
    {
        public ParserCharacters Characters = new ParserCharacters();

        public static ParserSettings Default { get; } = new ParserSettings();

        public class ParserCharacters
        {
            public char NewLine = '\n';
            public char Separator = '=';
            public char Comment = '#';
            public char SectionStart = '[';
            public char SectionEnd = ']';
        }
    }
}
