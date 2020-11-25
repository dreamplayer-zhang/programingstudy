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
        }

        byte[] m_aZero = new byte[10]; 
        void ClearMemory()
        {
            int w = p_memDst.p_sz.X;
            if (m_aZero.Length != w)
            {
                m_aZero = new byte[w];
                for (int n = 0; n < w; n++) m_aZero[n] = 0; 
            }
            for (int y = 0; y < p_memDst.p_sz.Y; y++)
            {
                IntPtr ip = p_memDst.GetPtr(0, 0, y);
                Marshal.Copy(m_aZero, 0, ip, w); 
            }
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
            ImageCopy(m_wCam - wMargin + m_wMargin, m_wCopy * ix + m_wMargin);
            RunGrab(dx + (dPixel - m_wCamValid / 2) * m_pulsePerPixel);
            ImageCopy(wMargin + m_wMargin, m_wCopy * ix + m_wCopy / 2); 
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
        int m_wMargin = 2;
        void ImageCopy(int x0, int x1)
        {
            CPoint cpCenter = new CPoint(p_aArray[p_xArray / 2, p_yArray / 2].m_rpCenter);
            for (int y = 0; y < p_yArray; y++)
            {
                int yp = (int)(Math.Round(cpCenter.Y + p_yGap * (y - p_yArray / 2)));
                ImageCopy(new CPoint(x0, yp - m_wCopy / 2 + m_wMargin), new CPoint(x1, y * m_wCopy + m_wMargin)); 
            }
        }

        MemoryData p_memSrc { get { return m_axisMapping.m_memoryPool.m_viewer.p_memoryData; } }
        MemoryData p_memDst { get { return m_axisMapping.m_memoryPoolMerge.m_viewer.p_memoryData; } }
        unsafe void ImageCopy(CPoint cp0, CPoint cp1)
        {
            int l = m_wCopy / 2 - m_wMargin; 
            for (int y = 0; y < 2 * l; y++)
            {
                IntPtr ipSrc = p_memSrc.GetPtr(0, cp0.X, cp0.Y + y);
                IntPtr ipDst = p_memDst.GetPtr(0, cp1.X, cp1.Y + y); 
                Buffer.MemoryCopy(ipSrc.ToPointer(), ipDst.ToPointer(), l, l); 
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
