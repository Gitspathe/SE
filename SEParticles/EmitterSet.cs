using System;
using System.Collections.Generic;
using System.Text;
using SE.Utility;

namespace SEParticles
{
    public class EmitterSet
    {
        public QuickList<Emitter> Emitters { get; } = new QuickList<Emitter>();

        public void Update(float deltaTime)
        {
            Emitter[] emitterArray = Emitters.Array;
            for (int i = 0; i < emitterArray.Length; i++) {
                emitterArray[i].Update(deltaTime);
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

        public EmitterSet() { }

        public EmitterSet(Emitter emitter)
        {
            Emitters.Add(emitter);
        }

        public EmitterSet(Emitter[] emitters)
        {
            Emitters.AddRange(emitters);
        }

        public EmitterSet(IEnumerable<Emitter> emitters)
        {
            Emitters.AddRange(emitters);
        }

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
