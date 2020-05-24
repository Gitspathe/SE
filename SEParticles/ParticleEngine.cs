using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SE.Utility;

namespace SEParticles
{
    public static class ParticleEngine
    {
        public static QuickList<Emitter> Emitters = new QuickList<Emitter>();

        public static bool MultiThreaded {
            get => multiThreaded;
            set {
                if (multiThreaded)
                    WaitForThreads();

                multiThreaded = value;
            }
        }
        private static bool multiThreaded = true;

        public static bool Synchronous { get; set; } = false;

        public static int ParticleCount { 
            get  { 
                int total = 0;
                foreach (Emitter emitter in Emitters) {
                    total += emitter.ActiveParticles.Length;
                }
                return total;
            }
        }

        private static Task updateTask;

        public static void Update(float deltaTime)
        {
            if (MultiThreaded) {
                updateTask = Task.Factory.StartNew(() => {
                    Parallel.ForEach(Emitters, emitter => {
                        emitter.Update(deltaTime);
                    });
                });
                if (Synchronous)
                    WaitForThreads();
            } else {
                for (int i = 0; i < Emitters.Count; i++) {
                    Emitters.Array[i].Update(deltaTime);
                }
            }
        }

        public static void WaitForThreads()
        {
            if (updateTask != null && !updateTask.IsCompleted) {
                updateTask.Wait();
            }
        }

        internal static void AddEmitter(Emitter emitter)
        {
            if(emitter.ParticleEngineIndex != -1)
                return;

            emitter.ParticleEngineIndex = Emitters.Count;
            Emitters.Add(emitter);
        }

        internal static void RemoveEmitter(Emitter emitter)
        {
            if(emitter.ParticleEngineIndex == -1)
                return;

            Emitters.RemoveAt(emitter.ParticleEngineIndex);
            emitter.ParticleEngineIndex = -1;
        }
    }
}
