using System;
using System.Windows;

namespace RootTools
{
    [Serializable]
    public class CPoint
    {
        int _x;
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        int _y;
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public CPoint()
        {
            X = 0;
            Y = 0;
        }

        /// <summary>
        /// Left/Top Point 생성
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public CPoint(Rect rect)
        {
            X = (int)(rect.Left);
            Y = (int)(rect.Top);
        }

        public CPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public CPoint(CPoint p)
        {
            X = p.X;
            Y = p.Y;
        }

        public CPoint(RPoint p)
        {
            X = (int)Math.Round(p.X);
            Y = (int)Math.Round(p.Y);
        }

        public CPoint(string str, Log log)
        {
            try
            {
                if ((str == null) || (str == "")) return;
                string[] strs = str.Split(',');
                if (strs.Length < 2) return;
                X = Convert.ToInt32(strs[0].Substring(1, strs[0].Length - 1));
                Y = Convert.ToInt32(strs[1].Substring(0, strs[1].Length - 1));
            }
            catch (Exception)
            {
                if (log == null) return;
                log.Warn("new CPoint Error " + str); 
            }
        }

        public CPoint(Point pos)
        {
            X = (int)pos.X;
            Y = (int)pos.Y;
        }

        public void Set(CPoint cp)
        {
            X = cp.X;
            Y = cp.Y; 
        }

        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ")";
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false; 
            CPoint cp = (CPoint)obj;
            if (cp.X != X) return false;
            if (cp.Y != Y) return false;
            return true;
        }

        public static bool operator ==(CPoint cp0, CPoint cp1)
        {
            if (cp0 is null && cp1 is null) 
                return true;

            cp0 = cp0 ?? new CPoint();  
            return cp0.Equals(cp1);
        }

        public static bool operator !=(CPoint cp0, CPoint cp1)
        {
            if (cp0 is null && cp1 is null)
                return true;

            cp0 = cp0 ?? new CPoint();
            return !cp0.Equals(cp1);
        }

        public static CPoint operator +(CPoint cp0, CPoint cp1)
        {
            return new CPoint(cp0.X + cp1.X, cp0.Y + cp1.Y);
        }

        public static CPoint operator -(CPoint cp0, CPoint cp1)
        {
            return new CPoint(cp0.X - cp1.X, cp0.Y - cp1.Y);
        }

        public static CPoint operator *(CPoint cp0, double f)
        {
            return new CPoint((int)Math.Round(cp0.X * f), (int)Math.Round(cp0.Y * f));
        }

        public static CPoint operator /(CPoint cp0, double f)
        {
            return new CPoint((int)Math.Round(cp0.X / f), (int)Math.Round(cp0.Y / f));
        }

        public bool IsInside(CPoint cp)
        {
            if (cp.X > X) return false;
            if (cp.Y > Y) return false;
            if (cp.X < 0) return false;
            if (cp.Y < 0) return false;
            return true; 
        }
        public double Distance(CPoint cp)
        {
            return Math.Sqrt((X - cp.X) * (X - cp.X) + (Y - cp.Y) * (Y - cp.Y));
        }
        public void Transpose()
        {
            int n = X;
            X = Y;
            Y = n;
        }
    }
}
