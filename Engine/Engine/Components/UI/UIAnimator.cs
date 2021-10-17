using SE.Animating;

namespace SE.Components.UI
{
    /// <summary>
    /// Handles animations for a single UIObject.
    /// </summary>
    public class UIAnimator : UIComponent
    {
        public BasicAnimator AnimatorComponent;

        protected bool Configured;
        protected Animation Idle, Highlighted, Clicked, Toggled;

        /// <inheritdoc />
        public override void OnSelected()
        {
            base.OnSelected();
            if (!Configured || AnimatorComponent.IsPlaying("toggle"))
                return;

            AnimatorComponent.Play("highlight");
        }

        /// <inheritdoc />
        public override void OnDeselected()
        {
            base.OnDeselected();
            if (!Configured || AnimatorComponent.IsPlaying("toggle"))
                return;

            AnimatorComponent.Play("idle");
        }

        /// <inheritdoc />
        public override void OnClick()
        {
            base.OnClick();
            if (!Configured || AnimatorComponent.IsPlaying("toggle"))
                return;

            AnimatorComponent.Play("click");
        }

        public override void OnClickRelease()
        {
            base.OnClickRelease();
            if (!Configured || AnimatorComponent.IsPlaying("toggle"))
                return;

            AnimatorComponent.Play("highlight");
        }

        public virtual void Toggle()
        {
            if (!Configured || Toggled == null)
                return;

            AnimatorComponent.Play("toggle");
        }

        public virtual void Untoggle()
        {
            if (!Configured)
                return;

            AnimatorComponent.Play("idle");
        }

        /// <summary>
        /// Configures the UIAnimation component by adding some generic animations: idle, highlight, click.
        /// </summary>
        /// <param name="idle">Idle animation state.</param>
        /// <param name="highlighted">Highlight animation state.</param>
        /// <param name="clicked">Click animation state.</param>
        /// <param name="toggled">Toggled animation state.</param>
        public void Configure(Animation idle, Animation highlighted, Animation clicked, Animation toggled)
        {
            Idle = idle;
            Highlighted = highlighted;
            Clicked = clicked;

            AnimatorComponent = new BasicAnimator();
            AnimatorComponent.AddAnimation("idle", Idle);
            AnimatorComponent.AddAnimation("highlight", Highlighted);
            AnimatorComponent.AddAnimation("click", Clicked);
            if (toggled != null) {
                Toggled = toggled;
                AnimatorComponent.AddAnimation("toggle", Toggled);
            }
            Owner.AddComponent(AnimatorComponent);

            AnimatorComponent.Play("idle");
            Configured = true;
        }
    }

}
