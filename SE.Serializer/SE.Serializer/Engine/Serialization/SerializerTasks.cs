using SE.Serialization.Exceptions;

namespace SE.Serialization
{
    // TODO: Reference loop handling.
    public ref struct SerializeTask
    {
        public int CurrentDepth;
        public SerializerSettings Settings;

        public SerializeTask(SerializerSettings settings)
        {
            CurrentDepth = 0;
            Settings = settings;
        }

        public SerializeTask Clone(int incrementDepth = 1)
        {
            SerializeTask copy = new SerializeTask {
                CurrentDepth = CurrentDepth + incrementDepth,
                Settings = Settings
            };
            return copy;
        }
    }

    public ref struct DeserializeTask
    {
        public SerializerSettings Settings;

        public DeserializeTask(SerializerSettings settings)
        {
            Settings = settings;
        }
    }
}
