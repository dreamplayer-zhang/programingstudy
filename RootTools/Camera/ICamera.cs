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
    public class GrabData
    {
        public int nUserSet = 1;
        public int nScanOffsetY = 0;
        public bool bInvY = false;
        public int ReverseOffsetY = 0;
        public double m_dScale = 1; //Gray
        public double m_dScaleR = 1;
        public double m_dScaleG = 1;
        public double m_dScaleB = 1;
        public double m_dShiftR = 0;
        public double m_dShiftG = 0;
        public double m_dShiftB = 0;
        public int m_nFovStart = 0;
        public int m_nFovSize = 8000;
        public int m_nOverlap = 0;

        public int m_nYShiftR = 0; // 채널별 Y 영상 맺히는 위치
        public int m_nYShiftG = 0;
        public int m_nYShiftB = 0;

        public bool m_bUseFlipVertical = false;   // VisionWorks2 - Root Memory 간의 영상 상하 반전을 위함. (버퍼 아래서부터 이미지 올림)
    }
    public interface ICamera
    {
        event EventHandler Grabed;

        string p_id { get; set; }
        bool bStopThread
        {
            get;set; }
        int p_nGrabProgress { get; set; }

        TreeRoot p_treeRoot { get; set; }

        CPoint p_sz { get; set; }

        UserControl p_ui { get; }

        void ThreadStop();
        
        CPoint GetRoiSize();

        string StopGrab();


        //void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine,int nScanOffsetY =0, bool bInvY = false, int ReserveOffsetY = 0, GrabData m_GrabData = null);
        //void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, int nScanOffsetY = 0, bool bInvY = false, int ReverseOffsetY = 0, GrabData m_GrabData = null);/
        void GrabLineScan(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null, bool bTest = false);
        void GrabLineScanColor(MemoryData memory, CPoint cpScanOffset, int nLine, GrabData m_GrabData = null);
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
