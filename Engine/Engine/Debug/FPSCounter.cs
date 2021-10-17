using SE.Core;
using System;
using System.Collections.Generic;

namespace SE.Debug
{

    /// <summary>
    /// Calculates the current and average framerate.
    /// </summary>
    public class FPSCounter
    {
        /// <summary>Current fps, calculated from the current frame.</summary>
        public int CurrentFPS;

        /// <summary>The average frame-rate, calculated by averaging 100 frames.</summary>
        public int AverageFPS;
        private List<int> previousFPS = new List<int>();

        /// <summary>Update loop.</summary>
        public void Update()
        {
            int cur;
            if (!double.IsInfinity(Math.Round((1.0f / Time.UnscaledDeltaTime)))) {
                CurrentFPS = (int)Math.Round(1.0f / Time.UnscaledDeltaTime);
                previousFPS.Add(CurrentFPS);
            }
            if (previousFPS.Count > 0) {
                if (previousFPS.Count > 100) {
                    previousFPS.RemoveAt(0);
                }
                int curSum = 0;
                for (int i = 0; i < previousFPS.Count; i++) {
                    cur = previousFPS[i];
                    curSum += cur;
                }
                AverageFPS = (curSum / previousFPS.Count);
            }
        }

    }

}
