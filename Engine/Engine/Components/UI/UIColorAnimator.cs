using Microsoft.Xna.Framework;
using SE.Animating;
using SE.UI;

namespace SE.Components.UI
{
    /// <summary>
    /// Handles animations for a single UIObject.
    /// </summary>
    public class UIColorAnimator : UIAnimator
    {
        private UIObject obj;
        protected AnimatedColor IdleColor, HighlightedColor, ClickedColor, ToggledColor;

        /// <inheritdoc />
        public override void OnSelected()
        {
            base.OnSelected();
            HighlightedColor.SetLerp(obj.SpriteColor, HighlightedColor.To, HighlightedColor.Duration);
        }

        /// <inheritdoc />
        public override void OnDeselected()
        {
            base.OnDeselected();
            IdleColor.SetLerp(obj.SpriteColor, IdleColor.To, IdleColor.Duration);
        }

        /// <inheritdoc />
        public override void OnClick()
        {
            base.OnClick();
            ClickedColor.SetLerp(obj.SpriteColor, ClickedColor.To, ClickedColor.Duration);
        }

        public override void OnClickRelease()
        {
            base.OnClickRelease();
            HighlightedColor.SetLerp(obj.SpriteColor, HighlightedColor.To, HighlightedColor.Duration);
        }

        public override void Toggle()
        {
            base.Toggle();
            ToggledColor.SetLerp(obj.SpriteColor, ToggledColor.To, ToggledColor.Duration);
        }

        public override void Untoggle()
        {
            base.Untoggle();
            IdleColor.SetLerp(obj.SpriteColor, IdleColor.To, IdleColor.Duration);
        }

        /// <summary>
        /// Configures the UIAnimation component by adding some generic animations: idle, highlight, click.
        /// </summary>
        /// <param name="idle">Idle animation state.</param>
        /// <param name="highlighted">Highlight animation state.</param>
        /// <param name="clicked">Click animation state.</param>
        /// <param name="toggled">Toggled animation state.</param>
        /// <param name="transition">Transition time in seconds between animation states.</param>
        public void Configure(Color idle, Color highlighted, Color clicked, Color? toggled, float transition)
        {
            IdleColor = new AnimatedColor();
            IdleColor.SetLerp(idle, idle, transition);
            IdleColor.Attach(new Ref<Color>(i => obj.SpriteColor = i));

            HighlightedColor = new AnimatedColor();
            HighlightedColor.SetLerp(idle, highlighted, transition);
            HighlightedColor.Attach(new Ref<Color>(i => obj.SpriteColor = i));

            ClickedColor = new AnimatedColor();
            ClickedColor.SetLerp(highlighted, clicked, transition);
            ClickedColor.Attach(new Ref<Color>(i => obj.SpriteColor = i));

            Animation idleAnimation = new Animation(IdleColor) {
                WrapMode = WrapMode.Once
            };
            Animation highlightedAnimation = new Animation(HighlightedColor) {
                WrapMode = WrapMode.Once
            };
            Animation clickedAnimation = new Animation(ClickedColor) {
                WrapMode = WrapMode.Once
            };
            Animation toggledAnimation = null;
            if (toggled.HasValue) {
                Color col = toggled.Value;
                ToggledColor = new AnimatedColor();
                ToggledColor.SetLerp(highlighted, col, transition);
                ToggledColor.Attach(new Ref<Color>(i => obj.SpriteColor = i));
                toggledAnimation = new Animation(ToggledColor) {
                    WrapMode = WrapMode.Once
                };
            }

            Configure(idleAnimation, highlightedAnimation, clickedAnimation, toggledAnimation);
        }

        public UIColorAnimator(UIObject uiObj)
        {
            obj = uiObj;
            IdleColor = new AnimatedColor();
            HighlightedColor = new AnimatedColor();
            ClickedColor = new AnimatedColor();
            ToggledColor = new AnimatedColor();
        }
    }
}
