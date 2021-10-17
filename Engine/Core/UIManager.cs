using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SE.AssetManagement;
using SE.Common;
using SE.Components.UI;
using SE.Editor.UI;
using SE.Input;
using SE.UI;
using SE.UI.Events;
using SE.Utility;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;

namespace SE.Core
{
    public static class UIManager
    {
        public static Asset<SpriteFont> DefaultFont;
        public static SlicedImage DefaultSlicedImage;

        public static UIInputReceiver RootInputFocus { get; set; }
        public static UIObject AssumedScrollWheelControl;
        public static List<UIObject> OrderedTopLevelMenus = new List<UIObject>();

        private static Dictionary<string, UIObject> menus = new Dictionary<string, UIObject>();
        private static QuickList<UIObject> gizmos = new QuickList<UIObject>();
        private static QuickList<UIObject> assumedKeyboardControl = new QuickList<UIObject>();
        private static bool listenButtonInput;
        private static QuickList<string> uiInputButtons = new QuickList<string>
            {"UIUp", "UILeft", "UIDown", "UIRight", "UIConfirm", "UIBack"};

        private static AssetConsumerContext assetConsumerContext = new AssetConsumerContext();

        // Caching...
        private static UIObject mouseBlocker;
        private static QuickList<Transform> menuTransforms = new QuickList<Transform>();
        private static List<UIObject> menuUIObjects = new List<UIObject>();
        private static List<UIObject> tmpUIObjects = new List<UIObject>();
        private static UIObject uiObject;

        // Cache mouse event arguments.
        private static MouseEventArgs leftEventArgs = new MouseEventArgs(MouseButtons.Left, null);
        private static MouseEventArgs rightEventArgs = new MouseEventArgs(MouseButtons.Right, null);
        private static MouseEventArgs noneEventArgs = new MouseEventArgs(MouseButtons.None, null);

        public static bool MouseBlocked => mouseBlocker != null;

        private static UIInputReceiver inputFocus;
        public static UIInputReceiver InputFocus {
            get => inputFocus;
            set {
                if (value != inputFocus) {
                    inputFocus?.Owner.OnDeselected(new MouseEventArgs(MouseButtons.Left, Screen.MousePoint));
                    value?.Owner.OnSelected(new MouseEventArgs(MouseButtons.Left, Screen.MousePoint));
                }
                inputFocus = value;
            }
        }

        private static ButtonInputSet uiUpInput;
        public static ButtonInputSet UIUpInput {
            get => uiUpInput;
            set {
                if (value != null && uiUpInput != null && uiUpInput.Key != value.Key) {
                    InputManager.RemoveButtonInput(uiUpInput.Key);
                }
                uiUpInput = value;
            }
        }

        private static ButtonInputSet uiRightInput;
        public static ButtonInputSet UIRightInput {
            get => uiRightInput;
            set {
                if (value != null && uiRightInput != null && uiRightInput.Key != value.Key) {
                    InputManager.RemoveButtonInput(uiRightInput.Key);
                }
                uiRightInput = value;
            }
        }

        private static ButtonInputSet uiDownInput;
        public static ButtonInputSet UIDownInput {
            get => uiDownInput;
            set {
                if (value != null && uiDownInput != null && uiDownInput.Key != value.Key) {
                    InputManager.RemoveButtonInput(uiDownInput.Key);
                }
                uiDownInput = value;
            }
        }

        private static ButtonInputSet uiLeftInput;
        public static ButtonInputSet UILeftInput {
            get => uiLeftInput;
            set {
                if (value != null && uiLeftInput != null && uiLeftInput.Key != value.Key) {
                    InputManager.RemoveButtonInput(uiLeftInput.Key);
                }
                uiLeftInput = value;
            }
        }

        private static ButtonInputSet uiConfirmInput;
        public static ButtonInputSet UIConfirmInput {
            get => uiConfirmInput;
            set {
                if (value != null && uiConfirmInput != null && uiConfirmInput.Key != value.Key) {
                    InputManager.RemoveButtonInput(uiConfirmInput.Key);
                }
                uiConfirmInput = value;
            }
        }

        private static ButtonInputSet uiBackInput;
        public static ButtonInputSet UIBackInput {
            get => uiBackInput;
            set {
                if (value != null && uiBackInput != null && uiBackInput.Key != value.Key) {
                    InputManager.RemoveButtonInput(uiBackInput.Key);
                }
                uiBackInput = value;
            }
        }

        public static void RegisterMenu(string key, UIObject rootUIObject, int priority)
        {
            if (Screen.DisplayMode != DisplayMode.Normal && key != "Console")
                return;
            if (menus.ContainsKey(key))
                return;

            rootUIObject.SetPriority(priority);
            menus.Add(key, rootUIObject);
            ReorderMenus();
        }

