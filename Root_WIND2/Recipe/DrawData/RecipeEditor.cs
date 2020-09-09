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
        DrawData_Plain m_DrawData_Plain;
        public RecipeEditor(RecipeData _recipeData)
        {
            SetPlain(_recipeData);
        }

        private void SetPlain(RecipeData _recipeData)
        {
            m_DrawData_Plain = new DrawData_Plain(_recipeData);
        }

        public void PushPlain(PLAIN_TYPE plaintype, int nRoiCount, List<BasicShape> basicShapes)
        {
            m_DrawData_Plain.PushPlain(plaintype, nRoiCount, basicShapes);
        }

        public void UpdateRecipe()
        {
            m_DrawData_Plain.ApplyPlainData();
        }
    }
}
