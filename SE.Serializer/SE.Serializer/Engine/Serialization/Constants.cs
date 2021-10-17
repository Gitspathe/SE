namespace SE.Serialization
{
    public static class Constants
    {
        // TODO: Escape characters to prevent corruption when reading. https://www.tutorialspoint.com/json_simple/json_simple_escape_characters.htm
        // Remember that "\n" (two characters) is ecaped to become '\n' (one character), etc. See EscapedNewLine.
        public const byte _ESCAPE = (byte)'\\';

        public const byte _BEGIN_VALUE = (byte)':';
        public const byte _BEGIN_CLASS = (byte)'{';
        public const byte _END_CLASS = (byte)'}';
        public const byte _BEGIN_ARRAY = (byte)'[';
        public const byte _END_ARRAY = (byte)']';
        public const byte _BEGIN_META = (byte)'(';
        public const byte _END_META = (byte)')';
        public const byte _ARRAY_SEPARATOR = (byte)',';
        public const byte _NEW_LINE = (byte)'\n';
        public const byte _TAB = (byte)' ';
        public const byte _STRING_IDENTIFIER = (byte)'"';

        public static readonly byte[] Tabs = { _TAB, _TAB };
        public static readonly byte[] NullValue = { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };
        public static readonly byte[] NaN = { (byte)'N', (byte)'a', (byte)'N' };
        public static readonly byte[] TrueValue = { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
        public static readonly byte[] FalseValue = { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };

        public static readonly byte[] EscapedNewLine = { _ESCAPE, (byte)'n' };
        public static readonly byte[] EscapedDoubleQuote = { _ESCAPE, (byte)'"' };
        public static readonly byte[] EscapedBackslash = { _ESCAPE, _ESCAPE };
    }
}
