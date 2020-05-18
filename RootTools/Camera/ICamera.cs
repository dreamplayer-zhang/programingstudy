using System.Windows.Controls;
using RootTools.Memory;
using RootTools.Trees;

namespace RootTools.Camera
{
    public interface ICamera
    {  
        string p_id { get; set; }

        int p_nGrabProgress { get; set; }

        TreeRoot p_treeRoot { get; set; }

        CPoint p_sz { get; set; }

        UserControl p_ui { get; }

        void ThreadStop();

        void StopGrabbing();

        void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReserveOffsetY = 0);
    }
}
