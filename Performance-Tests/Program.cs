using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace DeeZ.PerformanceTests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //BenchmarkRunner.Run<ListAccess>();
            //BenchmarkRunner.Run<ListAdd>();
            //BenchmarkRunner.Run<ListRemove>();

            BenchmarkRunner.Run<Loops.Access>();
        }
    }
}
