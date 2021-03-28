namespace SE.Editor.GUI.Windows.Properties
{
    public class PropertiesWindow : GUIObject
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
