using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{
    public enum Point_Type
    {
        // 임시
        RECT = 1,
        LINE,
        POLYGON,
        POINTLIST,  // AREA
    }

    /// <summary>
    /// RECIPE별 ROI 총괄
    /// RecipeData_ROIData 단위로 ROI 관리.
    /// BYTE ROI및 기타 ROI 단위의 동작
    /// </summary>

    public class RecipeData_ROI
    {
        //RecipeData_ROIData[] m_ROIList;
        const int nTotalROICount = 30;
        List<RecipeData_ROIData> m_ROIList;

        public RecipeData_ROI()
        {
            for(int i = 0; i < nTotalROICount; i ++)
            {
                m_ROIList[i] = new RecipeData_ROIData(i);
            }
        }

        public void SetROI(int nRoiNumber, List<BasicShape> basicShapes)
        {
            m_ROIList[nRoiNumber].SetROI(basicShapes);
        }

        public List<BasicShape> GetROI(int nRoiNumber)
        {
            return m_ROIList[nRoiNumber].GetROI();
        }

    }
}
