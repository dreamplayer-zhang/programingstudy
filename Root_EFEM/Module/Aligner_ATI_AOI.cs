using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Root_EFEM.Module
{
    public class Aligner_ATI_AOI
    {
        #region class Data
        public class Data
        {
            public int m_gvEdge = 120;
            public CPoint m_szMargin = new CPoint(100, 20);
            public CPoint m_szSmooth = new CPoint(0, 0);
            public CPoint m_szNotch = new CPoint(35, 35);

            public void Clone(Data data)
            {
                m_szMargin = new CPoint(data.m_szMargin);
                m_szSmooth = new CPoint(data.m_szSmooth);
                m_gvEdge = data.m_gvEdge;
                m_szNotch = new CPoint(data.m_szNotch);
            }

            public void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_gvEdge = tree.Set(m_gvEdge, m_gvEdge, "GV", "Edge GV value (0 ~ 255)", bVisible);
                m_szMargin = tree.Set(m_szMargin, m_szMargin, "Margin", "Margin Size (pixel)", bVisible);
                m_szSmooth = tree.Set(m_szSmooth, m_szSmooth, "Smooth", "Smoothing Size (pixel)", bVisible);
                m_szNotch = tree.Set(m_szNotch, m_szNotch, "Notch", "Notch Size (Pixel)", bVisible);
            }
        }
        public Data m_data = new Data();
        #endregion

        #region class Notch
        public class Notch
        {
            public double m_fScore = 0;
            public CPoint m_cpNotch = new CPoint();
            public double m_degNotch = 0;

            public Notch(CPoint cpNotch)
            {
                m_cpNotch = cpNotch;
            }

            List<CPoint> m_aPoints = new List<CPoint>();
            public void MakePoints(double a, double c, int h, int yOffset, List<int> aCalc)
            {
                double fSum = 0;
                double dSum = 0;
                m_aPoints.Clear();
                for (int x = m_cpNotch.X - h, xp = -h; x <= m_cpNotch.X + h; x++, xp++)
                {
                    int y = (int)(a * xp * xp + c);
                    CPoint cp = new CPoint(x, y + yOffset);
                    m_aPoints.Add(cp);
                    fSum += y;
                    dSum += Math.Abs(y - aCalc[x]);
                }
                int xCenter = aCalc.Count / 2;
                m_fScore = 100 * (fSum - dSum) / fSum - 10 * Math.Abs(m_cpNotch.X - xCenter) / xCenter;
                if (m_fScore < 0) m_fScore = 0;
            }
        }
        #endregion

        #region class AOI
        public class AOI
        {
            public string m_id;
            public int m_nID;
            public MemoryData m_memory;

            public AOI(string id, MemoryData memory, int nID)
            {
                m_id = id + "." + nID.ToString("000");
                m_nID = nID;
                m_memory = memory;
            }

            public List<int> m_aEdge = new List<int>();
            public List<int> m_aCalc = new List<int>();
            public List<int> m_aCircle = new List<int>();
            public List<bool> m_abEdge = new List<bool>();
            public void InitBuffer()
            {
                int lX = m_memory.p_sz.X;
                while (m_aEdge.Count < lX) m_aEdge.Add(0);
                while (m_aCalc.Count < lX) m_aCalc.Add(0);
                while (m_aCircle.Count < lX) m_aCircle.Add(0);
                while (m_abEdge.Count < lX) m_abEdge.Add(false);
            }

            public List<Notch> m_aNotch = new List<Notch>();
            public Notch m_maxNotch = null;
            public RPoint m_rpCenter = new RPoint();
            public double m_dR;
            public double m_posGrab = 0;

            public double GetNotchPos(double lRotate)
            {
                if (m_maxNotch == null) return m_posGrab;
                return m_posGrab + m_maxNotch.m_degNotch * lRotate / 360;
            }

            public double p_fScore
            {
                get { return (m_maxNotch == null) ? 0 : m_maxNotch.m_fScore; }
            }
        }

        public Queue<AOI> m_qAOI = new Queue<AOI>();
        public void StartInspect(AOI aoi)
        {
            aoi.m_maxNotch = null;
            m_qAOI.Enqueue(aoi);
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(ThreadRun));
            m_thread.Start();
        }

        void ThreadRun()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                if (m_qAOI.Count == 0) Thread.Sleep(10);
                else
                {
                    AOI aoi = m_qAOI.Dequeue();
                    try { Inspect(aoi); }
                    catch (Exception e) { m_log.Error("AOI Inspect Error : " + e.Message); }
                }
            }
        }
        #endregion

        #region Inspect
        AOI m_aoi;
        public string Inspect(AOI aoi)
        {
            m_aoi = aoi;
            InitValue(aoi);
            Smooth();
            FindEdge();
            FindCalc();
            FindNotch();
            CalcNotch();
            CalcCenter();
            DrawLines(); 
            return "OK";
        }

        CPoint m_szMem;
        int[] m_xROI;
        byte[] m_aBuf = new byte[1];
        void InitValue(AOI aoi)
        {
            m_szMem = aoi.m_memory.p_sz;
            m_xROI = new int[2] { m_data.m_szMargin.X, m_szMem.X - m_data.m_szMargin.X };
            int lSize = m_szMem.X * m_szMem.Y;
            if (m_aBuf.Length != lSize) m_aBuf = new byte[lSize];
            Marshal.Copy(aoi.m_memory.GetPtr(aoi.m_nID), m_aBuf, 0, lSize);
            aoi.InitBuffer();
        }
        #endregion

        #region Smooth
        const int c_lSmoothThread = 8;
        void Smooth()
        {
            if (m_data.m_szSmooth.X > 0) Parallel.For(0, c_lSmoothThread, y => SmoothX(y));
            if (m_data.m_szSmooth.Y > 0) Parallel.For(0, c_lSmoothThread, x => SmoothY(x));
        }

        void SmoothX(int y)
        {
            byte[] aBuf = new byte[m_szMem.X];
            while (y < m_szMem.Y)
            {
                SmoothX(y, aBuf);
                y += c_lSmoothThread;
            }
        }

        void SmoothX(int y, byte[] aBuf)
        {
            int nSum = 0;
            int xSmooth = m_data.m_szSmooth.X;
            int p = y * m_szMem.X + m_xROI[0] - xSmooth;
            for (int x = m_xROI[0] - xSmooth; x < m_xROI[0] + xSmooth; x++, p++) nSum += m_aBuf[p];
            int nDiv = 2 * xSmooth;
            p = y * m_szMem.X + m_xROI[0];
            int pm = y * m_szMem.X + m_xROI[0] - xSmooth;
            int pp = y * m_szMem.X + m_xROI[0] + xSmooth;
            for (int x = m_xROI[0]; x <= m_xROI[1]; x++, p++, pm++, pp++)
            {
                aBuf[x] = (byte)(nSum / nDiv);
                nSum += m_aBuf[pp] - m_aBuf[pm];
            }
            p = y * m_szMem.X + m_xROI[0];
            Array.Copy(aBuf, m_xROI[0], m_aBuf, y * m_szMem.X + m_xROI[0], m_xROI[1] - m_xROI[0]);
        }

        void SmoothY(int x)
        {
            byte[] aBuf = new byte[m_szMem.Y];
            while (x < m_szMem.X)
            {
                SmoothY(x, aBuf);
                x += c_lSmoothThread;
            }
        }

        void SmoothY(int x, byte[] aBuf)
        {
            int nSum = 0;
            int yMargin = m_data.m_szMargin.Y;
            int ySmooth = m_data.m_szSmooth.Y;
            int p = (yMargin - ySmooth) * m_szMem.X + x;
            for (int y = yMargin - ySmooth; y < yMargin + ySmooth; y++, p += m_szMem.X) nSum += m_aBuf[p];
            int nDiv = 2 * ySmooth;
            p = yMargin * m_szMem.X + x;
            int pm = (yMargin - ySmooth) * m_szMem.X + x;
            int pp = (yMargin + ySmooth) * m_szMem.X + x;
            for (int y = yMargin; y <= m_szMem.Y - yMargin; y++, p += m_szMem.X, pm += m_szMem.X, pp += m_szMem.X)
            {
                aBuf[y] = (byte)(nSum / nDiv);
                nSum += m_aBuf[pp] - m_aBuf[pm];
            }
            p = yMargin * m_szMem.X + x;
            for (int y = yMargin; y <= m_szMem.Y - yMargin; y++, p += m_szMem.X) m_aBuf[p] = aBuf[y];
        }
        #endregion

        #region FindEdge
        void Remove255(int y)
        {
            byte[] aBuf = m_aBuf;
            int p = y * m_szMem.X;
            for (int x = 0; x < m_szMem.X; x++, p++)
            {
                if (aBuf[p] > 254) aBuf[p] = (byte)254;
            }
        }

        bool m_bFindEdge = true;
        List<int> m_aEdge;
        void FindEdge()
        {
            m_aEdge = m_aoi.m_aEdge;
            Parallel.For(0, m_szMem.Y, y => Remove255(y));
            for (int x = 0; x < m_szMem.X; x++) m_aEdge[x] = 0;
            Parallel.For(m_xROI[0] - 1, m_xROI[1] + 1, x => FindEdge(x));
            m_bFindEdge = true;
            for (int n = 0; n < 10; n++)
            {
                FindEdgeSub();
                if (m_bFindEdge == false) return;
            }
        }

        void FindEdge(int x)
        {
            byte[] aBuf = m_aBuf;
            int yMargin = m_data.m_szMargin.Y;
            int y0 = m_szMem.Y - yMargin; 
            int p = x + y0 * m_szMem.X;
            for (int y = y0; y > yMargin; y--, p -= m_szMem.X)
            {
                if (aBuf[p] >= m_data.m_gvEdge) aBuf[p] = 255;
                else
                {
                    m_aEdge[x] = y - 1;
                    return;
                }
            }
        }

        const int c_nFindEdgeSub = 16;
        void FindEdgeSub()
        {
            m_bFindEdge = false;
            Parallel.For(0, c_nFindEdgeSub, n => FindEdgeSub(n));
        }

        void FindEdgeSub(int n)
        {
            int x0 = Math.Max(m_szMem.X * n / c_nFindEdgeSub, m_xROI[0]);
            int x1 = Math.Min(m_szMem.X * (n + 1) / c_nFindEdgeSub, m_xROI[1]);
            for (int x = x0; x < x1; x++) FindEdgeSub(x, x - 1);
            for (int x = x1 - 1; x >= x0; x--) FindEdgeSub(x, x + 1);
        }

        void FindEdgeSub(int x, int xn)
        {
            if (m_aEdge[x] >= m_aEdge[xn]) return;
            byte[] aBuf = m_aBuf;
            int y = m_aEdge[x] - 1;
            int p = x + y * m_szMem.X;
            int pn = xn + y * m_szMem.X;
            while (y >= m_aEdge[xn])
            {
                if ((aBuf[p] >= m_data.m_gvEdge) && (aBuf[pn] == 255))
                {
                    m_bFindEdge = true;
                    while (aBuf[p] >= m_data.m_gvEdge)
                    {
                        m_aEdge[x] = y;
                        aBuf[p] = 255;
                        y--;
                        p += m_szMem.X;
                        pn += m_szMem.X;

                    }
                }
                y--;
                p += m_szMem.X;
                pn += m_szMem.X;
            }
        }
        #endregion

        #region Calc
        int[] m_xCalc;
        List<int> m_aCalc;

        void FindCalc()
        {
            m_aCalc = m_aoi.m_aCalc;
            for (int x = 0; x < m_szMem.X; x++) m_aCalc[x] = 0;
            m_xCalc = new int[2] { m_xROI[0] + m_data.m_szNotch.X, m_xROI[1] - m_data.m_szNotch.X };
            for (int x = m_xCalc[0]; x < m_xCalc[1]; x++)
            {
                m_aCalc[x] = (m_aEdge[x - m_data.m_szNotch.X] + m_aEdge[x + m_data.m_szNotch.X]) - 2 * m_aEdge[x];
            }
        }
        #endregion

        #region FindNotch
        List<bool> m_abEdge;
        List<Notch> m_aNotch;
        void FindNotch()
        {
            m_abEdge = m_aoi.m_abEdge;
            m_aNotch = m_aoi.m_aNotch;
            m_aNotch.Clear();
            while (m_abEdge.Count < m_szMem.X) m_abEdge.Add(false);
            for (int x = 0; x < m_szMem.X; x++) m_abEdge[x] = true;
            for (int n = 0; n < 10; n++)
            {
                int xMax0 = 0;
                int yMax = 0;
                for (int x = m_xCalc[0]; x < m_xCalc[1]; x++)
                {
                    if (m_abEdge[x] && (yMax < m_aCalc[x]))
                    {
                        yMax = m_aCalc[x];
                        xMax0 = x;
                    }
                }
                if (yMax < (m_data.m_szNotch.Y / 4)) return;
                int xMax1 = FindNotchX(xMax0, 0);
                m_aNotch.Add(new Notch(new CPoint(xMax1, m_aEdge[xMax1])));
                int w = 2 * m_data.m_szNotch.X;
                for (int x = xMax0 - w; x <= xMax0 + w; x++) m_abEdge[x] = false;
                for (int x = xMax1 - w; x <= xMax1 + w; x++) m_abEdge[x] = false;
            }
        }

        int FindNotchX(int xMax, int nTry)
        {
            if (nTry > 3) return xMax;
            double sumY = 0;
            double sumXY = 0;
            for (int x = xMax - m_data.m_szNotch.X; x <= xMax + m_data.m_szNotch.X; x++)
            {
                int y = (m_aCalc[x] > 0) ? m_aCalc[x] : 0;
                sumY += y;
                sumXY += (x * y);
            }
            int xMax1 = (int)Math.Round(1.0 * sumXY / sumY);
            if (xMax == xMax1) return xMax;
            return FindNotchX(xMax1, nTry + 1);
        }
        #endregion

        #region CalcNotch
        const int c_hCalc = 50;
        void CalcNotch()
        {
            foreach (Notch notch in m_aNotch)
            {
                double y0 = m_aCalc[notch.m_cpNotch.X];
                double dy = Math.Abs(y0 - m_data.m_szNotch.Y);
                double h = 0.15 * m_data.m_szNotch.Y;
                if (dy > h)
                {
                    dy = h * Math.Sqrt(dy / h);
                    y0 = (y0 < m_data.m_szNotch.Y) ? y0 - dy : y0 + dy;
                }
                double a = -1.0 * y0 / m_data.m_szNotch.Y / m_data.m_szNotch.Y;
                notch.MakePoints(a, y0, m_data.m_szNotch.Y, c_hCalc, m_aCalc);
            }
            m_aoi.m_maxNotch = GetMaxNotch();
        }

        Notch GetMaxNotch()
        {
            if (m_aNotch.Count <= 0) return null;
            Notch notch = m_aNotch[0];
            for (int n = 1; n < m_aNotch.Count; n++)
            {
                if (notch.m_fScore < m_aNotch[n].m_fScore) notch = m_aNotch[n];
            }
            return notch;
        }
        #endregion

        #region CalcCenter
        RPoint m_rpCenter = new RPoint();
        double m_dR;
        double[,] m_XY = new double[4, 4];
        double[,] m_a = new double[3, 3];
        double[] m_b = new double[3];
        void CalcCenter()
        {
            if (m_aNotch.Count <= 0) return;
            CalcCenterEdge();
            GaussEllimination();
            CalcCircle();
            CalcAngle();
        }

        void CalcCenterEdge()
        {
            for (int x = 0; x < 4; x++) for (int y = 0; y < 4; y++) m_XY[x, y] = 0;
            for (int y = 0; y < 3; y++)
            {
                m_b[y] = 0;
                for (int x = 0; x < 3; x++) m_a[x, y] = 0;
            }
            for (int x = m_xCalc[0]; x < m_xCalc[1]; x++)
            {
                if (m_abEdge[x])
                {
                    int y = m_aEdge[x];
                    m_XY[0, 0]++;
                    m_XY[0, 1] += y;
                    m_XY[1, 0] += x;
                    m_XY[0, 2] += (y * y);
                    m_XY[1, 1] += (x * y);
                    m_XY[2, 0] += (x * x);
                    m_XY[0, 3] += ((double)y * y * y);
                    m_XY[1, 2] += ((double)x * y * y);
                    m_XY[2, 1] += ((double)x * x * y);
                    m_XY[3, 0] += ((double)x * x * x);
                }

            }
            m_a[0, 0] = m_XY[2, 0];
            m_a[1, 1] = m_XY[0, 2];
            m_a[2, 2] = m_XY[0, 0];
            m_a[1, 0] = m_a[0, 1] = m_XY[1, 1];
            m_a[2, 0] = m_a[0, 2] = m_XY[1, 0];
            m_a[2, 1] = m_a[1, 2] = m_XY[0, 1];
            m_b[0] = -m_XY[3, 0] - m_XY[1, 2];
            m_b[1] = -m_XY[2, 1] - m_XY[0, 3];
            m_b[2] = -m_XY[2, 0] - m_XY[0, 2];
        }

        bool GaussEllimination()
        {
            for (int k = 0; k < 3; k++)
            {
                if (m_a[k, k] == 0)
                {
                    for (int i = k + 1; i < 3; i++)
                    {
                        if (m_a[i, k] != 0) continue;
                        for (int j = 0; j < 3; j++) Swap(ref m_a[k, j], ref m_a[i, j]);
                        Swap(ref m_b[k], ref m_b[i]);
                        break;
                    }
                }
                if (m_a[k, k] == 0) return true;
                for (int i = k + 1; i < 3; i++)
                {
                    m_a[i, k] /= m_a[k, k];
                    for (int j = k + 1; j < 3; j++) m_a[i, j] -= (m_a[i, k] * m_a[k, j]);
                    m_b[i] -= (m_a[i, k] * m_b[k]);
                }
            }
            double[] dResult = new double[3] { 0, 0, 0 };
            for (int i = 2; i >= 0; i--)
            {
                double dSum = 0;
                for (int j = i + 1; j < 3; j++) dSum += (m_a[i, j] * dResult[j]);
                dResult[i] = (m_b[i] - dSum) / m_a[i, i];
            }
            dResult[0] = -0.5 * dResult[0];
            dResult[1] = -0.5 * dResult[1];
            dResult[2] = Math.Sqrt(dResult[0] * dResult[0] + dResult[1] * dResult[1] - dResult[2]);
            m_rpCenter = new RPoint(dResult[0], dResult[1]);
            m_dR = dResult[2];
            m_aoi.m_rpCenter = new RPoint(m_rpCenter);
            m_aoi.m_dR = m_dR;
            return false;
        }

        void Swap(ref double a, ref double b)
        {
            double c = a;
            a = b;
            b = c;
        }

        List<int> m_aCircle;
        void CalcCircle()
        {
            m_aCircle = m_aoi.m_aCircle;
            for (int x = 0; x < m_szMem.X; x++)
            {
                double dx = Math.Abs(x - m_rpCenter.X);
                double dy = Math.Sqrt(m_dR * m_dR - dx * dx);
                m_aCircle[x] = (int)(m_rpCenter.Y + dy);
                if (m_aCircle[x] < 0) m_aCircle[x] = 0;
            }
        }

        void CalcAngle()
        {
            foreach (Notch notch in m_aNotch)
            {
                double dx = m_rpCenter.X - notch.m_cpNotch.X;
                double dy = m_rpCenter.Y - notch.m_cpNotch.Y;
                notch.m_degNotch = 90 - Math.Atan2(dy, dx) * 180 / Math.PI;
            }
        }
        #endregion

        #region DrawLines
        void DrawLines()
        {
            MemoryDraw draw = m_aoi.m_memory.m_aDraw[m_aoi.m_nID];
            draw.Clear();
            draw.AddPolyline(Brushes.Red, m_aEdge);
            draw.AddPolyline(Brushes.Aqua, m_aCalc, m_szMem.Y - m_data.m_szMargin.Y, -1);
            draw.AddPolyline(Brushes.Yellow, m_aCircle);
            foreach (Notch notch in m_aNotch) draw.AddCross(Brushes.Orange, notch.m_cpNotch, 10);
            draw.AddText(Brushes.Red, m_aNotch[0].m_cpNotch, "Notch"); 
            draw.InvalidDraw();
        }
        #endregion

        Log m_log;
        public Aligner_ATI_AOI(Log log)
        {
            m_log = log;
            InitThread();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
        }
    }
}
