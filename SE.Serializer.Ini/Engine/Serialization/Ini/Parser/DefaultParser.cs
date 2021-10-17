using SE.Utility;
using System.Linq;

namespace SE.Serialization.Ini.Parser
{
    public class DefaultParser : IIniParser
    {
        // TODO: Array support. Separate values with ','

        public IniData Parse(string iniString, ParserSettings settings)
        {
            ParserSettings.ParserCharacters chars = settings.Characters;
            QuickList<string> linesList = new QuickList<string>();
            string[] lines = iniString.Split(chars.NewLine);

            // Trim whitespace.
            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                linesList.Add(line.Trim());
            }

            // Merge lines with continuation character.
            for (int i = 0; i < linesList.Count; i++) {
                string line = linesList.Array[i];
                while (line.EndsWith(chars.Continuation)) {
                    line = line.Substring(0, line.Length - 1);
                    if (i + 1 > linesList.Count)
                        break;

                    line += linesList.Array[i + 1];
                    linesList.RemoveAt(i + 1);
                }
                linesList.Array[i] = line;
            }

            // Finally, convert the finished lines into an array.
            lines = linesList.ToArray();

            IniData iniData = new IniData();
            IniSection currentSection = new IniSection();
            QuickList<string> nextNodeComments = new QuickList<string>();
            int lineIndex = 0;

            // TODO: Support special characters in strings / values via " and ' characters.

            while (lineIndex < lines.Length) {
                string line = lines[lineIndex];

                if (line.StartsWith(chars.Comment)) {
                    // Line is a comment.
                    nextNodeComments.Add(line.Substring(1));

                } else if (line.StartsWith(chars.SectionStart)) {
                    // Line indicates start of a new section.
                    iniData.AddSection(currentSection);
                    currentSection = new IniSection(
                        line.ReadBetween(chars.SectionStart, chars.SectionEnd),
                        nextNodeComments.Copy(),
                        null);

                    nextNodeComments.Clear();
                } else {
                    // Line is a key+value pair.
                    (string key, string value) = line.GetKeyValuePair(chars.Separator);

                    string[] values = value.Split(chars.ArraySeparator).Trim();
                    currentSection.AddNode(new IniNode(
                        key,
                        new QuickList<string>(values),
                        nextNodeComments.Copy()));

                    nextNodeComments.Clear();
                }

                lineIndex++;
            }

            iniData.AddSection(currentSection);
            return iniData;
        }

        public string Compose(IniData iniData, ParserSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}
