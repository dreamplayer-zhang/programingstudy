using Root_ASIS.AOI;
using Root_AxisMapping.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
            //for (int ix = 11; ix < 13; ix++) Run(ix); //Test 용도
            EraseMargin(); 
        }

        int m_wCam = 12000; 
        int m_wCamValid = 10000;
        double m_pulsePerPixel = 12;
        int m_pulseOffset = 120;
        void Run(int ix) //forget
        {
            double dx = p_dx * (p_xSetup - ix);
            int dPixel = (int)Math.Round(m_wCam / 2 - p_aArray[p_xArray / 2, p_yArray / 2].m_rpCenter.X);
            RunGrab(dx + (dPixel + m_wCamValid / 2) * m_pulsePerPixel + m_pulseOffset); 
            int wMargin = (m_wCam - m_wCamValid) / 2;
            ImageCopy(m_wCam - wMargin - m_wCopy / 2, m_wCopy * ix);
            RunGrab(dx + (dPixel - m_wCamValid / 2) * m_pulsePerPixel - m_pulseOffset);
            ImageCopy(wMargin , m_wCopy * ix + m_wCopy / 2); 
        }

        void RunTreeCam(Tree tree)
        {
            m_wCam = tree.Set(m_wCam, m_wCam, "Width", "Camera Valid Width (pixel)");
            m_wCamValid = tree.Set(m_wCamValid, m_wCamValid, "Valid Width", "Camera Valid Width (pixel)");
            m_pulsePerPixel = tree.Set(m_pulsePerPixel, m_pulsePerPixel, "Pulse / Pixel", "Pulse / Pixel");
            m_pulseOffset = tree.Set(m_pulseOffset, m_pulseOffset, "Pulse Offset", "Pulse Offset");
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

        #region Inspect
        CPoint m_szAOI = new CPoint();
        List<AOIData> m_aAOI = new List<AOIData>();
        List<Blob> m_aBlob = new List<Blob>(); 
        public void Inspect()
        {
            m_szAOI = new CPoint(m_wCopy - 2 * m_wMargin, m_wCopy - 2 * m_wMargin);
            m_aAOI.Clear();
            while (m_aBlob.Count < p_xArray) m_aBlob.Add(new Blob());
            Parallel.For(0, p_xArray, x =>
            {
                Inspect(x); 
            }); 
            Draw(AOIData.eDraw.Inspect); 
        }

        void Inspect(int x)
        {
            Blob blob = m_aBlob[x];
            for (int y = 0; y < p_yArray; y++) Inspect(blob, x, y); 
        }

        CPoint m_mmGV = new CPoint(160, 0); 
        void Inspect(Blob blob, int x, int y)
        {
            if (p_aArray[x, y].p_eState == Array.eState.Empty) return;
            AOIData aoi = new AOIData("(" + x.ToString() + ", " + y.ToString() + ")", m_szAOI);
            aoi.p_cp0 = new CPoint(x * m_wCopy + m_wMargin, y * m_wCopy + m_wMargin);
            blob.RunBlob(p_memDst, 0, aoi.p_cp0, m_szAOI, m_mmGV.X, m_mmGV.Y, 10);
            blob.RunSort(Blob.eSort.Size);
            if (blob.m_aSort.Count == 0) return;
            Blob.Island island = blob.m_aSort[0];
            aoi.m_bInspect = true;
            aoi.m_rpCenter = island.m_rpCenter;
            CPoint dp = new CPoint(island.m_rpCenter - new RPoint((x + 0.5) * m_wCopy, (y + 0.5) * m_wCopy)); 
            aoi.m_sDisplay = dp.ToString() + ", " + island.m_sz.ToString();
            m_aAOI.Add(aoi); 
        }

        public void Draw(AOIData.eDraw eDraw)
        {
            if (m_axisMapping.m_memoryPoolMerge.m_viewer.p_memoryData == null) return;
            MemoryDraw draw = m_axisMapping.m_memoryPoolMerge.m_viewer.p_memoryData.m_aDraw[0];
            draw.Clear();
            foreach (AOIData aoi in m_aAOI) aoi.Draw(draw, eDraw); 
            draw.InvalidDraw();
        }

        void RunTreeInspect(Tree tree)
        {
            m_mmGV = tree.Set(m_mmGV, m_mmGV, "GV", "Gray Value Range (0~255)");
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
            RunTreeInspect(m_treeRoot.GetTree("Inspect")); 
        }
        #endregion

        string m_id;
        public Mapping m_mapping;
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
