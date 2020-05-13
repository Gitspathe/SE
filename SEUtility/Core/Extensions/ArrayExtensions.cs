using System;

namespace DeeZ.Core.Extensions
{
    public static class ArrayExtensions
    {
        public static bool Contains<T>(this T[] array, T check)
        {
            if(array == null)
                return false;

            return Array.IndexOf(array, check) != -1;
        }
    }
}
