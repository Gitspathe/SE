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

        public static void WriteQuotedText(this Utf8Writer writer, string str)
        {
            writer.Write(_STRING_IDENTIFIER);
            writer.Write(Encoding.UTF8.GetBytes(str));
            writer.Write(_STRING_IDENTIFIER);
        }

        // TODO: ReadArray<T>, to read array entries separated with ','.

        public static string ReadQuotedString(this Utf8Reader reader)
        {
            strBuilder.Clear();
            bool firstStrIdentifier = true;
            while (true) {
                try {
                    byte c = reader.ReadByte();
                    switch (c) {
                        case _NEW_LINE:
                        case _END_ARRAY:
                            break;

                        case _STRING_IDENTIFIER: {
                            if (!firstStrIdentifier) {
                                return strBuilder.ToString();
                            } else {
                                firstStrIdentifier = false;
                            }
                        } continue;

                        default:
                            strBuilder.Append((char) c);
                            break;
                    }
                } catch (Exception) {
                    break;
                }
            }
            return strBuilder.ToString();
        }

        public static string ReadUntil(this Utf8Reader reader, byte symbol, bool skipPast = true)
        {
            strBuilder.Clear();
            while (true) {
                try {
                    byte c = reader.ReadByte();
                    if (c != symbol) {
                        strBuilder.Append((char)c);
                        continue;
                    }

                    if (!skipPast) {
                        reader.BaseStream.Position -= 1;
                    }
                    break;
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
