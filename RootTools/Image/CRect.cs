using System;
using System.Xml.Serialization;

namespace RootTools
{
    public class CRect
    {
        [XmlIgnore]
        public int X
        {
            get 
            {
                return Left + Convert.ToInt32((Width / 2.0));
            }
        }

        [XmlIgnore]
        public int Y
        {
            get
            {
                return Top + Convert.ToInt32((Height / 2.0));
            }
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
                return Math.Abs(Right - Left);
            }
        }
        [XmlIgnore]
        public int Height
        {
            get
            {
                return Math.Abs(Bottom - Top);
            }
        }
        public CRect()
        {
            Left = 0;
            Top = 0;
            Right = 0;
            Bottom = 0;
        }
        public CRect(System.Windows.Point startPoint, System.Windows.Point endPoint)
		{
            Left = (int)startPoint.X;
            Top = (int)startPoint.Y;
            Right = (int)endPoint.X;
            Bottom = (int)endPoint.Y;
        }

        public CRect(int cenx, int ceny, int size)
        {
            Left = cenx - size / 2;
            Right = cenx + size / 2;
            Top = ceny - size / 2;
            Bottom = ceny + size / 2;
        }

        public CRect(CPoint p, int width, int height)
        {
            Left = p.X - width / 2;
            Right = p.X + width / 2;
            Top = p.Y - height / 2;
            Bottom = p.Y + height / 2;
        }

        public CRect(RPoint p, int width, int height)
        {
            Left = Convert.ToInt32(p.X) - width / 2;
            Right = Convert.ToInt32(p.X) + width / 2;
            Top = Convert.ToInt32(p.Y) - height / 2;
            Bottom = Convert.ToInt32(p.Y) + height / 2;
        }
		public CRect(int l, int t, int r, int b)
		{
			Left = l;
			Right = r;
			Top = t;
			Bottom = b;

			if (l > r)
			{
				Left = r;
				Right = l;
			}
			if (b < t)
			{
				Bottom = t;
				Top = b;
            }
        }
        /// <summary>
        /// Top,Left,Bottom,Right를 위치에 맞게 재정렬한다
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static CRect ReAllocate(CRect rect)
		{
            CRect result = new CRect();

            result.Left = rect.Left;
            result.Right = rect.Right;
            result.Top = rect.Top;
            result.Bottom = rect.Bottom;

            if (rect.Left > rect.Right)
            {
                result.Left = rect.Right;
                result.Right = rect.Left;
            }
            if (rect.Bottom < rect.Top)
            {
                result.Bottom = rect.Top;
                result.Top = rect.Bottom;
            }

            return result;
        }
        public CRect(string str, Log log)
        {
            try
            {
                if ((str == null) || (str == "")) return;
                string[] strs = str.Split(',');
                if (strs.Length < 2) return;
            }
            catch (Exception)
            {
                if (log == null) return;
                log.Warn("new CPoint Error " + str);
            }
        }

        public void Set(CRect rt)
        {
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
