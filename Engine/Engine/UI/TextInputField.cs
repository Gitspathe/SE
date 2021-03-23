using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SE.AssetManagement;
using SE.Components.UI;
using SE.Core;
using SE.Input;
using SE.UI.Events;
using SE.Utility;
using Vector2 = System.Numerics.Vector2;

namespace SE.UI
{
    public class TextInputField : UIObject
    {
        public Action OnFocus;
        public Action OnUnfocus;
        public delegate void ConfirmedHandler(object sender, string value);
        public event ConfirmedHandler Confirmed;

        private string input;

        public bool ClearOnUnfocus { get; set; }
        public bool ConfirmOnEnter { get; set; } = true;
        public bool DefocusOnConfirm { get; set; } = true;

        public Alignment TextAlignment {
            get => Input.Align;
            set => Input.Align = value;
        }

        public Color TextColor {
            get => Input.SpriteColor;
            set => Input.SpriteColor = value;
        }

        public Color BackgroundColor {
            get => Background.SpriteColor;
            set => Background.SpriteColor = value;
        }

        public string Prompt {
            get => Input.Prefix;
            set => Input.Prefix = value;
        }

        public string Value {
            get => Input.Value;
            set => Input.Value = value;
        }

        public Point Size {
            get => Background.Size;
            set => Background.Size = value;
        }

        private bool active;
        public bool IsActive {
            get => active;
            set {
                active = value;
                if (active) {
                    UIManager.AddAssumedKeyboardControl(this);
                    OnFocus?.Invoke();
                } else {
                    UIManager.RemoveAssumedKeyboardControl(this);
                    if (ClearOnUnfocus) {
                        Input.Value = "";
                    }
                    OnUnfocus?.Invoke();
                }
            }
        }

        public Text Input { get; private set; }

        public Panel Background { get; private set; }

        protected sealed override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void OnEnable(bool isRoot = false)
        {
            base.OnEnable(isRoot);
        }

        protected override void OnDisable(bool isRoot = false)
        {
            base.OnDisable();
            IsActive = false;
        }

        public override void OnClick(MouseEventArgs mouseEventArgs)
        {
            base.OnClick(mouseEventArgs);
            if (mouseEventArgs.MouseButton == MouseButtons.Left) {
                IsActive = true;
            }
        }

        public override void OnClickAway(MouseEventArgs mouseEventArgs)
        {
            base.OnClickAway(mouseEventArgs);
            IsActive = false;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            input = Input.Value;
            if (!active)
                return;

            QuickList<Keys> pressedKeys = InputManager.PressedKeys;
            foreach (Keys key in pressedKeys) {
                if (key == Keys.Back && input.Length > 0) {
                    input = input.Remove(input.Length - 1, 1);
                } else if (key == Keys.Enter) {
                    if (ConfirmOnEnter) {
                        Confirm(this, input);
                    }
                }
            }
            foreach (char c in InputManager.PressedChars) {
                input += c;
            }
            Input.Value = input;
        }

        public void SetValue(string str)
        {
            input = str;
            Input.Value = str;
        }

        public void SetFont(Asset<SpriteFont> font, Color? color = null, Alignment? alignment = null)
        {
            if(font == null)
                return;

            if (Input != null) {
                Input.Font = font;
                Input.SpriteColor = color ?? Input.SpriteColor;
                Input.Align = alignment ?? Alignment.Left;
            } else {
                Input = new Text(font, Vector2.Zero) {
                    Parent = Background.Transform,
                    Align = alignment ?? Alignment.Left,
                    SpriteColor = color ?? Color.White
                };
            }
        }

        public void SetBackground(SlicedImage image, Point? size = null, int? borderSize = null, Color? color = null)
        {
            if(image == null)
                return;

            if (Background != null) {
                Background.SlicedSprite = new UISlicedSprite(size ?? Size, color ?? Color.White, image, borderSize ?? 4);
            } else {
                Background = new Panel(Vector2.Zero, size ?? Size, image, borderSize ?? 4) {
                    Parent = Transform,
                    SpriteColor = color ?? Color.White
                };
            }
        }

        public TextInputField(Vector2 pos, Point size) : base(pos, size)
        {
            SetBackground(UIManager.DefaultSlicedImage, size);
            SetFont(UIManager.DefaultFont, Color.White);
            Interactable = true;
            Bounds = new RectangleF(Transform.GlobalPositionInternal.X, Transform.GlobalPositionInternal.Y, size.X, size.Y);
        }

        private void Confirm(object sender, string value)
        {
            Confirmed?.Invoke(sender, value);
            input = "";
            Input.Value = input;
            if (DefocusOnConfirm) {
                IsActive = false;
            }
        }
    }
}
