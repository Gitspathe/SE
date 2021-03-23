namespace SE.Editor.GUI.Properties
{
    public class Properties : GUIObject
    {
        public IPropertiesView View;

        public override void OnPaint()
        {
            GUI.Begin("Properties");
            View?.OnPaint();
            GUI.End();
        }
    }
}