        public static void UnregisterMenu(string key)
        {
            menus.TryGetValue(key, out UIObject uiObj);
            if (uiObj != null) {
                menus.Remove(key);
            }
            ReorderMenus();
        }

        public static void EnableMenu(string key)
        {
            menus.TryGetValue(key, out UIObject menu);
            menu?.Enable();
        }

        public static void DisableMenu(string key)
        {
            menus.TryGetValue(key, out UIObject menu);
            menu?.Disable();
        }

        public static void Initialize()
        {
            DefaultFont = AssetManager.GetAsset<SpriteFont>("editor");
            DefaultSlicedImage = EditorTheme.PanelFullTransparent;

            Controller controller = InputManager.GetController(Players.One);

            UIUpInput = controller.AddButtonInput("UIUp",
                ButtonInput.FromKeyboard(Keys.Up),
                ButtonInput.FromGamepad(GamepadButtons.DPadUp));

            UIRightInput = controller.AddButtonInput("UIRight",
                ButtonInput.FromKeyboard(Keys.Right),
                ButtonInput.FromGamepad(GamepadButtons.DPadRight));

            UIDownInput = controller.AddButtonInput("UIDown",
                ButtonInput.FromKeyboard(Keys.Down),
                ButtonInput.FromGamepad(GamepadButtons.DPadDown));

            UILeftInput = controller.AddButtonInput("UILeft",
                ButtonInput.FromKeyboard(Keys.Left),
                ButtonInput.FromGamepad(GamepadButtons.DPadLeft));

            UIConfirmInput = controller.AddButtonInput("UIConfirm",
                ButtonInput.FromKeyboard(Keys.Enter),
                ButtonInput.FromGamepad(GamepadButtons.A));

            UIBackInput = controller.AddButtonInput("UIBack",
                ButtonInput.FromKeyboard(Keys.Escape),
                ButtonInput.FromGamepad(GamepadButtons.B));
        }

        public static void Update()
        {
            UpdateUIObjects(gizmos, Screen.MousePoint);
            UpdateUIObjects(menus.Values, Screen.MousePoint);
            UpdateUIInput();
        }

        private static void UpdateUIObjects(IEnumerable<UIObject> objectList, Vector2? mousePoint)
        {
            // Update cached mouse event arguments. And reset mouse blocker.
            leftEventArgs.MousePoint = mousePoint;
            rightEventArgs.MousePoint = mousePoint;
            noneEventArgs.MousePoint = mousePoint;
            mouseBlocker = null;

            // Order top level UI menus by priority, and process them in order.
            tmpUIObjects.Clear();
            tmpUIObjects.AddRange(objectList);
            tmpUIObjects.Sort((x, y) => y.Priority.CompareTo(x.Priority));

            // Process each of the top level UIObjects.
            for (int m = 0; m < tmpUIObjects.Count; m++) {
                UIObject menu = tmpUIObjects[m];
                if (!menu.Enabled)
                    continue;

                // Clear lists and prepare to process all menu UIObjects.
                menuUIObjects.Clear();
                menuTransforms.Clear();
                menuTransforms.Add(menu.Transform);
                menu.Transform.GetAllChildrenNonAlloc(menuTransforms);
                for (int i = 0; i < menuTransforms.Count; i++) {
                    GameObject menuGO = menuTransforms.Array[i].GameObject;
                    if (menuGO.Enabled && menuGO is UIObject uiObj) {
                        menuUIObjects.Add(uiObj);
                    }
                }

                // Sort UIObjects by priority, and process them in order. Higher priority UIObjects can block
                // events such as MouseOver() on overlapping UIObjects.
                menuUIObjects.Sort((x, y) => y.Priority.CompareTo(x.Priority));

                // Update each UIObject.
                for (int i = 0; i < menuUIObjects.Count; i++) {
                    UpdateUIObject(menuUIObjects[i], mousePoint);
                }
            }
        }

