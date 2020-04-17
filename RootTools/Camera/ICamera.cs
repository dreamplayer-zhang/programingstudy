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
        string p_id { get; }
        int p_nGrabProgress
        {
            get;
        }
        TreeRoot p_treeRoot
        {
            get;
            set;
        }

        UserControl p_ui { get; }

        

        void ThreadStop();

        CPoint GetRoiSize();

        void StopGrabbing();

        void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, bool bInvY = false, int ReserveOffsetY = 0);
    }
}
