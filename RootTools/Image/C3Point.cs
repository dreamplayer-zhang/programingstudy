using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools
{
    public class C3Point
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

        int _z;
        public int Z
        {
            get { return _z; }
            set { _z = value; }
        }

        public C3Point()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public C3Point(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public C3Point(C3Point p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        public C3Point(string str, Log log)
        {
            try
            {
                if ((str == null) || (str == "")) return;
                string[] strs = str.Split(',');
                if (strs.Length < 3) return;
                X = Convert.ToInt32(strs[0].Substring(1, strs[0].Length - 1));
                Y = Convert.ToInt32(strs[1].Substring(0, strs[1].Length - 1));
                Z = Convert.ToInt32(strs[2].Substring(0, strs[2].Length - 1));
            }
            catch (Exception)
            {
                if (log == null) return;
                log.Warn("new RPoint Error " + str);
            }
        }

        public void Set(C3Point rp)
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
            C3Point cp = (C3Point)obj;
            if (cp.X != X) return false;
            if (cp.Y != Y) return false;
            if (cp.Z != Z) return false;
            return true;
        }

        public static bool operator ==(C3Point cp0, C3Point cp1)
        {
            cp0 = cp0 ?? new C3Point();
            return cp0.Equals(cp1);
        }

        public static bool operator !=(C3Point cp0, C3Point cp1)
        {
            cp0 = cp0 ?? new C3Point();
            return !cp0.Equals(cp1);
        }

        public static C3Point operator +(C3Point cp0, C3Point cp1)
        {
            return new C3Point(cp0.X + cp1.X, cp0.Y + cp1.Y, cp0.Z + cp1.Z);
        }

        public static C3Point operator -(C3Point cp0, C3Point cp1)
        {
            return new C3Point(cp0.X - cp1.X, cp0.Y - cp1.Y, cp0.Z - cp1.Z);
        }

        public static C3Point operator *(C3Point cp0, double f)
        {
            return new C3Point((int)(cp0.X * f), (int)(cp0.Y * f), (int)(cp0.Z * f));
        }

        public static C3Point operator /(C3Point cp0, double f)
        {
            return new C3Point((int)(cp0.X / f), (int)(cp0.Y / f), (int)(cp0.Z / f));
        }

    }
}
