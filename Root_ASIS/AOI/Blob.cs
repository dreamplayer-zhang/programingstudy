using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Data;

namespace Root_ASIS.AOI
{
    public class Blob
    {
        #region Buffer & Visit 
        byte[,] m_aDst = null;

        class Visit
        {
            public bool m_bVisit = false;
            public int X;
            public int Y; 

            public void Set(bool bVisit, int x, int y)
            {
                m_bVisit = bVisit;
                X = x;
                Y = y;
            }

            public bool IsSame(int x, int y)
            {
                if (x != X) return false;
                if (y != Y) return false;
                return true; 
            }

            public Visit(int x, int y)
            {
                Set(false, x, y); 
            }
        }
        Visit[,] m_aVisit = null;

        void InitData(int nGV0, int nGV1)
        {
            byte[] aBuf = new byte[p_sz.X]; 
            for (int y = 0; y < p_sz.Y; y++)
            {
                Marshal.Copy(m_memory.GetPtr(m_iMemory, m_cp.X, m_cp.Y + y), aBuf, 0, p_sz.X);
                for (int x = 0; x < p_sz.X; x++)
                {
                    m_aVisit[y, x].Set(false, x, y); 
                    m_aDst[y, x] = ((aBuf[x] >= nGV0) && (aBuf[x] <= nGV1)) ? (byte)255 : (byte)0;
                }
            }
        }
        #endregion

        #region Property
        CPoint _sz = new CPoint(); 
        public CPoint p_sz
        {
            get { return _sz; }
            set
            {
                CPoint sz = value;
                if (sz.X < 0) { m_cp.X += sz.X; sz.X = -sz.X; }
                if (sz.Y < 0) { m_cp.Y += sz.Y; sz.Y = -sz.Y; }
                if (_sz == sz) return;
                _sz = sz;
                m_aDst = new byte[_sz.Y, _sz.X]; 
                m_aVisit = new Visit[_sz.Y, _sz.X];
                for (int y = 0; y < p_sz.Y; y++)
                {
                    for (int x = 0; x < p_sz.X; x++) m_aVisit[y, x] = new Visit(x, y);
                }
            }
        }

        CPoint m_cp;

        bool IsRangeOver(CPoint cp)
        {
            return (cp.X < 0) || (cp.Y < 0) || (cp.X >= m_memory.p_sz.X) || (cp.Y >= m_memory.p_sz.Y); 
        }
        #endregion

        #region Island
        public class Island
        {
            public int m_nSize;
            public CPoint m_sz = new CPoint();
            public int m_nLength; 
            public CPoint[] m_cp = new CPoint[2] { new CPoint(), new CPoint() };
            public RPoint m_rpCenter = new RPoint(); 

            public void Add(int x, int y)
            {
                m_nSize++;
                if (m_cp[0].X > x) m_cp[0].X = x;
                if (m_cp[0].Y > y) m_cp[0].Y = y;
                if (m_cp[1].X < x) m_cp[1].X = x;
                if (m_cp[1].Y < y) m_cp[1].Y = y;
                m_rpCenter.X += x;
                m_rpCenter.Y += y; 
            }

            public void Done(CPoint cp)
            {
                m_sz = m_cp[1] - m_cp[0];
                m_nLength = Math.Max(m_sz.X, m_sz.Y);
                m_cp[0] += cp;
                m_cp[1] += cp;
                m_rpCenter /= m_nSize;
                m_rpCenter.X += cp.X;
                m_rpCenter.Y += cp.Y;
            }

            public int GetSize(eSort eSort)
            {
                switch (eSort)
                {
                    case eSort.Size: return m_nSize;
                    case eSort.Length: return m_nLength;
                    case eSort.X: return m_sz.X;
                    case eSort.Y: return m_sz.Y;
                }
                return m_nSize; 
            }

            public Island(int x, int y)
            {
                m_nSize = 1;
                m_cp[0] = new CPoint(x, y);
                m_cp[1] = new CPoint(x, y);
                m_rpCenter = new RPoint(x, y);
            }
        }
        List<Island> m_aIsland = new List<Island>();
        #endregion

