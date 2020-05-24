using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SE.Engine.Components;
using SE.Utility;
using SEParticles;

namespace SE.Core
{
    public static class NewParticleEngine
    {
        public static QuickList<NewTestParticleEmitter> Emitters = new QuickList<NewTestParticleEmitter>();

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
                foreach (NewTestParticleEmitter emitter in Emitters) {
                    total += emitter.Emitter.ActiveParticles.Length;
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
                        emitter.Emitter.Update(deltaTime);
                    });
                });
                if (Synchronous)
                    WaitForThreads();
            } else {
                for (int i = 0; i < Emitters.Count; i++) {
                    Emitters.Array[i].Emitter.Update(deltaTime);
                }
            }
        }

        public static void WaitForThreads()
        {
            if (updateTask != null && !updateTask.IsCompleted) {
                updateTask.Wait();
            }
        }

        internal static void AddEmitter(NewTestParticleEmitter emitter)
        {
            if(emitter.AddedToParticleEngine)
                return;

            emitter.ParticleEngineIndex = Emitters.Count;
            Emitters.Add(emitter);
            emitter.AddedToParticleEngine = true;
        }

        internal static void RemoveEmitter(NewTestParticleEmitter emitter)
        {
            if(emitter.ParticleEngineIndex == -1)
                return;

            Emitters.RemoveAt(emitter.ParticleEngineIndex);
            emitter.AddedToParticleEngine = false;
            emitter.ParticleEngineIndex = -1;
        }

    }
}
