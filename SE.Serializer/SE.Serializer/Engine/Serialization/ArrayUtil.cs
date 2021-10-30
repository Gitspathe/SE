using SE.Utility;
using System;
using System.Collections.Generic;

namespace SE.Engine.Serialization
{
    public static class SerializerArrayPool
    {
        private static Dictionary<Type, QuickList<RentableArray>> arrays = new Dictionary<Type, QuickList<RentableArray>>();

        public static RentableArray Rent(Type arrayType, int minSize)
        {
            if (!arrays.TryGetValue(arrayType, out QuickList<RentableArray> list)) {
                list = new QuickList<RentableArray>();
                arrays.Add(arrayType, list);
            }

            if (list.Count == 0) {
                list.Add(new RentableArray(arrayType, minSize));
            }

            RentableArray arr = list.Array[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return arr;
        }

        public static Array Return(RentableArray array)
        {
            if (!arrays.TryGetValue(array.ArrayType, out QuickList<RentableArray> val)) {
                throw new Exception("Array not found in pool.");
            }

            Array arr = array.ToArray();
            array.Clear();
            val.Add(array);
            return arr;
        }
    }

    public class RentableArray
    {
        private Array array;

        public Type ArrayType { get; private set; }
        public int Count { get; private set; }

        internal RentableArray(Type arrayType, int count)
        {
            ArrayType = arrayType;
            Count = 0;
            array = Array.CreateInstance(arrayType, count);
        }

        public void Add(object obj)
        {
            EnsureCapacity();
            array.SetValue(obj, Count++);
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++) {
                array.SetValue(null, i);
            }
            Count = 0;
        }

        internal Array ToArray()
        {
            Array returnArray = Array.CreateInstance(ArrayType, Count);
            array.CopyTo(returnArray, 0);
            return returnArray;
        }

        private void EnsureCapacity()
        {
            if (Count + 1 > array.Length) {
                Resize();
            }
        }

        private void Resize()
        {
            Array newArray = Array.CreateInstance(ArrayType, array.Length * 2);
            Array.Copy(array, newArray, array.Length);
            Count = array.Length;
            array = newArray;
        }
    }

}
