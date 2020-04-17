using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using System.Drawing;
namespace Root_Wind
{
    class DrawData
    {
        public RectData m_OriginData;
        public List<StringData> m_StringData= new List<StringData>();
        public List<PointData> m_PointData = new List<PointData>();
        public List<RectData> m_RectData = new List<RectData>();
        public List<LineData> m_LineData = new List<LineData>();
        
        public void AddString(string str,CPoint pt,Color c)
        {
            m_StringData.Add(new StringData(str, pt, c));
        }
        public void AddPointData(PointData.PointType PointType, int size, CPoint pt, Color c)
        {
            m_PointData.Add(new PointData(PointType,size, pt, c));
        }
        public void AddRectData(CRect rt, Color c)
        {
            m_RectData.Add(new RectData(rt, c));
        }
        public void AddLineData(CPoint ptS,CPoint ptE,Color c)
        {
            m_LineData.Add(new LineData(ptS,ptE,c));
        }
        public void Clear()
        {
            m_StringData.Clear();
            m_PointData.Clear();
            m_RectData.Clear();
            m_LineData.Clear();
        }

    }

    public class StringData
    {
        public  StringData(string s,CPoint p,Color c)
        {
            m_str = s;
            m_pt = p;
            m_color = c;
        }
        public string m_str;
        public CPoint m_pt;
        public Color m_color;
    }
    public class PointData
    {
        public enum PointType
        {
            Cross,
            Plus
        }
        public CPoint m_pt;
        public int m_size;
        public PointType m_pointtype;
        public Color m_color;
        public PointData(PointType PointType, int size, CPoint p, Color c)
        {
            m_pointtype = PointType;
            m_pt = p;
            m_color = c;
            m_size  = size;
        }
    }
    public class RectData
    {
        public CRect m_rt;
        public Color m_color;
        public RectData(CRect rt, Color c)
        {
            m_rt = rt;
            m_color = c;
        }
    }
    public class LineData
    {
        public CPoint m_ptS;
        public CPoint m_ptE;
        public Color m_color;
        public LineData(CPoint ptS,CPoint ptE,Color c)
        {
            m_ptS = ptS;
            m_ptE = ptE;
            m_color = c;
        }
    }
}
