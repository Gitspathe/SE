using System;
using System.Text;
using FastStream;
using static SE.Core.Serializer;

namespace SE.Serialization
{
    public static class SerializerUtil
    {
        private static StringBuilder strBuilder = new StringBuilder(255);

        public static void WriteText(this FastMemoryWriter writer, string str)
        {
            writer.Write(Encoding.Unicode.GetBytes(str));
        }

        public static string ReadQuotedString(this FastReader reader)
        {
            strBuilder.Clear();
            while (true) {
                try {
                    char c = reader.ReadChar();
                    if (c != _ARRAY_SEPARATOR && c != _NEW_LINE && c != _END_ARRAY) {
                        strBuilder.Append(c);
                    } else {
                        break;
                    }
                } catch (Exception) {
                    break;
                }
            }
            return strBuilder.ToString();
        }

        public static void WriteIndent(this FastMemoryWriter writer, int depth)
        {
            for (int i = 1; i < depth; i++) {
                writer.Write(Tabs);
            }
        }
    }
}
