using Microsoft.Xna.Framework;
using SE.Audio;
using Vector2 = System.Numerics.Vector2;

namespace SE.Core
{

    /// <summary>
    /// Static class which controls all audio of the game.
    /// </summary>
    public static class Audio
    {

        /// <summary>
        /// Applies panning and volume reduction to an AudioInstance based on the audio's position relative to the camera.
        /// </summary>
        /// <param name="audioInstance">AudioInstance to apply panning.</param>
        public static void ApplySoundPanning(AudioInstance audioInstance)
        {
            Vector2 distance = audioInstance.Position; //- Screen.UnscaledViewCenter;
            float fade;
            if (distance.Length() < audioInstance.MinDistance) {
                fade = 1f;
            } else {
                fade = MathHelper.Clamp(1f - distance.Length() / audioInstance.MaxDistance, 0, 1);
            }
            audioInstance.Sound.Volume = fade * fade * audioInstance.BaseVolume;
            audioInstance.Sound.Pan = MathHelper.Clamp(distance.X / audioInstance.MaxDistance, -1, 1);
        }

    }

}
