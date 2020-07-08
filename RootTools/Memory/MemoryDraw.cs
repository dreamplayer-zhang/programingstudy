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
            public Brush m_brush;
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
        }

        public void AddPolyline(List<int> aList, Brush brush)
        {
            Lines lines = new Lines();
            lines.m_brush = brush; 
            for (int x = 0; x < aList.Count; x++)
            {
                CPoint cp = new CPoint(x, aList[x]);
                lines.m_aPoint.Add(cp); 
            }
            m_aDraw.Add(lines); 
        }

        public void AddPolyline(List<int> aList, int dy, Brush brush)
        {
            Lines lines = new Lines();
            lines.m_brush = brush;
            for (int x = 0; x < aList.Count; x++)
            {
                CPoint cp = new CPoint(x, aList[x] + dy);
                lines.m_aPoint.Add(cp);
            }
            m_aDraw.Add(lines);
        }

        public void AddPolyline(List<CPoint> aList, Brush brush)
        {
            Lines lines = new Lines();
            lines.m_brush = brush;
            foreach (CPoint cp in aList) lines.m_aPoint.Add(cp);
            m_aDraw.Add(lines);
        }
        #endregion

        MemoryData m_memory; 
        public MemoryDraw(MemoryData memory)
        {
            m_memory = memory; 
        }
    }
}
