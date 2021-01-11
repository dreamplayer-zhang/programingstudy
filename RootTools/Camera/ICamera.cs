using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Windows.Controls;

namespace RootTools.Camera
{
    public enum eGrabDirection
    {
        Forward,
        BackWard,
    }

    public interface ICamera
    {
        event EventHandler Grabed;

        string p_id { get; set; }

        int p_nGrabProgress { get; set; }

        TreeRoot p_treeRoot { get; set; }

        CPoint p_sz { get; set; }

        UserControl p_ui { get; }

        void ThreadStop();
        
        CPoint GetRoiSize();

        string StopGrab();

        void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReserveOffsetY = 0);
        void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReverseOffsetY = 0);
        double GetFps();
    }

    public class GrabedArgs : System.EventArgs
    {
        public MemoryData mdMemoryData;
        public int nFrameCnt;
        public CRect rtRoi;
        public int nProgress;
        public GrabedArgs(MemoryData md, int FrameCnt, CRect Roi, int progress)
        {
            mdMemoryData = md;
            nFrameCnt = FrameCnt;
            rtRoi = Roi;
            nProgress = progress;
        }
    }
}
