#if EDITOR
using SE.Components.UI;

namespace SE.Editor.Components.UI
{

    /// <summary>
    /// This UIComponent is attached to buttons within the level editor user interface. It stores a tile ID to be used in level editing.
    /// </summary>
    internal class IDHolder : UIComponent
    {

        public int ID;

        public IDHolder(int id)
        {
            ID = id;
        }

    }

}
#endif