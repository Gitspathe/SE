using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

// ReSharper disable InconsistentNaming

namespace DeeZ.Engine.Utility
{
    /// <summary>
    /// Exposes direct access to it's inner array.
    /// Exercise caution when working with the inner array, as there are no sanity checks.
    /// QuickList is not thread-safe. Remember to wrap it in locks when writing multi-threaded code.
    /// </summary>
    /// <typeparam name="T">Type of the inner array.</typeparam>
    public sealed class QuickList<T> : IEnumerable<T>
    {
        /// <summary>Inner array/buffer.</summary>
        public T[] Array;

        /// <summary>Elements length. DO NOT MODIFY!</summary>
        public int Count;

        private int bufferLength; // Faster to cache the array length.

        /// <summary>
        /// NOTE: Directly accessing the Buffer is preferred, as that would be faster.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [Obsolete("It's preferred to access the inner array directly via the " + nameof(Array) + " field.")]
        public T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array[index];
        }

        /// <summary>
        /// Resets the length back to zero without clearing the items.
        /// </summary>
        public void Reset() => Count = 0;

        /// <summary>
        /// Clears the list of all it's items.
        /// </summary>
        public void Clear()
        {
            System.Array.Clear(Array, 0, Array.Length);
            Count = 0;
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="item">The item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Hot path.
        public void Add(T item)
        {
            if (Count < bufferLength) {
                Array[Count++] = item;
            } else {
                AddWithResize(item);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // Cold path.
        private void AddWithResize(T item)
        {
            T[] newItems = new T[bufferLength * 2];
            System.Array.Copy(Array, 0, newItems, 0, bufferLength);
            Array = newItems;
            bufferLength = Array.Length;
            Array[Count++] = item;
        }

        public void AddRange(QuickList<T> items)
        {
            EnsureCapacity(items.Count);
            System.Array.Copy(items.Array, 0, Array, Count, items.Count);
            Count += items.Count;
        }

        public void AddRange(List<T> items)
        {
            EnsureCapacity(items.Count);
            items.CopyTo(0, Array, Count, items.Count);
            Count += items.Count;
        }

        public void AddRange(T[] items)
        {
            EnsureCapacity(items.Length);
            System.Array.Copy(items, 0, Array, Count, items.Length);
            Count += items.Length;
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // Cold path.
        public void EnsureCapacity(int additionalItems = 1)
        {
            if (Count + additionalItems >= bufferLength) {
                T[] newItems = new T[bufferLength + additionalItems];
                System.Array.Copy(Array, 0, newItems, 0, bufferLength);
                Array = newItems;
                bufferLength = Array.Length;
            }
        }

        /// <summary>
        /// Adds a enumerable range of items.
        /// </summary>
        /// <param name="enumerable">IEnumerable collection.</param>
        public void AddRange(IEnumerable<T> enumerable)
        {
            foreach (T item in enumerable)
                Add(item);
        }

        /// <summary>
        /// Sorts the list with a given comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(IComparer comparer) 
            => System.Array.Sort(Array, 0, Count, comparer);

        /// <summary>
        /// Sorts the list with a given comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort(IComparer<T> comparer)
            => System.Array.Sort(Array, 0, Count, comparer);

        /// <summary>
        /// Checks if an item is present in the list. Uses the default equality comparer to test.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>True if the item is present, false otherwise.</returns>
        public bool Contains(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; ++i) {
                if (comparer.Equals(item, Array[i])) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes an element at a specific index.
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        public void RemoveAt(int index)
        {
            Count--;
            if(index < Count)
                System.Array.Copy(Array, index + 1, Array, index, Count - index);

            Array[Count] = default;
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>True if the item was present and was removed, false otherwise.</returns>
        public bool Remove(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; ++i) {
                if (comparer.Equals(item, Array[i])) {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a new QuickList instance.
        /// </summary>
        /// <param name="size">Initial capacity. Automatically resizes.</param>
        public QuickList(int size)
        {
            size = Math.Max(1, size);
            Array = new T[size];
            bufferLength = Array.Length;
        }

        public QuickList() : this(8) { }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private struct Enumerator : IEnumerator<T>
        {
            private QuickList<T> quickList;
            private int curIndex;

            public bool MoveNext()
            {
                curIndex++;
                return (curIndex < quickList.Count);
            }

            public void Reset() => curIndex = -1;

            public T Current => quickList.Array[curIndex];

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public Enumerator(QuickList<T> quickList)
            {
                curIndex = -1;
                this.quickList = quickList;
            }
        }
    }
}
