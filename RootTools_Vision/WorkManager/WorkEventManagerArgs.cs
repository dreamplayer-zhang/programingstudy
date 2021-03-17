using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    [Serializable]
    public class WorkDoneAllEventArgs : EventArgs
    {
        public WorkDoneAllEventArgs()
        {
            
        }
    }
    [Serializable]
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

    [Serializable]
    public class InspectionDoneEventArgs : EventArgs
    {
        public readonly Workplace workplace;
        public readonly List<CRect> listRect;

        public InspectionDoneEventArgs(List<CRect> _rect)
        {
            this.workplace = new Workplace();
            this.listRect = _rect;
        }

        public InspectionDoneEventArgs(List<CRect> _rect, Workplace workplace)
        {
            this.workplace = workplace;
            this.listRect = _rect;
        }
    }
    [Serializable]
    public class ProcessDefectDoneEventArgs : EventArgs
    {
        public ProcessDefectDoneEventArgs()
        {

        }
    }
    [Serializable]
    public class IntegratedProcessDefectDoneEventArgs : EventArgs
    {
        public IntegratedProcessDefectDoneEventArgs()
        {

        }
    }
    [Serializable]
    public class ProcessDefectWaferStartEventArgs : EventArgs
    {
        public ProcessDefectWaferStartEventArgs()
        {

        }
    }
    [Serializable]
    public class ProcessDefectEdgeDoneEventArgs : EventArgs
    {
        public ProcessDefectEdgeDoneEventArgs()
        {

        }
    }
    [Serializable]
    public class ProcessMeasurementDoneEventArgs : EventArgs
    {
        public ProcessMeasurementDoneEventArgs()
        {

        }
    }
    [Serializable]
    public class UIRedrawEventArgs : EventArgs
    {
        public UIRedrawEventArgs()
        {

        }
    }

    [Serializable]
    public class WorkplaceStateChangedEventArgs : EventArgs
    {
        public readonly Workplace workplace;

        public WorkplaceStateChangedEventArgs(Workplace _workplace)
        {
            this.workplace = _workplace;
        }
    }
    [Serializable]
    public class RequestStopEventArgs : EventArgs
    {
        public RequestStopEventArgs()
        {
        }
    }
    [Serializable]
    public class InspectionStartArgs : EventArgs
    {
        public InspectionStartArgs()
        {
        }
    }

    [Serializable]
    public class MemoryIDArgs : EventArgs
    {
        public readonly MemoryID MemoryID;

        public MemoryIDArgs(MemoryID memoryID)
        {
            this.MemoryID = memoryID;
        }
    }

    [Serializable]
    public class LogArgs : EventArgs
    {
        public readonly LOG_MESSAGE_TYPE type;
        public readonly string msg;

        public LogArgs(LOG_MESSAGE_TYPE type, string msg)
        {
            this.type = type;
            this.msg = msg;
        }
    }
}
