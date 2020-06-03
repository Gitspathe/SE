using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SE.Utility;
using SEParticles.AreaModules;
using SEParticles.Modules;
using SEParticles.Shapes;
using Vector2 = System.Numerics.Vector2;

namespace SEParticles
{
    public static class ParticleEngine
    {
        public static readonly QuickList<Emitter> Emitters = new QuickList<Emitter>();
        public static readonly QuickList<AreaModule> AreaModules = new QuickList<AreaModule>();

        public static int ParticleCount { 
            get  { 
                int total = 0;
                foreach (Emitter emitter in Emitters) {
                    total += emitter.ActiveParticles.Length;
                }
                return total;
            }
        }

        /// <summary>Controls how particles are allocated for new emitters. CANNOT be changed after Initialize() has been called.</summary>
        public static ParticleAllocationMode AllocationMode {
            get => allocationMode;
            set {
                if (Initialized)
                    throw new InvalidOperationException("Cannot change allocation mode after the particle engine has been initialized.");

                allocationMode = value;
            }
        }
        private static ParticleAllocationMode allocationMode = ParticleAllocationMode.ArrayPool;

        /// <summary>Controls how the particle engine updates.</summary>
        public static UpdateMode UpdateMode = UpdateMode.ParallelAsynchronous;

        internal static bool Initialized = false;

        private static Task updateTask;
        private static bool temp = true;

        public static void Initialize()
        {
            Initialized = true;
        }

        public static void Update(float deltaTime)
        {
            if (!Initialized)
                throw new InvalidOperationException("Particle engine has not been initialized. Call ParticleEngine.Initialize() first.");

            switch (UpdateMode) {
                case UpdateMode.ParallelAsynchronous: {
                    CreateTasks(deltaTime);
                } break;
                case UpdateMode.ParallelSynchronous: {
                    CreateTasks(deltaTime);
                    WaitForThreads();
                } break;
                case UpdateMode.Synchronous: {
                    // Update area modules.
                    foreach (AreaModule aMod in AreaModules) {
                        for (int i = 0; i < Emitters.Count; i++) {
                            Emitter e = Emitters.Array[i];
                            if (aMod.Shape.Intersects(e.Bounds)) {
                                e.AreaModules.Add(aMod);
                                aMod.AttachedEmitters.Add(e);
                            } else {
                                e.AreaModules.Remove(aMod);
                                aMod.AttachedEmitters.Remove(e);
                            }
                        }
                    }

                    // Update emitters.
                    for (int i = 0; i < Emitters.Count; i++) {
                        Emitters.Array[i].Update(deltaTime);
                    }
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO: Temp code. Remove when done.
            if (temp) {
                ForceModule mod = ForceModule.Attract(
                    new CircleShape(1024.0f), 
                    new Vector2(1024.0f, 1024.0f), 
                    256.0f, 
                    1024.0f);

                AreaModules.Add(mod);

                //AttractorModule mod2 = new AttractorModule(new CircleShape(512.0f), new System.Numerics.Vector2(2048.0f, 1024.0f));
                //AreaModules.Add(mod2);

                temp = false;
            }
        }

        private static void CreateTasks(float deltaTime)
        {
            updateTask = Task.Factory.StartNew(() => {
                // Update area modules.
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
                // Update emitters.
                Parallel.ForEach(Emitters, emitter => { emitter.Update(deltaTime); });
            });
        }

        public static void WaitForThreads()
        {
            if (!Initialized)
                throw new InvalidOperationException("Particle engine has not been initialized. Call ParticleEngine.Initialize() first.");

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

    /// <summary>
    /// Controls how particles are allocated for new emitters.
    /// </summary>
    public enum ParticleAllocationMode
    {
        /// <summary>Particles are allocated using standard arrays. Most useful when fewer emitters are created and destroyed at runtime,
        ///          such as when particle emitters are pooled.</summary>
        Array,

        /// <summary>Particles are allocated using arrays, which are rented and returned to the shared array pool.
        ///          This option will result in less garbage generation, and faster instantiation of new particle emitters.
        ///          Most useful when creating and destroying many particle emitters at runtime.
        ///          However, this option may result in a buildup of memory usage due to how ArrayPool.Shared internally works.</summary>
        ArrayPool

        // TODO: AllocHGlobal mode?
    }

    /// <summary>
    /// Controls how the particle engine updates.
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>Update is done using Parallel loops, within tasks. ParticleEngine.WaitForThreads() must be called when the state
        ///          is to be synchronized. For example, ParticleEngine.WaitForThreads() would be called before you render or query the particles.
        ///          Results in better performance on machines with 2+ cores.</summary>
        ParallelAsynchronous,

        /// <summary>Update is done synchronously using Parallel loops. Results in betters performance on machines with 2+ cores.
        ///          State is synchronized immediately after Update() has finished processing.</summary>
        ParallelSynchronous,

        /// <summary>Update is done synchronously on whatever thread Update() was called from. Results in lower performance on machines with
        ///          2+ cores, and potentially better performance on machines with 1-2 cores.</summary>
        Synchronous
    }
}
