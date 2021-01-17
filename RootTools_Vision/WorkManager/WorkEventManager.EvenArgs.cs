using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class PositionDoneEventArgs : EventArgs
    {
        public readonly CPoint ptOldStart;
        public readonly CPoint ptOldEnd;

        public readonly CPoint ptNewStart;
        public readonly CPoint ptNewEnd;

        public readonly bool bSuccess;

        public PositionDoneEventArgs(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, bool bSuccess)
        {
            this.ptOldStart = ptOldStart;
            this.ptOldEnd = ptOldEnd;
            this.ptNewStart = ptNewStart;
            this.ptNewEnd = ptNewEnd;
            this.bSuccess = bSuccess;
        }
    }

    public class InspectionDoneEventArgs : EventArgs
    {
        public readonly List<CRect> listRect;
        public readonly bool reDraw;

        public InspectionDoneEventArgs(List<CRect> _rect, bool _reDraw = false)
        {
            this.listRect = _rect;
            this.reDraw = _reDraw;
        }
    }


    public class ProcessDefectDoneEventArgs : EventArgs
    {
        public ProcessDefectDoneEventArgs()
        {

        }
    }

    public class ProcessDefectWaferDoneEventArgs : EventArgs
    {
        public ProcessDefectWaferDoneEventArgs()
        {

        }
    }

    public class ProcessMeasurementDoneEventArgs : EventArgs
	{
        public ProcessMeasurementDoneEventArgs()
		{

		}
    }

    public class UIRedrawEventArgs : EventArgs
    { 
        public UIRedrawEventArgs()
        {

        }
    }


    public class WorkplaceStateChangedEventArgs : EventArgs
    {
        public readonly Workplace workplace;

        public WorkplaceStateChangedEventArgs(Workplace _workplace)
        {
            this.workplace = _workplace;
        }
    }


}
