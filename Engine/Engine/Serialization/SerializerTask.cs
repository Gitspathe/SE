namespace SE.Serialization
{
    // TODO: Reference loop handling.
    public ref struct SerializerTask
    {
        public int CurrentDepth;
        public SerializerSettings Settings;

        public SerializerTask Clone(int incrementDepth = 1)
        {
            SerializerTask copy = new SerializerTask {
                CurrentDepth = CurrentDepth + incrementDepth,
                Settings = Settings
            };
            return copy;
        }

        public SerializerTask(SerializerSettings settings)
        {
            CurrentDepth = 0;
            Settings = settings;
        }
    }
}