        private static void UpdateUIObject(UIObject uiObject, Vector2? mousePoint)
        {
            if (!uiObject.IsInteractable || uiObject.Destroyed)
                return;

            // Check if the mouse is within the scissorRect containing the current UIObject.
            bool inScissorRect = true;
            Point mousePnt = mousePoint.HasValue
                ? new Point((int)mousePoint.Value.X, (int)mousePoint.Value.Y)
                : new Point(-9999, -9999);

            if (uiObject.ParentScissorRect.HasValue) {
                Rectangle rectTest = uiObject.ParentScissorRect.Value;
                if (!rectTest.Contains(mousePnt)) {
                    inScissorRect = false;
                }
            }

            // Don't update mouse input if the UI system is listening to the keyboard or a controller.
            if (listenButtonInput)
                return;

            // If the mouse is within the scissorRect, and over the UIObject, handle specific mouse over/click events.
            if (inScissorRect && uiObject.Bounds.Contains(mousePnt)) {
                if (IsMouseBlocker(uiObject)) {
                    uiObject.OnSelected(noneEventArgs);
                    if (InputManager.LeftMouseClicked) {
                        uiObject.OnClick(leftEventArgs);
                    }
                    if (InputManager.RightMouseClicked) {
                        uiObject.OnClick(rightEventArgs);
                    }
                    if (InputManager.LeftMouseReleased) {
                        uiObject.OnClickRelease(leftEventArgs);
                    }
                    if (InputManager.RightMouseReleased) {
                        uiObject.OnClickRelease(rightEventArgs);
                    }
                    if (uiObject.BlocksSelection) {
                        mouseBlocker = uiObject;
                    }
                } else {
                    // Prevents highlighting of overlapping UIObjects.
                    uiObject.OnDeselected(noneEventArgs);
                }

                // Otherwise, if the mouse isn't over the UIObject, handle events such as MouseLeave().
            } else {
                uiObject.OnDeselected(noneEventArgs);
                if (InputManager.LeftMouseClicked) {
                    uiObject.OnClickAway(leftEventArgs);
                }
                if (InputManager.RightMouseClicked) {
                    uiObject.OnClickAway(rightEventArgs);
                }
            }
        }

        private static void UpdateUIInput()
        {
            if (!listenButtonInput) {
                if (InputManager.ButtonPressed(Players.One, Filter.Any, uiInputButtons)) {
                    listenButtonInput = true;
                    if (RootInputFocus != null) {
                        InputFocus = RootInputFocus;
                    }
                }
            } else if (InputManager.MouseMoved) {
                listenButtonInput = false;
                InputFocus = RootInputFocus;
            }

            // Don't update the controller / keyboard input if the mouse is being used.
            if (!listenButtonInput || InputFocus == null)
                return;

            // Handle controller + keyboard input.
            if (UIUpInput != null && InputManager.ButtonPressed(Players.One, UIUpInput.Key)) {
                if (InputFocus.Up != null) {
                    InputFocus = InputFocus.Up;
                }
                InputFocus.OnUp?.Invoke();
            }
            if (UILeftInput != null && InputManager.ButtonPressed(Players.One, UILeftInput.Key)) {
                if (InputFocus.Left != null) {
                    InputFocus = InputFocus.Left;
                }
                InputFocus.OnLeft?.Invoke();
            }
            if (UIDownInput != null && InputManager.ButtonPressed(Players.One, UIDownInput.Key)) {
                if (InputFocus.Down != null) {
                    InputFocus = InputFocus.Down;
                }
                InputFocus.OnDown?.Invoke();
            }
            if (UIRightInput != null && InputManager.ButtonPressed(Players.One, UIRightInput.Key)) {
                if (InputFocus.Right != null) {
                    InputFocus = InputFocus.Right;
                }
                InputFocus.OnRight?.Invoke();
            }
            if (UIBackInput != null && InputManager.ButtonPressed(Players.One, UIBackInput.Key)) {
                if (InputFocus.Back != null) {
                    InputFocus = InputFocus.Back;
                }
                InputFocus.OnBack?.Invoke();
            }
            if (UIConfirmInput != null && InputManager.ButtonPressed(Players.One, UIConfirmInput.Key)) {
                InputFocus.Owner.OnClick(new MouseEventArgs(MouseButtons.Left, Screen.MousePoint));
            }
        }

        private static bool IsMouseBlocker(UIObject uiObj)
            => mouseBlocker == null || mouseBlocker.Transform.Children.Contains(uiObj.Transform);

        public static QuickList<UIObject> GetGizmos() => gizmos;

        public static void AddGizmo(UIObject gizmo)
        {
            gizmos.Add(gizmo);
        }

        public static void RemoveGizmo(UIObject gizmo)
        {
            gizmos.Remove(gizmo);
        }

        public static UIObject GetMenu(string key)
        {
            menus.TryGetValue(key, out UIObject menu);
            return menu;
        }

        internal static Dictionary<string, UIObject> GetMenus()
        {
            return menus;
        }

        public static void ReorderMenus()
        {
            OrderedTopLevelMenus.Clear();
            foreach (UIObject menu in menus.Values) {
                OrderedTopLevelMenus.Add(menu);
            }
            OrderedTopLevelMenus.Sort((pair1, pair2) => pair1.Priority.CompareTo(pair2.Priority));
        }

        public static bool IsKeyboardFree()
        {
            return assumedKeyboardControl.Count < 1;
        }

        public static void AddAssumedKeyboardControl(UIObject uiObj)
        {
            if (!assumedKeyboardControl.Contains(uiObj)) {
                assumedKeyboardControl.Add(uiObj);
            }
        }

        public static void RemoveAssumedKeyboardControl(UIObject uiObj)
        {
            assumedKeyboardControl.Remove(uiObj);
        }

    }

}
