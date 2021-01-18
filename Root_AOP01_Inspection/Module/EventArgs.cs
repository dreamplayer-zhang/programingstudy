using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_AOP01_Inspection.Module
{
    public class SnapDoneArgs : EventArgs
    {
        public readonly CPoint startPosition;
        public readonly CPoint endPosition;

        public SnapDoneArgs(CPoint _startPosition, CPoint _endPosition)
        {
            this.startPosition = _startPosition;
            this.endPosition = _endPosition;
        }
    }

    public class RecipeEventArgs : EventArgs
    {

        public RecipeEventArgs()
        {

        }
    }
}
