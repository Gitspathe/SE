using DeeZ.Engine.Utility;

namespace SE.Particles
{

    /// <summary>
    /// Optimized container for particle render data. Should not be created or interacted with manually. The rendering system uses
    /// these containers to minimize the amount of draw-calls dispatched.
    /// </summary>
    public class ParticleRendererContainer
    {
        public QuickList<Particle>[] ParticlesRenderData;
        public int Length;
        public int CurList;

        public void Add(Particle p)
        {
            if (p.InParticleEngine)
                return;

            ParticlesRenderData[CurList].Add(p);
            p.CurList = CurList;
            CurList++;
            if (CurList > ParticlesRenderData.Length - 1) {
                CurList = 0;
            }
        }

        public void Remove(Particle p)
        {
            ParticlesRenderData[p.CurList].Remove(p);
        }

        public ParticleRendererContainer(int lists)
        {
            Length = lists;
            ParticlesRenderData = new QuickList<Particle>[lists];
            for (int i = 0; i < lists; i++) {
                ParticlesRenderData[i] = new QuickList<Particle>();
            }
        }
    }

}
