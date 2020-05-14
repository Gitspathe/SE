using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SE.Engine.Utility;
using SE.Particles;

namespace SE.Core
{
    public static class ParticleEngine
    {
        public static bool SafeThreaded;

        internal const int _DEFAULT_MAX_THREADS = 8;
        internal const int _DEFAULT_PARTICLE_LISTS = 100;
        internal const int _DEFAULT_PARTICLE_POOL_SIZE = 100_000;

        internal static int VisibleNum;
        internal static QuickList<Particle> ParticlePool;
        internal static QuickList<QuickList<ParticleSystem>> ParticleSystems;
        internal static QuickList<ParticleSystem> VisibleParticleSystems = new QuickList<ParticleSystem>(32);
        internal static Dictionary<int, ParticleRendererContainer> AlphaParticles = new Dictionary<int, ParticleRendererContainer>();
        internal static Dictionary<int, ParticleRendererContainer> AlphaParticlesUnlit = new Dictionary<int, ParticleRendererContainer>();
        internal static Dictionary<int, ParticleRendererContainer> AdditiveParticles = new Dictionary<int, ParticleRendererContainer>();
        internal static QuickList<Particle> PendingDisable;

        private static ParallelOptions parallelOptions = new ParallelOptions {
            MaxDegreeOfParallelism = _DEFAULT_MAX_THREADS
        };
        private static Task updateTask;
        private static int curList;

        private static bool multithreaded = true;
        public static bool Multithreaded
        {
            get => multithreaded;
            set {
                if (value == multithreaded)
                    return;

                multithreaded = value;
                if (value == false) {
                    Initialize(particlePoolSize, 1, 1);
                } else {
                    Initialize(particlePoolSize, particleLists, maxThreads);
                }
            }
        }

        private static int maxThreads = _DEFAULT_MAX_THREADS;
        public static int MaxThreads
        {
            get => maxThreads;
            set {
                if (value == maxThreads)
                    return;

                if (value < 1) {
                    multithreaded = false;
                } else {
                    multithreaded = true;
                }
                maxThreads = value;
                Initialize(particlePoolSize, particleLists, maxThreads);
            }
        }

        private static int particleLists = _DEFAULT_PARTICLE_LISTS;
        public static int ParticleLists
        {
            get => particleLists;
            set {
                if (value == particleLists)
                    return;
                if (value < maxThreads)
                    value = maxThreads;

                particleLists = value;
                Initialize(particlePoolSize, particleLists, maxThreads);
            }
        }

        private static int particlePoolSize = _DEFAULT_PARTICLE_POOL_SIZE;
        public static int ParticlePoolSize {
            get => particlePoolSize;
            set {
                if (value == particlePoolSize)
                    return;
                if (value <= 0)
                    value = 0;

                particlePoolSize = value;
                Initialize(particlePoolSize, particleLists, maxThreads);
            }
        }

        private static float deltaTime;
        private static float updateRate = 1.0f / 144.0f;
        public static float UpdateRate {
            get => updateRate;
            set => updateRate = Math.Clamp(value, 1.0f / 1000.0f, 1.0f / 10);
        }

        internal static void Initialize(int particlePoolSize = _DEFAULT_PARTICLE_POOL_SIZE, int particleCacheLists = _DEFAULT_PARTICLE_LISTS, int maximumThreads = _DEFAULT_MAX_THREADS)
        {
            // Clean up and preparation.
            FinalizeThreads();
            DisablePending();
            parallelOptions.MaxDegreeOfParallelism = maximumThreads;
            QuickList<ParticleSystem> existing = null;
            int existingParticleNum = 0;

            // If particle systems exist, prepare to re-add them.
            if (ParticleSystems != null) {
                existing = new QuickList<ParticleSystem>();
                for (int i = 0; i < ParticleSystems.Count; i++) {
                    existing.AddRange(ParticleSystems.Array[i]);
                    for (int ii = 0; ii < ParticleSystems.Array[i].Count; ii++) {
                        existingParticleNum += ParticleSystems.Array[i].Array[ii].ActiveParticles.Count;
                    }
                }
                ParticleSystems.Clear();
                PendingDisable.Clear();
                curList = 0;
            }

            // Setup particle pool.
            ParticlePool = new QuickList<Particle>(particlePoolSize);
            int start = Math.Clamp(existingParticleNum, 0, particlePoolSize);
            for (int i = start; i < particlePoolSize; i++) {
                ReturnParticle(new Particle());
            }

            // Setup particle system cache.
            ParticleSystems = new QuickList<QuickList<ParticleSystem>>(particleCacheLists);
            for (int i = 0; i < particleCacheLists; i++) {
                ParticleSystems.Add(new QuickList<ParticleSystem>());
            }

            // Re-add existing particle systems.
            if (existing != null) {
                for (int i = 0; i < existing.Count; i++) {
                    existing.Array[i].InParticleEngine = false;
                    AddParticleSystem(existing.Array[i]);
                }
                GC.Collect();
            }
            PendingDisable = new QuickList<Particle>(2048);
        }

