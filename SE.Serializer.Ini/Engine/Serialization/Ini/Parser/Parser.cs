namespace SE.Serialization.Ini.Parser
{
    public interface IIniParser
    {
        IniData Parse(string iniString, ParserSettings settings);
        string Compose(IniData iniData, ParserSettings settings);
    }

    public static class ParserStringExtensions
    {
        public static string[] Trim(this string[] strArray)
        {
            for (int i = 0; i < strArray.Length; i++) {
                strArray[i] = strArray[i].Trim();
            }
            return strArray;
        }

        public static bool StartsWith(this string str, char character)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            return str[0] == character;
        }

        public static bool EndsWith(this string str, char character)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            return str[str.Length - 1] == character;
        }

        public static string ReadBetween(this string str, char start, char end)
        {
            if (string.IsNullOrEmpty(str) || str[0] != start || str[str.Length - 1] != end)
                return null;

            return str.Substring(1, str.Length - 2);
        }

        public static (string, string) GetKeyValuePair(this string str, char separator)
        {
            if (string.IsNullOrEmpty(str))
                return (null, null);

            string[] arr = str.Split(separator, 2);
            return (arr[0], arr[1]);
        }
    }
}
