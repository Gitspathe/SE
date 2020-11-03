using System;
using System.Runtime.CompilerServices;
using System.Text;
using FastStream;
using static SE.Core.Serializer;

namespace SE.Serialization
{
    public static class SerializerUtil
    {
        private static StringBuilder strBuilder = new StringBuilder(255);

        public static void WriteText(this Utf8Writer writer, string str)
        {
            writer.Write(Encoding.UTF8.GetBytes(str));
        }

        public static string ReadQuotedString(this Utf8Reader reader)
        {
            strBuilder.Clear();
            while (true) {
                try {
                    byte c = reader.ReadByte();
                    switch (c) {
                        case _ARRAY_SEPARATOR:
                        case _NEW_LINE: 
                        case _END_ARRAY:
                        case _STRING_IDENTIFIER:
                            return strBuilder.ToString();
                        
                        default:
                            strBuilder.Append(c);
                            break;
                    }
                } catch (Exception) {
                    break;
                }
            }
            return strBuilder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteIndent(this Utf8Writer writer, int depth)
        {
            for (int i = 2; i < depth*2; i++) {
                writer.Write(_TAB);
            }
        }
    }
}
