namespace SE.Serialization
{
    public class SerializerSettings
    {
        public NullValueHandling NullValueHandling { get; set; }
    }

    public enum NullValueHandling
    {
        Ignore,
        DefaultValue
    }
}
