using Microsoft.Xna.Framework.Audio;
using Vector2 = System.Numerics.Vector2;

namespace SE.Audio
{

    /// <summary>
    /// Container for an AudioClip.
    /// </summary>
    public class AudioClip
    {
        /// <summary>SoundEffect the AudioClip uses.</summary>
        public SoundEffect SoundEffect;

        /// <summary>Audio will be heard at max volume at or below this distance.</summary>
        public float MinDistance;

        /// <summary>Audio will be inaudible at or beyond this distance.</summary>
        public float MaxDistance;

        /// <summary>How loud this AudioClip is at minDistance.</summary>
        public float BaseVolume;

        /// <summary>Apply panning effects to this AudioClip.</summary>
        public bool ApplyPanning;

        /// <summary>Repeats this AudioClip continually.</summary>
        public bool Loop;

        /// <summary>
        /// Creates an instance of this AudioClip to play sound.
        /// </summary>
        /// <param name="position">Position of the AudioClip in pixels.</param>
        /// <returns>An AudioInstance of this AudioClip.</returns>
        public AudioInstance CreateInstance(Vector2 position)
        {
            return new AudioInstance(SoundEffect.CreateInstance(), position, MinDistance, MaxDistance, BaseVolume, ApplyPanning, Loop);
        }

        /// <summary>
        /// AudioClip constructor.
        /// </summary>
        /// <param name="soundEffect">SoundEffect the AudioClip uses.</param>
        /// <param name="minDistance">Audio will be heard at max volume at or below this distance.</param>
        /// <param name="maxDistance">Audio will be inaudible at or beyond this distance.</param>
        /// <param name="baseVolume">How loud this AudioClip is at minDistance.</param>
        /// <param name="applyPanning">Apply panning effects to this AudioClip</param>
        /// <param name="loop">Repeats this AudioClip continually.</param>
        public AudioClip(SoundEffect soundEffect, float minDistance, float maxDistance, float baseVolume, bool applyPanning = true, bool loop = false)
        {
            this.SoundEffect = soundEffect;
            this.MinDistance = minDistance;
            this.MaxDistance = maxDistance;
            this.BaseVolume = baseVolume;
            this.ApplyPanning = applyPanning;
            this.Loop = loop;
        }

    }

}
