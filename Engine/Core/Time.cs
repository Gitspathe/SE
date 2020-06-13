using Microsoft.Xna.Framework;

namespace SE.Core
{

    /// <summary>
    /// Static class which handles time.
    /// </summary>
    public static class Time
    {
        /// <summary>Maximum allowed fixed time step.</summary>
        public static float MaxFixedTimestep { get; set; } = 0.1f;

        /// <summary>Fixed amount of time passed for each FixedUpdate iteration.
        ///          Should be used in place of DeltaTime inside of FixedUpdate methods.</summary>
        public static float FixedTimestep { get; set; } = 1.0f / 60.0f;
        
        internal static int FixedTimeStepIterations { get; private set; }
        private static float fixedTimer;

        /// <summary>Amount of time passed in seconds since the last update, scaled with timeScale.</summary>
        public static float DeltaTime { get; internal set; }

        /// <summary>Real amount of time passed in seconds since the last update, not scaled.</summary>
        public static float UnscaledDeltaTime { get; internal set; }

        /// <summary>Current time scale, used to speed up or slow down the game.</summary>
        public static float TimeScale = 1.0f;

        public static GameTime GameTime;

        internal static void Update(GameTime gTime)
        {
            GameTime = gTime;
            float timeSec = (float) gTime.ElapsedGameTime.TotalSeconds;
            UnscaledDeltaTime = timeSec;
            DeltaTime = timeSec * TimeScale;

            fixedTimer += UnscaledDeltaTime;
            if(fixedTimer > MaxFixedTimestep)
                fixedTimer = MaxFixedTimestep;

            while (fixedTimer > FixedTimestep) {
                FixedTimeStepIterations++;
                fixedTimer -= FixedTimestep;
            }
        }

        internal static void FinalizeFixedTimeStep()
        {
            FixedTimeStepIterations = 0;
        }

        /// <summary>
        /// Get partially scaled time, useful for making some logic less dependent on timeScale.
        /// </summary>
        /// <param name="multiplier">Multiplier used.</param>
        /// <returns>Partially scaled DeltaTime.</returns>
        public static float GetPartiallyScaledTime(float multiplier)
        {
            return UnscaledDeltaTime * (TimeScale * multiplier);
        }

    }

}
