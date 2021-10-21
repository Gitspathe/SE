namespace SE.Animating
{
    public interface IAnimatedValue
    {
        void Update(float time);

        void Reset();

        float Duration { get; }

        float Location { get; }
    }
}