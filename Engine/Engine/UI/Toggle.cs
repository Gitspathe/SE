using Microsoft.Xna.Framework;
using SE.Components.UI;
using SE.Core;
using SE.Rendering;
using SE.UI.Events;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI
{
    public class Toggle : UIObject
    {
        public delegate void ValueChangedHandler(object sender, bool value);
        public event ValueChangedHandler ValueChanged;

        public bool IsToggled { get; private set; } = false;

        public Image ToggleImage { get; private set; }

        public UISprite Background { get; private set; }

        public Color ToggleColor {
            get => ToggleImage.SpriteColor;
            set => ToggleImage.SpriteColor = value;
        }

        public Color BackgroundColor {
            get => Background.Color;
            set => Background.Color = value;
        }

        /// <inheritdoc />
        protected sealed override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        protected override void OnEnable(bool isRoot = false)
        {
            base.OnEnable();
            if (IsToggled) {
                ToggleImage.Enable(isRoot);
            } else {
                ToggleImage.Disable();
            }
        }

        public override void OnSelected(MouseEventArgs mouseEventArgs = null)
        {
            base.OnSelected(mouseEventArgs);
        }

        /// <inheritdoc />
        public override void OnDeselected(MouseEventArgs mouseEventArgs = null)
        {
            base.OnDeselected(mouseEventArgs);
        }

        /// <inheritdoc />
        public override void OnClick(MouseEventArgs mouseEventArgs = null)
        {
            base.OnClick(mouseEventArgs);
            if (InputManager.LeftMouseClicked) {
                if (IsToggled) {
                    IsToggled = false;
                    ToggleImage.Disable();
                }
                else {
                    IsToggled = true;
                    ToggleImage.Enable();
                }
                ValueChanged?.Invoke(this, IsToggled);
            }
        }

        public Toggle(Vector2 pos, Point size, SpriteTexture untoggled, SpriteTexture toggled) : base(pos, size)
        {
            Background = new UISprite(new Point(size.X, size.Y), Color.White, untoggled);
            ToggleImage = new Image(Vector2.Zero, size, toggled) {
                SpriteColor = Color.Red,
                Parent = Transform
            };
            //AddComponent(new MouseOverChangeColor(toggledColor)); // TODO: Change this to the new UIAnimation component.
            AddComponent(Background);
            ToggleImage.Disable();
            Interactable = true;
            Bounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y, size.X, size.Y);
        }
    }
}
