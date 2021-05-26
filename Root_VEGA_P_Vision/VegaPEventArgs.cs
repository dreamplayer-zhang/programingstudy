using RootTools;
using System;

namespace Root_VEGA_P_Vision
{
    public class SnapDoneArgs:EventArgs
    {
        public readonly CPoint startPos;
        public readonly CPoint endPos;

        public SnapDoneArgs(CPoint startPos, CPoint endPos)
        {
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }
    public class RecipeEventArgs : EventArgs
    {

        public RecipeEventArgs()
        {

        }
    }
    public class ImageROIEventArgs:EventArgs
    {
        public readonly string memstr;
        public ImageROIEventArgs(string memstr)
        {
            this.memstr = memstr;
        }
    }
}
