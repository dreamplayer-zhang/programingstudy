using System.Windows.Controls;
using RootTools.Memory;
using RootTools.Trees;

namespace RootTools.Camera
{
    public enum eGrabDirection
    {
        Forward,
        BackWard,
    }

    public interface ICamera
    {
        event System.EventHandler Grabed;
        string p_id { get; set; }

        int p_nGrabProgress { get; set; }

        TreeRoot p_treeRoot { get; set; }

        CPoint p_sz { get; set; }

        UserControl p_ui { get; }

        void ThreadStop();
        
        CPoint GetRoiSize();

        string StopGrab();

        void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReserveOffsetY = 0);
    }

    public class GrabedArgs : System.EventArgs
    {
        public MemoryData mdMemoryData;
        public int nFrameCnt;
        public CRect rtRoi;
        public GrabedArgs(MemoryData md, int FrameCnt, CRect Roi)
        {
            mdMemoryData = md;
            nFrameCnt = FrameCnt;
            rtRoi = Roi;
        }
    }
}
