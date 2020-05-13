using SE.Common;
using SE.UI;

namespace SE.Components.UI
{

    /// <summary>
    /// Component which is attached to a UIObject.
    /// </summary>
    public class UIComponent : Component
    {
        public new UIObject Owner => (UIObject) OwnerProp;

        protected sealed override GameObject OwnerProp { get; set; }

        /// <summary>
        /// Called when the owner of the UIComponent is clicked.
        /// </summary>
        public virtual void OnClick() { }

        /// <summary>
        /// Called when the owner of the UIComponent is hovered over with the mouse.
        /// </summary>
        public virtual void OnSelected() { }

        /// <summary>
        /// Called when the mouse isn't over the owner of the UIComponent.
        /// </summary>
        public virtual void OnDeselected() { }

        /// <summary>
        /// Called when mouse click is released.
        /// </summary>
        public virtual void OnClickRelease() { }

        /// <summary>
        /// Called when the mouse is clicked away from the owner of the UIComponent.
        /// </summary>
        public virtual void OnClickAway() { }

        /// <summary>
        /// Called when the owner of the UIComponent is dragged with the mouse.
        /// </summary>
        public virtual void OnDrag() { }

    }

}
