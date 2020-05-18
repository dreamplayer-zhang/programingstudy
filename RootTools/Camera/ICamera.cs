using System.Windows.Controls;
using RootTools.Memory;
using RootTools.Trees;

namespace RootTools.Camera
{
    public interface ICamera
    {  
        string p_id { get; }

        int p_nGrabProgress { get; }

        TreeRoot p_treeRoot { get; set; }

        CPoint p_sz { get; }

        UserControl p_ui { get; }

        void ThreadStop();

        void StopGrabbing();

        void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReserveOffsetY = 0);
    }
}
