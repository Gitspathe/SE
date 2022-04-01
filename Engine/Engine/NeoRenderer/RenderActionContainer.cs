using SE.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace SE.NeoRenderer
{
    public class RenderActionContainer
    {
        private QuickList<IRenderAction> actions = new QuickList<IRenderAction>();
        private HashSet<IRenderAction> checkSet = new HashSet<IRenderAction>();
        private ActionComparer actionComparer = new ActionComparer();

        internal void Clear()
        {
            actions.Clear();
            checkSet.Clear();
        }

        public void AddAction(IRenderAction action)
        {
            if(checkSet.Contains(action))
                throw new Exception();

            actions.Add(action);
            checkSet.Add(action);
        }

        internal void Execute()
        {
            // Sort render actions...
            actions.Sort(actionComparer);

            // Execute render actions...
            IRenderAction[] arr = actions.Array;
            for(int i = 0; i < actions.Count; i++) {
                arr[i].Execute();
            }
        }

        private struct ActionComparer : IComparer<IRenderAction>
        {
            int IComparer<IRenderAction>.Compare(IRenderAction x, IRenderAction y)
                => x.RenderQueue.CompareTo(y.RenderQueue);
        }
    }
}
