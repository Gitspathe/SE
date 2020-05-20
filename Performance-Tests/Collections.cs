using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SE.Utility;

// ReSharper disable AssignmentIsFullyDiscarded

namespace DeeZ.PerformanceTests
{
    [SimpleJob(1, 10, 10, 1_000_000)]
    public class ListAdd
    {
        private List<int> normalList = new List<int>(1_000_000);
        private QuickList<int> quickList = new QuickList<int>(1_000_000);

        [Benchmark(Baseline = true)]
        public void AddList()
        {
            normalList.Add(0);
        }

        [Benchmark]
        public void AddQuickList()
        {
            quickList.Add(0);
        }

        [IterationCleanup]
        public void Cleanup()
        {
            normalList.Clear();
            quickList.Clear();
            GC.Collect();
        }
    }

    [SimpleJob(1, 10, 10, 10_000)]
    public class ListRemove
    {
        private List<int> normalList = new List<int> { 10 };
        private QuickList<int> quickList = new QuickList<int> { 10 };

        [Benchmark(Baseline = true)]
        public void RemoveList()
        {
            normalList.RemoveAt(0);
        }

        [Benchmark]
        public void RemoveQuickList()
        {
            quickList.RemoveAt(0);
        }

        [IterationSetup]
        public void Setup()
        {
            for (int i = 0; i < 100_000; i++) {
                normalList.Add(i);
                quickList.Add(i);
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            normalList = null;
            quickList = null;
            GC.Collect();
        }
    }

    [SimpleJob(1, 10, 10, 1_000_000)]
    public class ListAccess
    {
        private List<int> normalList = new List<int> { 10, 15, 20, 25, 30 };
        private QuickList<int> quickList = new QuickList<int> { 10, 15, 20, 25, 30 };

        [Benchmark(Baseline = true)]
        public void AccessList()
        {
            _ = normalList[0];
        }

        [Benchmark]
        public void AccessQuickList()
        {
            _ = quickList.Array[0];
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            normalList = null;
            quickList = null;
            GC.Collect();
        }
    }
}
