using Root_AxisMapping.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Runtime.InteropServices;

namespace Root_AxisMapping.MainUI
{
    public class Merge : NotifyProperty
    {
        #region Mapping Property
        int p_xArray { get { return m_mapping.p_xArray; } }
        int p_yArray { get { return m_mapping.p_yArray; } }
        Array[,] p_aArray { get { return m_mapping.m_aArray; } }
        double p_dx { get { return m_mapping.m_dx; } }
        int p_xSetup {  get { return m_mapping.m_xSetup; } }
        double p_yGap { get { return m_result.p_yGap; } }
        #endregion

        #region Run
        public void Run()
        {
            for (int ix = 0; ix < p_xArray; ix++) Run(ix);
            EraseMargin(); 
        }

        int m_wCam = 12000; 
        int m_wCamValid = 10000;
        double m_pulsePerPixel = 12; 
        void Run(int ix) //forget
        {
            double dx = p_dx * (p_xSetup - ix);
            int dPixel = (int)Math.Round(m_wCam / 2 - p_aArray[p_xArray / 2, p_yArray / 2].m_rpCenter.X);
            RunGrab(dx + (dPixel + m_wCamValid / 2) * m_pulsePerPixel); 
            int wMargin = (m_wCam - m_wCamValid) / 2;
            ImageCopy(m_wCam - wMargin - m_wCopy / 2, m_wCopy * ix);
            RunGrab(dx + (dPixel - m_wCamValid / 2) * m_pulsePerPixel);
            ImageCopy(wMargin, m_wCopy * ix + m_wCopy / 2); 
        }

        void RunTreeCam(Tree tree)
        {
            m_wCam = tree.Set(m_wCam, m_wCam, "Width", "Camera Valid Width (pixel)");
            m_wCamValid = tree.Set(m_wCamValid, m_wCamValid, "Valid Width", "Camera Valid Width (pixel)");
            m_pulsePerPixel = tree.Set(m_pulsePerPixel, m_pulsePerPixel, "Pulse / Pixel", "Pulse / Pixel"); 
        }
        #endregion

        #region Grab
        string RunGrab(double dx)
        {
            AxisMapping.Run_Grab runGrab = (AxisMapping.Run_Grab)m_axisMapping.m_runGrab.Clone();
            runGrab.m_xStart += dx;
            runGrab.Run();
            return "OK";
        }
        #endregion

        #region Image Copy
        int m_wCopy = 1000; 
        int m_wMargin = 10;
        void ImageCopy(int x0, int x1)
        {
            CPoint cpCenter = new CPoint(p_aArray[p_xArray / 2, p_yArray / 2].m_rpCenter);
            for (int y = 0; y < p_yArray; y++)
            {
                int yp = (int)(Math.Round(cpCenter.Y + p_yGap * (y - p_yArray / 2))); //forget
                ImageCopy(new CPoint(x0, yp - m_wCopy / 2), new CPoint(x1, y * m_wCopy)); 
            }
        }

        MemoryData p_memSrc { get { return m_axisMapping.m_memoryPool.m_viewer.p_memoryData; } }
        MemoryData p_memDst { get { return m_axisMapping.m_memoryPoolMerge.m_viewer.p_memoryData; } }
        unsafe void ImageCopy(CPoint cp0, CPoint cp1)
        {
            for (int y = 0; y < m_wCopy; y++)
            {
                IntPtr ipSrc = p_memSrc.GetPtr(0, cp0.X, cp0.Y + y);
                IntPtr ipDst = p_memDst.GetPtr(0, cp1.X, cp1.Y + y); 
                Buffer.MemoryCopy(ipSrc.ToPointer(), ipDst.ToPointer(), m_wCopy / 2, m_wCopy / 2); 
            }
        }

        void EraseMargin()
        {
            m_aZeroY = new byte[p_memDst.p_sz.X];
            for (int x = 0; x < p_memDst.p_sz.X; x++) m_aZeroY[x] = 0; 
            for (int y = 0; y < p_yArray; y++) EraseMarginY(y);
            m_aZeroX = new byte[m_wMargin];
            for (int x = 0; x < m_wMargin; x++) m_aZeroX[x] = 0;
            for (int x = 0; x < p_xArray; x++) EraseMarginX(x); 
        }

        byte[] m_aZeroY = null; 
        void EraseMarginY(int iy)
        {
            for (int i = 0; i < m_wMargin; i++)
            {
                IntPtr ip = p_memDst.GetPtr(0, 0, m_wCopy * iy + i);
                Marshal.Copy(m_aZeroY, 0, ip, p_memDst.p_sz.X);
                ip = p_memDst.GetPtr(0, 0, m_wCopy * (iy + 1) - i - 1);
                Marshal.Copy(m_aZeroY, 0, ip, p_memDst.p_sz.X);
            }
        }

        byte[] m_aZeroX = null;
        void EraseMarginX(int ix)
        {
            int xp0 = ix * m_wCopy;
            int xp1 = (ix + 1) * m_wCopy - m_wMargin; 
            for (int y = 0; y < p_memDst.p_sz.Y; y++)
            {
                IntPtr ip = p_memDst.GetPtr(0, xp0, y);
                Marshal.Copy(m_aZeroX, 0, ip, m_wMargin);
                ip = p_memDst.GetPtr(0, xp1, y);
                Marshal.Copy(m_aZeroX, 0, ip, m_wMargin);
            }
        }

        void RunTreeImage(Tree tree)
        {
            m_wCopy = tree.Set(m_wCopy, m_wCopy, "Copy", "Copy Image Size (pixel)");
            m_wMargin = tree.Set(m_wMargin, m_wMargin, "Margin", "Copy Image Margin (pixel)");
        }
        #endregion

        #region Tree Grab
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(m_id, m_log);
            RunTree(Tree.eMode.RegRead);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            RunTreeCam(m_treeRoot.GetTree("Camera"));
            RunTreeImage(m_treeRoot.GetTree("Image")); 
        }
        #endregion

        string m_id;
        Mapping m_mapping;
        Result m_result; 
        AxisMapping_Engineer m_engineer;
        public AxisMapping m_axisMapping;
        Log m_log;
        public Merge(string id, Mapping mapping, Result result, AxisMapping_Engineer engineer)
        {
            m_id = id;
            m_mapping = mapping;
            m_result = result; 
            m_engineer = engineer;
            m_axisMapping = ((AxisMapping_Handler)engineer.ClassHandler()).m_axisMapping;
            m_log = LogView.GetLog(id);
            InitTree(); 
        }
    }
}
