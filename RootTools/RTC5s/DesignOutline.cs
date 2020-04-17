using System.Drawing;
using System.Drawing.Imaging;

namespace RootTools.RTC5s
{
    public class DesignOutline
    {
        CPoint[] m_cpDir = new CPoint[8]
        {
            new CPoint(1, 0),
            new CPoint(1, 1),
            new CPoint(0, 1),
            new CPoint(-1, 1),
            new CPoint(-1, 0),
            new CPoint(-1, -1),
            new CPoint(0, -1),
            new CPoint(1, -1)
        };

        DataList m_dataList;
        public void MakeOutline(Bitmap bitmap, DataList dataList)
        {
            m_dataList = dataList;
            dataList.Clear();
            if ((bitmap.Width == 0) || (bitmap.Height == 0)) return;
            MakeData(bitmap);
            DeleteThorn();
            MakeLine();
            m_aData = null;
        }

        #region MakeData
        CPoint m_szImg;
        Rectangle m_rect;
        byte[,] m_aData = null;
        unsafe void MakeData(Bitmap bitmap)
        {
            m_szImg = new CPoint(bitmap.Width, bitmap.Height);
            m_rect = new Rectangle(0, 0, m_szImg.X, m_szImg.Y);
            m_aData = new byte[m_szImg.X, m_szImg.Y];
            BitmapData bitmapData = bitmap.LockBits(m_rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
            byte* p = (byte*)bitmapData.Scan0.ToPointer();
            for (int y = 0; y < m_szImg.Y; y++)
            {
                int iy = (m_szImg.Y - y - 1) * m_szImg.X;
                for (int x = 0; x < m_szImg.X; x++)
                {
                    m_aData[x, y] = (p[(x + iy) * 4] == Color.Black.R) ? (byte)0 : (byte)1;
                }
            }
            bitmap.UnlockBits(bitmapData);
        }
        #endregion

        #region Thorn
        void DeleteThorn()
        {
            for (int y = 1; y < m_szImg.Y - 1; y++)
            {
                for (int x = 1; x < m_szImg.X - 1; x++)
                {
                    if (IsThorn(x, y)) DeleteThorn(x, y);
                }
            }
        }

        bool IsThorn(int x, int y)
        {
            int nWhite = 0;
            if (m_aData[x, y] == 0) return false;
            for (int n = 0; n < 8; n++)
            {
                if (m_aData[x + m_cpDir[n].X, y + m_cpDir[n].Y] == 1) nWhite++;
            }
            return nWhite < 2;
        }

        void DeleteThorn(int x, int y)
        {
            m_aData[x, y] = 0;
            for (int n = 0; n < 8; n++)
            {
                if (m_aData[x + m_cpDir[n].X, y + m_cpDir[n].Y] > 0)
                {
                    int xp = x + m_cpDir[n].X;
                    int yp = y + m_cpDir[n].Y;
                    if (IsThorn(xp, yp)) DeleteThorn(xp, yp);
                }
            }
        }
        #endregion

        #region Make Line
        void MakeLine()
        {
            for (int y = 1; y < m_szImg.Y - 1; y++)
            {
                for (int x = 1; x < m_szImg.X - 1; x++)
                {
                    MakeLine(x, y);
                }
            }
        }

        DataList.Data[] m_aLine = new DataList.Data[4];
        double[] m_fL = new double[3];
        void MakeLine(int x, int y)
        {
            if (m_aData[x, y] != 1) return;
            if (IsEdge(x, y) == false) return;
            int nStep = 0;
            int nDir0 = GetDir(x, y, 0);
            m_aLine[nStep] = new DataList.Data(DataList.Data.eCmd.Jump, x, y);
            nStep++;
            while (nDir0 >= 0)
            {
                MakeLine(ref x, ref y, nDir0);
                m_aLine[nStep] = new DataList.Data(DataList.Data.eCmd.Mark, x, y);
                m_fL[nStep - 1] = (m_aLine[nStep - 1] - m_aLine[nStep]).p_L;
                if (nStep < 3) nStep++;
                else if ((m_fL[1] < 1.5) && IsParallel(m_aLine[1] - m_aLine[0], m_aLine[3] - m_aLine[2]))
                {
                    m_aLine[1] += m_aLine[2];
                    m_aLine[1] /= 2;
                    m_aLine[2] = m_aLine[3];
                    m_fL[0] = (m_aLine[0] - m_aLine[1]).p_L;
                    m_fL[1] = (m_aLine[1] - m_aLine[2]).p_L;
                }
                else
                {
                    m_dataList.Add(m_aLine[0]);
                    m_aLine[0] = m_aLine[1];
                    m_aLine[1] = m_aLine[2];
                    m_aLine[2] = m_aLine[3];
                    m_fL[0] = m_fL[1];
                    m_fL[1] = m_fL[2];
                }
                nDir0 = GetDir(x, y, nDir0);
            }
            for (int n = 0; n < nStep; n++) m_dataList.Add(m_aLine[n]);
        }

        bool IsEdge(int x, int y)
        {
            for (int n = 0; n < 8; n += 2)
            {
                if (m_aData[x + m_cpDir[n].X, y + m_cpDir[n].Y] == 0) return true;
            }
            return false;
        }

        int GetDir(int x, int y, int nDir)
        {
            nDir += 5;
            int nDat0 = m_aData[x + m_cpDir[nDir % 8].X, y + m_cpDir[nDir % 8].Y];
            for (int n = 1; n <= 10; n++)
            {
                int nDat = m_aData[x + m_cpDir[(n + nDir) % 8].X, y + m_cpDir[(n + nDir) % 8].Y];
                if ((nDat0 == 0) && (nDat == 1)) return (n + nDir) % 8;
                nDat0 = nDat;
            }
            return -1;
        }

        void MakeLine(ref int x, ref int y, int nDir0)
        {
            x += m_cpDir[nDir0].X;
            y += m_cpDir[nDir0].Y;
            int nDir = GetDir(x, y, nDir0);
            m_aData[x, y] = 2;
            if (nDir == nDir0)
            {
                MakeLine(ref x, ref y, nDir0);
                return;
            }
        }

        public bool IsParallel(DataList.Data data0, DataList.Data data1)
        {
            double a, b, c, abc;
            a = data0.p_L;
            b = data1.p_L;
            abc = data0.m_x * data1.m_x + data0.m_y * data1.m_y;
            c = abc / a / b;
            if (c > 0.8) return true;
            else return false;
        }
        #endregion

        public DesignOutline()
        {
        }
    }
}
