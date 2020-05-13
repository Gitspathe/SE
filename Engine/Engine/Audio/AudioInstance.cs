using Microsoft.Xna.Framework.Audio;
using Vector2 = System.Numerics.Vector2;

namespace SE.Audio
{

    /// <summary>
    /// An instance created from an AudioClip. Used to play audio.
    /// </summary>
    public class AudioInstance
    {
        /// <summary>The SoundEffect this AudioInstance is playing.</summary>
        public SoundEffectInstance Sound;

        /// <summary>Audio will be heard at max volume at or below this distance.</summary>
        public float MinDistance;

        /// <summary>Audio will be inaudible at or beyond this distance.</summary>
        public float MaxDistance;

        /// <summary>How loud this AudioInstance is at minDistance.</summary>
        public float BaseVolume;

        /// <summary>Position of the AudioInstance in pixels.</summary>
        public Vector2 Position;

        /// <summary>Apply panning effects to this AudioClip.</summary>
        public bool ApplyPanning;

        /// <summary>
        /// AudioInstance constructor.
        /// </summary>
        /// <param name="sound">The SoundEffect this AudioInstance will play.</param>
        /// <param name="position">Position of the AudioInstance in pixels.</param>
        /// <param name="minDistance">Audio will be heard at max volume at or below this distance.</param>
        /// <param name="maxDistance">Audio will be inaudible at or beyond this distance.</param>
        /// <param name="baseVolume">How loud this AudioInstance is at minDistance.</param>
        /// <param name="applyPanning">Apply panning effects to this AudioClip.</param>
        /// <param name="loop">Repeats this AudioInstance continually.</param>
        public AudioInstance(SoundEffectInstance sound, Vector2 position, float minDistance, float maxDistance, float baseVolume, bool applyPanning = true, bool loop = false)
        {
            this.Sound = sound;
            this.Position = position;
            this.MinDistance = minDistance;
            this.MaxDistance = maxDistance;
            this.BaseVolume = baseVolume;
            this.ApplyPanning = applyPanning;
            sound.IsLooped = loop;
            Update();
            sound.Play();
        }

        /// <summary>
        /// Updates the AudioInstance.
        /// </summary>
        public void Update()
        {
            if (ApplyPanning) {
                Core.Audio.ApplySoundPanning(this);
            }
        }

    }

}
