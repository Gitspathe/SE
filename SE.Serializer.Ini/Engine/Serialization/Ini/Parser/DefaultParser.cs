using SE.Utility;

namespace SE.Serialization.Ini.Parser
{
    public class DefaultParser : IIniParser
    {
        public IniData Parse(string iniString, ParserSettings settings)
        {
            ParserSettings.ParserCharacters chars = settings.Characters;
            string[] lines = iniString.Split(chars.NewLine);

            IniData iniData = new IniData();
            IniSection currentSection = new IniSection();
            QuickList<string> nextNodeComments = new QuickList<string>();
            int lineIndex = 0;

            while (lineIndex < lines.Length) {
                string line = lines[lineIndex];

                if (string.IsNullOrEmpty(line)) {
                    lineIndex++;
                    continue;
                }
                
                if (line.StartsWith(chars.Comment)) {
                    // Line is a comment.
                    nextNodeComments.Add(line.Substring(1));

                } else if(line.StartsWith(chars.SectionStart)) {
                    // Line indicates start of a new section.
                    iniData.Sections.Add(currentSection);
                    currentSection = new IniSection(
                        line.ReadBetween(chars.SectionStart, chars.SectionEnd), 
                        nextNodeComments.Copy(), 
                        null);

                    nextNodeComments.Clear();
                } else {
                    // Line is a key+value pair.
                    (string key, string value) = line.GetKeyValuePair(chars.Separator);
                    currentSection.Nodes.Add(new IniNode(
                        key, 
                        value, 
                        nextNodeComments.Copy()));

                    nextNodeComments.Clear();
                }

                lineIndex++;
            }

            iniData.Sections.Add(currentSection);
            return iniData;
        }

        public string Compose(IniData iniData, ParserSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}
