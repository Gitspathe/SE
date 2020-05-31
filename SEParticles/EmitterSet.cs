using System;
using System.Collections.Generic;
using System.Text;
using SE.Utility;

namespace SEParticles
{
    public class EmitterSet
    {
        private QuickList<Emitter> Emitters { get; } = new QuickList<Emitter>();

        public EmitterSet() { }

        public EmitterSet(Emitter emitter) 
            => Emitters.Add(emitter);

        public EmitterSet(Emitter[] emitters) 
            => Emitters.AddRange(emitters);

        public EmitterSet(IEnumerable<Emitter> emitters) 
            => Emitters.AddRange(emitters);

        public bool Enabled {
            set {
                for (int i = 0; i < Emitters.Count; i++)
                    Emitters.Array[i].Enabled = value;
            }
        }

        public void Emit(int amount = 1)
        {
            Emitter[] emitterArray = Emitters.Array;
            for (int i = 0; i < Emitters.Count; i++) {
                emitterArray[i].Emit(amount);
            }
        }

        public void Add(Emitter emitter) 
            => Emitters.Add(emitter);

        public void Remove(Emitter emitter) 
            => Emitters.Remove(emitter);

        public void Clear() 
            => Emitters.Clear();

        public EmitterSet DeepCopy()
        {
            EmitterSet set = new EmitterSet();
            for (int i = 0; i < Emitters.Count; i++) {
                set.Emitters.Add(Emitters.Array[i].DeepCopy());
            }
            return set;
        }
    }
}
