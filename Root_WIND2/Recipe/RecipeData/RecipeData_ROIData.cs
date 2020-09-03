using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{

    /// <summary>
    /// 도형을 BasicShape 단위로 관리.
    /// 한개의 ROI를 표현하는 단위 클래스.
    /// </summary>
    public class RecipeData_ROIData
    {
        //BasicShape 
        //Point
        //Line
        // Circle
        // Rect
        // NONPATERN
        int m_nROINumber;
        protected List<BasicShape> m_ObjectList; // Object List

        public RecipeData_ROIData(int _nROINumber)
        {
            m_nROINumber = _nROINumber;
        }

        public void SetROI(List<BasicShape> basicShapes)
        {
            m_ObjectList = basicShapes;
        }

        public List<BasicShape> GetROI()
        {
            return m_ObjectList;
        }

        public BasicShape GetObject(int nIndex)
        {
            return m_ObjectList[nIndex];
        }

        public int GetObjectNumber()
        {
            return m_ObjectList.Count;
        }
    }
}
