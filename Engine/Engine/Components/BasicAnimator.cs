using SE.Animating;
using SE.Common;
using SE.Core;
using System;
using System.Collections.Generic;

namespace SE.Components
{
    public class BasicAnimator : Component
    {
        public override int Queue => 50;

        public bool PlaySolo = true;
        public bool Paused = false;
        public AnimationTimeScale TimeScale = AnimationTimeScale.DeltaTime;

        private Dictionary<string, AnimationState> animations = new Dictionary<string, AnimationState>();

        public int Count => animations.Count;

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (Paused)
                return;

            float time;
            switch (TimeScale) {
                case AnimationTimeScale.DeltaTime:
                    time = Time.DeltaTime;
                    break;
                case AnimationTimeScale.UnscaledDeltaTime:
                    time = Time.UnscaledDeltaTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (KeyValuePair<string, AnimationState> anim in animations) {
                if (anim.Value.IsPlaying) {
                    anim.Value.Animation.Update(time);
                }
            }
        }

        public void AddAnimation(string key, Animation val)
        {
            if (Contains(val))
                return;

            animations.Add(key, new AnimationState(val));
        }

        public void AddAnimation(KeyValuePair<string, Animation> pair)
        {
            if (Contains(pair.Key))
                return;

            animations.Add(pair.Key, new AnimationState(pair.Value));
        }

        public bool RemoveAnimation(string key)
        {
            return animations.Remove(key);
        }

        public bool RemoveAnimation(Animation animationVal)
        {
            foreach (KeyValuePair<string, AnimationState> anim in animations) {
                if (anim.Value.Animation == animationVal) {
                    return animations.Remove(anim.Key);
                }
            }
            return false;
        }

        public void RemoveAnimations(params Animation[] animationArray)
        {
            for (int i = 0; i < animationArray.Length; i++)
                RemoveAnimation(animationArray[i]);
        }

        public bool Contains(Animation animationVal)
        {
            foreach (KeyValuePair<string, AnimationState> anim in animations) {
                if (anim.Value.Animation == animationVal) {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string animationKey)
        {
            return animations.ContainsKey(animationKey);
        }

        public bool IsPlaying(string key)
        {
            if (!Contains(key))
                return false;

            return animations[key].IsPlaying;
        }

        public bool IsPlaying(Animation val)
        {
            if (!Contains(val))
                return false;

            foreach (KeyValuePair<string, AnimationState> anim in animations) {
                if (anim.Value.Animation == val) {
                    return anim.Value.IsPlaying;
                }
            }
            return false;
        }

        public void Clear()
        {
            animations.Clear();
        }

        public void Play(string key)
        {
            if (!animations.ContainsKey(key))
                return;

            if (PlaySolo) {
                StopAll();
            }
            animations[key].IsPlaying = true;
        }

        public void Stop(string key)
        {
            if (!animations.ContainsKey(key))
                return;

            animations[key].IsPlaying = false;
            animations[key].Animation.Reset(); // Reset time to zero.
        }

        public void StopAll()
        {
            foreach (KeyValuePair<string, AnimationState> anim in animations) {
                anim.Value.IsPlaying = false;
                anim.Value.Animation.Location = 0f;
            }
        }

        public void PlayAll()
        {
            foreach (KeyValuePair<string, AnimationState> anim in animations) {
                anim.Value.IsPlaying = true;
            }
        }

        public BasicAnimator() { }

        public BasicAnimator(params KeyValuePair<string, Animation>[] animations)
        {
            for (int i = 0; i < animations.Length; i++) {
                AddAnimation(animations[i]);
            }
        }

        public enum AnimationTimeScale
        {
            DeltaTime,
            UnscaledDeltaTime
        }

        private class AnimationState
        {
            public Animation Animation;
            public bool IsPlaying;

            public AnimationState(Animation anim)
            {
                Animation = anim;
                IsPlaying = false;
            }
        }
    }

}
