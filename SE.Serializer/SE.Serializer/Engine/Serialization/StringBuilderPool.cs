using System;
using System.Text;

namespace SE.Serialization
{
    internal static class StringBuilderPool
    {
        [ThreadStatic]
        private static StringBuilder stringBuilder;

        [ThreadStatic] 
        private static bool rented;

        public static StringBuilder Rent()
        {
            if (rented)
                throw new Exception();

            rented = true;
            return stringBuilder ??= new StringBuilder(128);
        }

        public static string ReturnAndGetString(StringBuilder builder)
        {
            if (builder != stringBuilder)
                throw new Exception();

            rented = false;
            builder.Clear();
            return builder.ToString();
        }
    }
}
