using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;


namespace RootTools_Vision
{
    public enum PLAIN_TYPE
    {
        ORIGIN,
        POSITION,
        ALIGN,
        ROI,
        SURFACE,
        D2D,
    }

    public class DrawData_Plain
    {
        int m_nRoiNumber;
        PLAIN_TYPE m_PlainType;
        List<TShape> m_Object; // ROI 단위

        public DrawData_Plain(PLAIN_TYPE _plaintype, int _nRoiNumber, List<TShape> basicShapes)
        {
            m_nRoiNumber = _nRoiNumber;
            m_PlainType = _plaintype;
            m_Object = basicShapes;
        }

        public List<TShape> GetObject()
        {
            return m_Object;
        }

        public PLAIN_TYPE GetPlainType()
        {
            return m_PlainType;
        }

        public int GetRoiCount()
        {
            return m_nRoiNumber;
        }
    }
}