        #region FindNeighbor
        void FindNeighbor(Island island, int x, int y)
        {
            m_aVisit[y, x].Set(true, x, y); 
            while (true)
            {
                if (x > 0)
                {
                    if ((m_aVisit[y, x - 1].m_bVisit == false) && (m_aDst[y, x - 1] == 255))
                    {
                        FindNeighbor(island, x, y, -1, 0);
                        x--;
                        continue; 
                    }
                }
                if ((x < (p_sz.X - 1)))
                {
                    if ((m_aVisit[y, x + 1].m_bVisit == false) && (m_aDst[y, x + 1] == 255))
                    {
                        FindNeighbor(island, x, y, 1, 0);
                        x++;
                        continue;
                    }
                }
                if (y > 0)
                {
                    if ((m_aVisit[y - 1, x].m_bVisit == false) && (m_aDst[y - 1, x] == 255))
                    {
                        FindNeighbor(island, x, y, 0, -1);
                        y--;
                        continue;
                    }
                }
                if (y < (p_sz.Y - 1))
                {
                    if ((m_aVisit[y + 1, x].m_bVisit == false) && (m_aDst[y + 1, x] == 255))
                    {
                        FindNeighbor(island, x, y, 0, 1);
                        y++;
                        continue;
                    }
                }
                Visit visit = m_aVisit[y, x]; 
                if (visit.IsSame(x, y)) break;
                x = visit.X;
                y = visit.Y; 
            }
        }

        void FindNeighbor(Island island, int x, int y, int dx, int dy)
        {
            int x1 = x + dx;
            int y1 = y + dy; 
            m_aDst[y1, x1] = 100;
            m_aVisit[y1, x1].Set(true, x, y);
            island.m_nSize++;
            island.m_rpCenter.X += x1;
            island.m_rpCenter.Y += y1; 
        }
        #endregion

        #region GetMaxSize, Sort
        public enum eSort
        {
            Size,
            Length,
            X,
            Y
        }
        public List<Island> m_aSort = new List<Island>();

        public int GetMaxSize(eSort eSort)
        {
            if (m_aIsland.Count == 0) return 0;
            RunSort(eSort);
            switch (eSort)
            {
                case eSort.Size: return m_aSort[0].m_nSize;
                case eSort.Length: return m_aSort[0].m_nLength;
                case eSort.X: return m_aSort[0].m_sz.X;
                case eSort.Y: return m_aSort[0].m_sz.Y;
            }
            return m_aSort[0].m_nSize;
        }
        public void RunSort(eSort eSort)
        {
            m_aSort.Clear();
            foreach (Island island in m_aIsland) m_aSort.Add(island); 
            switch (eSort)
            {
                case eSort.Size: m_aSort.Sort(CompareSize); break;
                case eSort.Length: m_aSort.Sort(CompareLength); break;
                case eSort.X: m_aSort.Sort(CompareX); break;
                case eSort.Y: m_aSort.Sort(CompareY); break;
            }
        }

        int CompareSize(Island A, Island B)
        {
            if (A.m_nSize > B.m_nSize) return -1;
            if (A.m_nSize < B.m_nSize) return 1;
            return 0;
        }

        int CompareLength(Island A, Island B)
        {
            if (A.m_nLength > B.m_nLength) return -1;
            if (A.m_nLength < B.m_nLength) return 1;
            return 0;
        }

        int CompareX(Island A, Island B)
        {
            if (A.m_sz.X > B.m_sz.X) return -1;
            if (A.m_sz.X < B.m_sz.X) return 1;
            return 0;
        }

        int CompareY(Island A, Island B)
        {
            if (A.m_sz.Y > B.m_sz.Y) return -1;
            if (A.m_sz.Y < B.m_sz.Y) return 1;
            return 0;
        }
        #endregion

        MemoryData m_memory;
        int m_iMemory; 
        public string RunBlob(MemoryData memory, int nIndex, CPoint cp, CPoint sz, int nGV0, int nGV1, int minSize)
        {
            if (memory == null) return "MemoryData is null"; 
            m_memory = memory;
            m_iMemory = nIndex; 
            m_cp = cp;
            p_sz = sz;
            if (IsRangeOver(m_cp)) return "ROI Range Over";
            if (IsRangeOver(m_cp + p_sz)) return "ROI Range Over";
            m_aIsland.Clear();
            InitData(nGV0, (nGV1 > 0) ? nGV1 : 255);
            for (int y = 0; y < p_sz.Y; y++)
            {
                for (int x = 0; x < p_sz.X; x++)
                {
                    if (m_aDst[y, x] == 255)
                    {
                        m_aDst[y, x] = 100;
                        Island island = new Island(x, y);
                        FindNeighbor(island, x, y);
                        island.Done(m_cp);
                        if (island.m_nSize >= minSize) m_aIsland.Add(island);
                    }
                }
            }
            return "OK";
        }
    }
}
