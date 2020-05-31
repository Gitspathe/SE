using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SE.Utility;
using SEParticles.Modules;
using SEParticles.Shapes;

namespace SEParticles
{
    public static class ParticleEngine
    {
        public static readonly QuickList<Emitter> Emitters = new QuickList<Emitter>();
        public static readonly QuickList<AreaModule> AreaModules = new QuickList<AreaModule>();

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
                    Parallel.ForEach(AreaModules, module => {
                        foreach (Emitter e in Emitters) {
                            if (module.Shape.Intersects(e.Bounds)) {
                                e.AreaModules.Add(module);
                                module.AttachedEmitters.Add(e);
                            } else {
                                e.AreaModules.Remove(module);
                                module.AttachedEmitters.Remove(e);
                            }
                        }
                    });
                }).ContinueWith((t1) => {
                    Parallel.ForEach(Emitters, emitter => { emitter.Update(deltaTime); });
                });
                if (Synchronous)
                    WaitForThreads();
            } else {
                // TODO: Area modules.
                for (int i = 0; i < Emitters.Count; i++) {
                    Emitters.Array[i].Update(deltaTime);
                }
            }

            // TODO: Temp code. Remove when done.
            //if (AreaModules.Count == 0) {
            //    ScaleModule s = ScaleModule.Lerp(5.0f, 1.0f);
            //    s.AbsoluteValue = true;
            //    AreaParticleModule mod = new AreaParticleModule(new CircleShape(1024.0f));
            //    AreaModules.Add(mod);
            //}
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
            foreach (AreaModule aModule in emitter.AreaModules) {
                aModule.AttachedEmitters.Add(emitter);
            }
        }

        internal static void RemoveEmitter(Emitter emitter)
        {
            if(emitter.ParticleEngineIndex == -1)
                return;

            Emitters.Remove(emitter);
            emitter.ParticleEngineIndex = -1;
            foreach (AreaModule aModule in emitter.AreaModules) {
                aModule.AttachedEmitters.Remove(emitter);
            }
        }

        internal static void AddAreaModule(AreaModule module)
        {
            if(module.AddedToEngine)
                return;

            module.AddedToEngine = true;
            AreaModules.Add(module);
            foreach (Emitter e in module.AttachedEmitters) {
                e.AreaModules.Add(module);
            }
        }

        internal static void RemoveAreaModule(AreaModule module)
        {
            module.AddedToEngine = false;
            AreaModules.Remove(module);
            foreach (Emitter e in module.AttachedEmitters) {
                e.AreaModules.Remove(module);
            }
        }
    }
}
