﻿using System;
using System.Xml.Serialization;

namespace RootTools
{
    public class CRect
    {
        int _x;
        [XmlIgnore]
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        int _y;
        [XmlIgnore]
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }
        int _left;
        public int Left
        {
            get { return _left; }
            set { _left = value; }
        }
        int _top;
        public int Top
        {
            get { return _top; }
            set { _top = value; }
        }
        int _right;
        public int Right
        {
            get { return _right; }
            set { _right = value; }
        }
        int _bottom;
        public int Bottom
        {
            get { return _bottom; }
            set { _bottom = value; }
        }

        [XmlIgnore]
        public int Width
        {
            get
            {
                return Right - Left;
            }
            set
            {
                Right = value + Left;
            }
        }
        [XmlIgnore]
        public int Height
        {
            get
            {
                return Bottom - Top;
            }
            set
            {
                Bottom = value + Top;
            }
        }
        public CRect()
        {
            Left = 0;
            Top = 0;
            Right = 0;
            Bottom = 0;
            X = 0;
            Y = 0;
        }

        public CRect(int cenx, int ceny, int size)
        {
            X = cenx;
            Y = ceny;
            Left = X - size / 2;
            Right = X + size / 2;
            Top = Y - size / 2;
            Bottom = Y + size / 2;
        }

        public CRect(CPoint p, int width, int height)
        {
            X = p.X;
            Y = p.Y;
            Left = X - width / 2;
            Right = X + width / 2;
            Top = Y - height / 2;
            Bottom = Y + height / 2;
        }

        public CRect(RPoint p, int width, int height)
        {
            X = (int)Math.Round(p.X);
            Y = (int)Math.Round(p.Y);
            Left = X - width / 2;
            Right = X + width / 2;
            Top = Y - height / 2;
            Bottom = Y + height / 2;
        }
        public CRect(int l,int t,int r,int b)
        {
            X = (r - l / 2);
            Y = (b - t / 2);
            Left = l;
            Right = r;
            Top = t;
            Bottom = b;
        }
        public CRect(string str, Log log)
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

        public void Set(CRect rt)
        {
            X = rt.X;
            Y = rt.Y;
            Left = rt.Left;
            Right = rt.Right;
            Top = rt.Top;
            Bottom = rt.Bottom;
        }

        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ", " + Left.ToString() + ", " + Top.ToString() + ", " + Right.ToString() + ", " + Bottom.ToString() + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            CRect rt = (CRect)obj;
            if (rt.X != X) return false;
            if (rt.Y != Y) return false;
            if (rt.Left != Left) return false;
            if (rt.Top != Top) return false;
            if (rt.Right != Right) return false;
            if (rt.Bottom != Bottom) return false;

            return true;
        }

        public static bool operator ==(CRect cp0, CRect cp1)
        {
            return cp0.Equals(cp1);
        }

        public static bool operator !=(CRect cp0, CRect cp1)
        {
            return !cp0.Equals(cp1);
        }

        public static CRect operator +(CRect rt, CPoint cp)
        {
            return new CRect(rt.Left + cp.X, rt.Top + cp.Y, rt.Right + cp.X, rt.Bottom + cp.Y);
        }

        public static CRect operator -(CRect rt, CPoint cp)
        {
            return new CRect(rt.Left - cp.X, rt.Top - cp.Y, rt.Right - cp.X, rt.Bottom - cp.Y);
        }
        public bool IsInside(CPoint rt)
        {
            if (rt.X > Right) return false;
            if (rt.Y > Bottom) return false;
            if (rt.X < Left) return false;
            if (rt.Y < Top) return false;
            return true;
        }
        public CPoint Center()
        {
            return new CPoint(X, Y);
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}
