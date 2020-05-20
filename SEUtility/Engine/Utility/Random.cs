namespace SE.Utility
{
    public static class Random
    {
        private static System.Random random = new System.Random();

        public static float Next(float max)
        {
            return (float)(random.NextDouble() * max);
        }

        public static int Next(int max)
        {
            return random.Next(max);
        }

        public static float Next(float min, float max)
        {
            return (float)(min + (random.NextDouble() * max));
        }

        public static int Next(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}
