using System;
using System.Collections.Generic;
using System.Text;

namespace DeeZ.Core.Extensions
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
