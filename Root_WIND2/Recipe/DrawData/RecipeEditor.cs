using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{

    class RecipeEditor
    {

        List<DrawData_Plain> m_DrawData_PlainList;
        RecipeData m_RecipeData;
        public RecipeEditor(RecipeData _recipeData)
        {
            m_RecipeData = _recipeData;
        }

        public void PushPlain(PLAIN_TYPE plaintype, int nRoiNumber, List<TShape> basicShapes)
        {
            DrawData_Plain plainData = new DrawData_Plain(plaintype, nRoiNumber, basicShapes);
            m_DrawData_PlainList.Add(plainData);
        }

        public void UpdateRecipe()
        {
            // ToRecipeData
            for (int i = 0; i < m_DrawData_PlainList.Count; i++)
            {
                PLAIN_TYPE type = m_DrawData_PlainList[i].GetPlainType();
                DrawData_Plain plainData = m_DrawData_PlainList[i];

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
