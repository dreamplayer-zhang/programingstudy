using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools.Memory
{
    public class MemoryDraw
    {
        #region IDraw
        interface IDraw
        {
            void Draw(Grid grid, CPoint cpOffset, double fZoom);
        }
        List<IDraw> m_aDraw = new List<IDraw>();
        public void Clear()
        {
            m_aDraw.Clear();
        }

        public void InvalidDraw()
        {
            m_memory.m_group.m_pool.m_viewer.InvalidDraw();
        }

        public void Draw(Grid grid, CPoint cpOffset, double fZoom)
        {
            grid.Children.Clear();
            foreach (IDraw draw in m_aDraw) draw.Draw(grid, cpOffset, fZoom);
        }
        #endregion

        #region Polyline
        class Lines : IDraw
        {
            Brush m_brush;
            public List<CPoint> m_aPoint = new List<CPoint>();

            public void Draw(Grid grid, CPoint cpOffset, double fZoom)
            {
                Polyline polyline = new Polyline();
                polyline.Stroke = m_brush;
                polyline.StrokeThickness = 1;
                foreach (CPoint cp in m_aPoint)
                {
                    Point p = new Point();
                    p.X = (cp.X - cpOffset.X) * fZoom;
                    p.Y = (cp.Y - cpOffset.Y) * fZoom;
                    polyline.Points.Add(p);
                }
                grid.Children.Add(polyline);
            }

            public Lines(Brush brush)
            {
                m_brush = brush; 
            }
        }

        public void AddPolyline(Brush brush, List<int> aList)
        {
            if (aList == null) return; 
            Lines lines = new Lines(brush);
            for (int x = 0; x < aList.Count; x++)
            {
                CPoint cp = new CPoint(x, aList[x]);
                lines.m_aPoint.Add(cp);
            }
            m_aDraw.Add(lines);
        }

        public void AddPolyline(Brush brush, List<int> aList, int yOffset, double fScale = 1)
        {
            if (aList == null) return;
            Lines lines = new Lines(brush);
            for (int x = 0; x < aList.Count; x++)
            {
                CPoint cp = new CPoint(x, (int)(fScale * aList[x] + yOffset));
                lines.m_aPoint.Add(cp);
            }
            m_aDraw.Add(lines);
        }

        public void AddPolyline(Brush brush, List<CPoint> aList)
        {
            if (aList == null) return;
            Lines lines = new Lines(brush);
            foreach (CPoint cp in aList) lines.m_aPoint.Add(cp);
            m_aDraw.Add(lines);
        }

        public void AddPolyline(Brush brush, params CPoint[] aList)
        {
            if (aList == null) return;
            Lines lines = new Lines(brush);
            foreach (CPoint cp in aList) lines.m_aPoint.Add(cp);
            m_aDraw.Add(lines);
        }

        public void AddCross(Brush brush, CPoint cp, int nSize)
        {
            AddLine(brush, new CPoint(cp.X - nSize, cp.Y), new CPoint(cp.X + nSize, cp.Y));
            AddLine(brush, new CPoint(cp.X, cp.Y - nSize), new CPoint(cp.X, cp.Y + nSize));
        }

        public void AddLine(Brush brush, CPoint cp0, CPoint cp1)
        {
            Lines lines = new Lines(brush);
            lines.m_aPoint.Add(cp0);
            lines.m_aPoint.Add(cp1);
            m_aDraw.Add(lines);
        }
        #endregion

        #region Text
        class Text : IDraw
        {
            Brush m_brush;
            string m_sLabel;
            CPoint m_cp;

            public void Draw(Grid grid, CPoint cpOffset, double fZoom)
            {
                Label label = new Label();
                label.Foreground = m_brush;
                label.Content = m_sLabel;
                Point p = new Point();
                p.X = (m_cp.X - cpOffset.X) * fZoom;
                p.Y = (m_cp.Y - cpOffset.Y) * fZoom;
                label.Margin = new Thickness(p.X, p.Y, 0, 0); 
                grid.Children.Add(label);
            }

            public Text(Brush brush, CPoint cp, string sLabel)
            {
                m_brush = brush;
                m_sLabel = sLabel;
                m_cp = cp; 
            }
        }

        public void AddText(Brush brush, CPoint cp, string sLabel)
        {
            Text text = new Text(brush, cp, sLabel);
            m_aDraw.Add(text); 
        }
        #endregion

        MemoryData m_memory;
        public MemoryDraw(MemoryData memory)
        {
            m_memory = memory;
        }
    }
}
