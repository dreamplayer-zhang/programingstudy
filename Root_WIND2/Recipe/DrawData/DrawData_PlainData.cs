using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{
    class DrawData_PlainData
    {
        int m_nRoiCount;
        PLAIN_TYPE m_PlainType;
        List<BasicShape> m_Object; // ROI 단위

        public DrawData_PlainData(PLAIN_TYPE _plaintype, int _nROICount, List<BasicShape> basicShapes)
        {
            m_nRoiCount = _nROICount;
            m_PlainType = _plaintype;
            m_Object = basicShapes;
        }

        public PLAIN_TYPE GetPlainType()
        {
            return m_PlainType;
        }

        public int GetRoiCount()
        {
            return m_nRoiCount;
        }

    }

}
