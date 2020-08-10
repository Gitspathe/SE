using System;

namespace SE.Serialization.Ini.Parser
{
    public class ParserSettings
    {
        public ParserCharacters Characters = new ParserCharacters();
        public IIniParser Parser = new DefaultParser();

        public static ParserSettings Default { get; } = new ParserSettings();

        public class ParserCharacters
        {
            public string NewLine = Environment.NewLine;
            public char Separator = '=';
            public char Comment = '#';
            public char SectionStart = '[';
            public char SectionEnd = ']';
            public char Continuation = '\\';
        }
    }
}
