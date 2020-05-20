using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BenchmarkDotNet.Attributes;
using SE.Utility;

namespace DeeZ.PerformanceTests
{
    public class Loops
    {

        [SimpleJob(1, 10, 10, 10_000)]
        public class Access
        {
            public TestStruct[] structs = new TestStruct[50_000];

            [GlobalSetup]
            public void GlobalSetup()
            {
                for (int i = 0; i < structs.Length; i++) {
                    structs[i] = new TestStruct();
                }
            }

            [Benchmark(Baseline = true)]
            public void AccessNormal()
            {
                int size = structs.Length;
                for (int i = 0; i < size; i++) {
                    TestStruct s = structs[i];
                    s.X = s.Y + s.Z;
                }
            }

            [Benchmark]
            public unsafe void AccessUnsafe()
            {
                fixed (TestStruct* ptr = structs) {
                    int size = structs.Length;
                    for (int i = 0; i < size; i++) {
                        TestStruct s = ptr[i];
                        s.X = s.Y + s.Z;
                    }
                }
            }

            [Benchmark]
            public unsafe void AccessUnsafeDarkMagic()
            {
                TestStruct[] ptrArray = structs;
                int size = structs.Length;
                fixed (TestStruct* ptr = ptrArray) {
                    for (int i = 0; i < size; i++) {
                        var local = &ptr[i];
                        local->X = local->Y + local->Z;
                    }
                }
            }

            [Benchmark]
            public void AccessRef()
            {
                int size = structs.Length;
                for (int i = 0; i < size; i++) {
                    ref TestStruct s = ref structs[i];
                    s.X = s.Y + s.Z;
                }
            }

            [GlobalCleanup]
            public void Cleanup()
            {
                structs = null;
                GC.Collect();
            }
        }

        public struct TestStruct
        {
            public int X;
            public int Y;
            public int Z;
            public Vector4 Vector;

            public void ExpensiveCall()
            {
                X = (int)Math.Sqrt(5);
                Y = (int)Math.Sqrt(5);
                Z = (int)Math.Sqrt(5);
            }
        }
    }
}
