using System.Collections.Generic;

namespace SE.Core.Extensions
{
    public static class HashsetExtensions
    {

        public static void AddRange<T>(this HashSet<T> hashset, HashSet<T> toAdd)
        {
            foreach (T entry in toAdd) {
                hashset.Add(entry);
            }
        }

    }
}