        public static void Update()
        {
            deltaTime += Time.DeltaTime;
            if (deltaTime < updateRate)
                return;

            DisablePending();
            FindVisibleParticleSystems();
            if (multithreaded) {
                if (SafeThreaded) {
                    Parallel.For(0, VisibleNum, parallelOptions, i => { VisibleParticleSystems.Array[i].UpdateThreaded(deltaTime); });
                } else {
                    updateTask = Task.Factory.StartNew(() => {
                        Parallel.For(0, VisibleNum, parallelOptions, i => { VisibleParticleSystems.Array[i].UpdateThreaded(deltaTime); });
                    });
                }
            } else {
                for (int i = 0; i < VisibleNum; i++) {
                    ParticleSystem ps = VisibleParticleSystems.Array[i];
                    ps.UpdateThreaded(deltaTime);
                }
            }
        }

        internal static Particle GetParticle()
        {
            if (ParticlePool.Count > 0) {
                Particle particle = ParticlePool.Array[ParticlePool.Count-1];
                ParticlePool.RemoveAt(ParticlePool.Count);
                particle.InPool = false;
                return particle;
            }
            return null;
        }

        internal static void ReturnParticle(Particle particle)
        {
            if(particle.InPool)
                return;

            particle.Reset();
            ParticlePool.Add(particle);
            RemoveParticle(particle);
            particle.InPool = true;
        }

        private static void DisablePending()
        {
            if (PendingDisable == null)
                return;

            for (int i = 0; i < PendingDisable.Count; i++) {
                Particle p = PendingDisable.Array[i];

                // TODO: Why are particles -sometimes- null for no reason!?
                if (p == null) 
                    continue;

                p.ParticleSystem.ReturnToPool(p);
            }
            PendingDisable.Clear();
        }

        private static void FindVisibleParticleSystems()
        {
            VisibleParticleSystems.Clear();

            int curPos = 0;
            for (int i = 0; i < ParticleSystems.Count; i++) {
                for (int ii = 0; ii < ParticleSystems.Array[i].Count; ii++) {
                    ParticleSystem ps = ParticleSystems.Array[i].Array[ii];
                    ps.UpdateVisibility();
                    if (ps.IsVisible) {
                        VisibleParticleSystems.Add(ps);
                        ps.InVisibleList = true;
                        curPos++;
                    } else {
                        if (ps.InVisibleList) {
                            ps.ClearParticles();
                        }
                        ps.InVisibleList = false;
                    }
                }
            }
            VisibleNum = curPos;
        }

        internal static void FinalizeThreads()
        {
            if (updateTask != null)
                Task.WaitAll(updateTask); 
            
            if (ParticleSystems == null || Math.Abs(deltaTime) < 0.00001f)
                return;

            if (deltaTime >= updateRate)
                deltaTime = 0f;
        }

        internal static void AddParticleSystem(ParticleSystem ps)
        {
            if (ps.InParticleEngine)
                return;

            ParticleSystems.Array[curList].Add(ps);
            ps.InParticleEngine = true;
            ps.MyList = curList;
            curList++;
            if (curList > ParticleSystems.Count-1) {
                curList = 0;
            }
        }

        internal static void RemoveParticleSystem(ParticleSystem ps)
        {
            for (int i = ps.ActiveParticles.Count - 1; i >= 0; i--) {
                Particle p = ps.ActiveParticles.Array[i];
                p.Enabled = false;
            }
            ParticleSystems.Array[ps.MyList].Remove(ps);
            ps.InParticleEngine = false;
        }

        internal static void AddParticle(Particle p)
        {
            if(p.InParticleEngine)
                return;

            switch (p.ParticleRenderType) {
                case ParticleRenderType.Alpha: {
                    if (AlphaParticles.TryGetValue(p.DrawCall, out ParticleRendererContainer particleList)) {
                        particleList.Add(p);
                    } else {
                        ParticleRendererContainer container = new ParticleRendererContainer(64);
                        container.Add(p);
                        AlphaParticles.Add(p.DrawCall, container);
                    }
                    break;
                }
                case ParticleRenderType.AlphaUnlit: {
                    if (AlphaParticlesUnlit.TryGetValue(p.DrawCall, out ParticleRendererContainer particleList)) {
                        particleList.Add(p);
                    } else {
                        ParticleRendererContainer container = new ParticleRendererContainer(64);
                        container.Add(p);
                        AlphaParticlesUnlit.Add(p.DrawCall, container);
                    }
                    break;
                }
                case ParticleRenderType.Additive: {
                    if (AdditiveParticles.TryGetValue(p.DrawCall, out ParticleRendererContainer particleList)) {
                        particleList.Add(p);
                    } else {
                        ParticleRendererContainer container = new ParticleRendererContainer(64);
                        container.Add(p);
                        AdditiveParticles.Add(p.DrawCall, container);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            p.InParticleEngine = true;
        }

        internal static void RemoveParticle(Particle p)
        {
            switch (p.ParticleRenderType) {
                case ParticleRenderType.Alpha: {
                    if (AlphaParticles.TryGetValue(p.DrawCall, out ParticleRendererContainer particles)) {
                        particles.Remove(p);
                    }
                    break;
                }
                case ParticleRenderType.AlphaUnlit: {
                    if (AlphaParticlesUnlit.TryGetValue(p.DrawCall, out ParticleRendererContainer particles)) {
                        particles.Remove(p);
                    }
                    break;
                }
                case ParticleRenderType.Additive: {
                    if (AdditiveParticles.TryGetValue(p.DrawCall, out ParticleRendererContainer particles)) {
                        particles.Remove(p);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            p.InParticleEngine = false;
        }
    }

}
