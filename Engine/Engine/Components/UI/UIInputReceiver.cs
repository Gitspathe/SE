using SE.Core;
using System;

namespace SE.Components.UI
{
    public class UIInputReceiver : UIComponent
    {
        public UIInputReceiver Up { get; set; }
        public Action OnUp;

        public UIInputReceiver Right { get; set; }
        public Action OnRight;

        public UIInputReceiver Down { get; set; }
        public Action OnDown;

        public UIInputReceiver Left { get; set; }
        public Action OnLeft;

        public UIInputReceiver Back { get; set; }
        public Action OnBack;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (UIManager.InputFocus == this) {
                UIManager.InputFocus = null;
            }
        }
    }
}
