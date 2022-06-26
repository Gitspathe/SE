using SE.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SE.Threading
{
    public static class ThreadManager
    {
        public static int ThreadCount { get; private set; }
        public static bool SingleThreadMode { get; private set; }

        internal static int IdleThreads;

        private static SEThread[] threads;
        private static bool initialized;

        public static void Initialize(int threadCount = -1)
        {
            if (initialized)
                throw new Exception("Already initialized.");

            ThreadCount = threadCount == -1 ? 8 : Math.Clamp(threadCount, 0, int.MaxValue);
            SingleThreadMode = ThreadCount == 0;
            IdleThreads = ThreadCount;

            // TODO: Single threaded mode - Run everything on the main thread.

            CreateThreads();
            initialized = true;
        }

        public static void RunParallelTasks(IThreadTask[] tasks)
        {
            if (!initialized)
                throw new Exception("Not initialized.");

            int curThread = 0;
            for (int i = 0; i < tasks.Length; i++) {
                if (curThread > ThreadCount) {
                    curThread = 0;
                }

                threads[curThread].GiveWork(tasks[i]);
                curThread++;
            }
            ExecuteThreads();
        }

        private static void ExecuteThreads()
        {
            int numActiveThreads = 0;
            for (int i = 0; i < threads.Length; i++) {
                if (threads[i].TaskCount > 0) {
                    threads[i].FlagExec(numActiveThreads);
                    numActiveThreads++;
                }
            }

            lock (SEThread.monitor) {
                Monitor.PulseAll(SEThread.monitor);
            }

            Wait();
        }

        private static void Wait()
        {
            while (IdleThreads != threads.Length) { }
        }

        private static void CloseThreads()
        {
            //todo.
        }

        private static void CreateThreads()
        {
            threads = new SEThread[ThreadCount];
            for (int i = 0; i < ThreadCount; i++) {
                threads[i] = new SEThread();
            }
        }
    }

    internal sealed class SEThread
    {
        public ThreadState ThreadState { get; private set; }
        public int TaskCount => tasks.Count;

        private QuickList<IThreadTask> tasks = new QuickList<IThreadTask>();
        private Thread thread;
        private Exception exception;
        private int threadNum;

        public static object monitor = new object();

        public SEThread()
        {
            ThreadState = ThreadState.Idle;

            thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Name = "SE Thread Manager";
            thread.Start();
        }

        public Exception CheckException()
        {
            return exception;
        }

        public void GiveWork(IThreadTask task)
        {
            if (ThreadState == ThreadState.Running)
                throw new Exception("Thread is currently running.");
            if (ThreadState == ThreadState.Exception)
                throw new Exception("Exception occurred in thread.", exception);
            if (ThreadState == ThreadState.Close)
                throw new Exception("Thread is closed.");

            tasks.Add(task);
        }

        public void FlagExec(int threadNum)
        {
            this.threadNum = threadNum;
            ThreadState = ThreadState.Running;
            Interlocked.Decrement(ref ThreadManager.IdleThreads);
        }

        public void Run()
        {
            //Thread.BeginThreadAffinity();
            while (true) {
                lock (monitor) {
                    Monitor.Wait(monitor);
                }

                if (ThreadState == ThreadState.Close || ThreadState == ThreadState.Exception)
                    break; // TODO exception handling.

                // Run task if not idle.
                try {

                    IThreadTask[] arr = tasks.Array;
                    for (int i = 0; i < tasks.Count; i++) {
                        arr[i].Execute();
                    }
                    
                } catch (Exception e) {
                    ThreadState = ThreadState.Exception;
                    exception = e;
                    break;
                }
                
                if (ThreadState == ThreadState.Exception)
                    break;

                // Clean up for the next call.
                tasks.Clear();
                ThreadState = ThreadState.Idle;
                Interlocked.Increment(ref ThreadManager.IdleThreads);
            }
            //Thread.EndThreadAffinity();
        }
    }

    public interface IThreadTask
    {
        void Execute();
        // TODO: uint Complexity
    }

    internal enum ThreadState
    {
        Idle,
        Running,
        Exception,
        Close
    }
}
