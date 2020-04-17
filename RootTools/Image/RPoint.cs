using System;

namespace RootTools
{
    public class RPoint
    {
        double _x;
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        double _y;
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public RPoint()
        {
            X = 0;
            Y = 0;
        }

        public RPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public RPoint(CPoint p)
        {
            X = p.X;
            Y = p.Y;
        }

        public RPoint(RPoint p)
        {
            X = p.X;
            Y = p.Y;
        }

        public RPoint(string str, LogWriter log)
        {
            try
            {
                if ((str == null) || (str == "")) return;
                string[] strs = str.Split(',');
                if (strs.Length < 2) return;
                X = Convert.ToDouble(strs[0].Substring(1, strs[0].Length - 1));
                Y = Convert.ToDouble(strs[1].Substring(0, strs[1].Length - 1));
            }
            catch (Exception)
            {
                if (log == null) return;
                log.Warn("new RPoint Error " + str);
            }
        }

        public void Set(RPoint rp)
        {
            X = rp.X; 
            Y = rp.Y;
        }

        public double GetL()
        {
            return Math.Sqrt(X * X + Y * Y); 
        }

        public override string ToString()
        {
            return "(" + X.ToString(".000") + ", " + Y.ToString(".000") + ")";
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false; 
            RPoint cp = (RPoint)obj;
            if (cp.X != X) return false;
            if (cp.Y != Y) return false;
            return true;
        }

        public static bool operator ==(RPoint cp0, RPoint cp1)
        {
            return cp0.Equals(cp1);
        }

        public static bool operator !=(RPoint cp0, RPoint cp1)
        {
            return !cp0.Equals(cp1);
        }

        public static RPoint operator +(RPoint cp0, RPoint cp1)
        {
            return new RPoint(cp0.X + cp1.X, cp0.Y + cp1.Y);
        }

        public static RPoint operator -(RPoint cp0, RPoint cp1)
        {
            return new RPoint(cp0.X - cp1.X, cp0.Y - cp1.Y);
        }

        public static RPoint operator *(RPoint cp0, double f)
        {
            return new RPoint((int)Math.Round(cp0.X * f), (int)Math.Round(cp0.Y * f));
        }

        public static RPoint operator /(RPoint cp0, double f)
        {
            return new RPoint(Math.Round(cp0.X / f), Math.Round(cp0.Y / f));
        }

        public bool IsInside(RPoint cp)
        {
            if (cp.X > X) return false;
            if (cp.Y > Y) return false;
            if (cp.X < 0) return false;
            if (cp.Y < 0) return false;
            return true;
        }

        public void Transpose()
        {
            double n = X;
            X = Y;
            Y = n;
        }
    }
}
