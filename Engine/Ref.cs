using System;

namespace SE
{
    public class Ref<T>
    {
        private readonly Action<T> setter;

        public Ref(Action<T> setter)
        {
            this.setter = setter;
        }

        public T Value {
            set => setter(value);
        }
    }
}
