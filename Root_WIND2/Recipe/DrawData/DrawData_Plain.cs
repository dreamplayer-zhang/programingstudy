using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;


namespace Root_WIND2
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

    class DrawData_Plain
    {
        int nROICount;
        List<DrawData_PlainData> m_EditData_PlainData;
        RecipeData m_RecipeData;
        public DrawData_Plain(RecipeData _recipeData)
        {
            m_RecipeData = _recipeData;
        }

        public void PushPlain(PLAIN_TYPE plaintype, int nRoiCount, List<BasicShape> basicShapes)
        {
            DrawData_PlainData plainData = new DrawData_PlainData(plaintype, nRoiCount, basicShapes);
            m_EditData_PlainData.Add(plainData);
        }

        public void ClearPlain()
        {
            m_EditData_PlainData.Clear();
        }

        public void ApplyPlainData()
        {
            // ToRecipeData
            for(int i = 0; i < m_EditData_PlainData.Count; i ++)
            {
                PLAIN_TYPE type = m_EditData_PlainData[i].GetPlainType();
                DrawData_PlainData plainData = m_EditData_PlainData[i];

                switch (type)
                {
                    case PLAIN_TYPE.ORIGIN:
                        // 그리기 Origin Data를 RecipeData_Origin에 필요한 데이터 맵핑.
                        RecipeData_Origin pOrigin = m_RecipeData.GetRecipeOrigin();
                        //pOrigin.SetOrigin();
                        break;

                    case PLAIN_TYPE.POSITION:
                        break;

                    case PLAIN_TYPE.ALIGN:
                        break;

                    case PLAIN_TYPE.ROI:
                        break;

                    case PLAIN_TYPE.D2D:
                        break;

                    default:
                        break;

                }
            }
        }

    }
}
