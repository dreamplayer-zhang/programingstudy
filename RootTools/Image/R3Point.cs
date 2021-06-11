using System;

namespace RootTools
{
    public class R3Point
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

        double _z;
        public double Z
        {
            get { return _z; }
            set { _z = value; }
        }

        public R3Point()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public R3Point(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = Z; 
        }

        public R3Point(R3Point p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z; 
        }

        public R3Point(string str, Log log)
        {
            try
            {
                if ((str == null) || (str == "")) return;
                string[] strs = str.Split(',');
                if (strs.Length < 3) return;
                X = Convert.ToDouble(strs[0].Substring(1, strs[0].Length - 1));
                Y = Convert.ToDouble(strs[1].Substring(0, strs[1].Length - 1));
                Z = Convert.ToDouble(strs[2].Substring(0, strs[2].Length - 1));
            }
            catch (Exception)
            {
                if (log == null) return;
                log.Warn("new RPoint Error " + str);
            }
        }

        public void Set(R3Point rp)
        {
            X = rp.X;
            Y = rp.Y;
            Z = rp.Z; 
        }

        public double GetL()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public override string ToString()
        {
            return "(" + X.ToString(".000") + ", " + Y.ToString(".000") + ", " + Z.ToString(".000") + ")";
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            R3Point cp = (R3Point)obj;
            if (cp.X != X) return false;
            if (cp.Y != Y) return false;
            if (cp.Z != Z) return false;
            return true;
        }

        public static bool operator ==(R3Point cp0, R3Point cp1)
        {
            cp0 = cp0 ?? new R3Point();
            return cp0.Equals(cp1);
        }

        public static bool operator !=(R3Point cp0, R3Point cp1)
        {
            cp0 = cp0 ?? new R3Point();
            return !cp0.Equals(cp1);
        }

        public static R3Point operator +(R3Point cp0, R3Point cp1)
        {
            return new R3Point(cp0.X + cp1.X, cp0.Y + cp1.Y, cp0.Z + cp1.Z);
        }

        public static R3Point operator -(R3Point cp0, R3Point cp1)
        {
            return new R3Point(cp0.X - cp1.X, cp0.Y - cp1.Y, cp0.Z - cp1.Z);
        }

        public static R3Point operator *(R3Point cp0, double f)
        {
            return new R3Point(cp0.X * f, cp0.Y * f, cp0.Z * f);
        }

        public static R3Point operator /(R3Point cp0, double f)
        {
            return new R3Point(cp0.X / f, cp0.Y / f, cp0.Z / f);
        }
    }
}
