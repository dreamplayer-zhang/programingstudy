using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{

    public class RecipeEditor
    {

        List<DrawData_Plain> m_DrawData_PlainList;
        RecipeData m_RecipeData;
        public RecipeEditor(RecipeData _recipeData)
        {
            m_RecipeData = _recipeData;
            m_DrawData_PlainList = new List<DrawData_Plain>();

        }
        public void PushPlain(DrawData_Plain _plain)
        {
            m_DrawData_PlainList.Add(_plain);
        }

        public void ClearPlain()
        {
            m_DrawData_PlainList.Clear();
        }


        public void PushPlain(PLAIN_TYPE plaintype, int nRoiNumber, List<TShape> basicShapes)
        {
            DrawData_Plain plainData = new DrawData_Plain(plaintype, nRoiNumber, basicShapes);
            m_DrawData_PlainList.Add(plainData);
        }

        public void SaveEditData()
        {
            // Recipe 도형정보 Save.
            int nX = 0;
            int nY = 0;
            int length = 1;

        }

        public void LoadEditData()
        {

        }



        public void UpdateRecipe()
        {
            // ToRecipeData
            for (int i = 0; i < m_DrawData_PlainList.Count; i++)
            {
                PLAIN_TYPE type = m_DrawData_PlainList[i].GetPlainType();
                DrawData_Plain plainData = m_DrawData_PlainList[i];
                List<TShape> shape;

                switch (type)
                {
                    case PLAIN_TYPE.ORIGIN:
                        // 그리기 Origin Data를 RecipeData_Origin에 필요한 데이터 맵핑.
                        RecipeData_Origin pOrigin = m_RecipeData.GetRecipeOrigin();
                        shape = plainData.GetObject();

                        TRect rect = shape[0] as TRect;
                        CPoint ptOrigin = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
                        //pOrigin.SetOrigin(ptOrigin, rect.MemoryRect.Right, rect.MemoryRect.Bottom);
                        pOrigin.SetOrigin(rect.MemoryRect);

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
