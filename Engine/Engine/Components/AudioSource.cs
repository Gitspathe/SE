using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using SE.Audio;
using SE.Common;

namespace SE.Components
{

    /// <summary>
    /// Component which handles audio for GameObjects.
    /// </summary>
    public class AudioSource : Component
    {
        /// <inheritdoc />
        public override int Queue => 50;

        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private List<AudioInstance> audioInstances = new List<AudioInstance>();

        /// <inheritdoc />
        protected override void OnUpdate()
        {
            base.OnUpdate();
            for (int i = 0; i < audioInstances.Count; i++) {
                if (audioInstances[i].Sound.State == SoundState.Stopped) {
                    audioInstances.Remove(audioInstances[i]);
                } else {
                    audioInstances[i].Update();
                }
            }
        }

        /// <summary>
        /// Adds an AudioClip to the AudioSource.
        /// </summary>
        /// <param name="key">Name of the clip, used for lookups.</param>
        /// <param name="value">The AudioClip.</param>
        public void AddAudioClip(string key, AudioClip value)
        {
            audioClips.Add(key, value);
        }

        /// <summary>
        /// Plays an AudioClip.
        /// </summary>
        /// <param name="sound">Name of the clip to play.</param>
        public void Play(string sound)
        {
            if(audioClips.TryGetValue(sound, out AudioClip clip)) {
                audioInstances.Add(clip.CreateInstance(Owner.Transform.GlobalPositionInternal));
            }
        }

    }

}
