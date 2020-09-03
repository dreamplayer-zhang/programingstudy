using RootTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WIND2
{
    class RecipeData_Origin : DrawRecipe, IRecipeData
    {

        CRect rtOrigin;

        public RecipeData_Origin()
        {
        }
        public void SetOrigin(List<BasicShape> _basicShapes)
        {
            SetData(_basicShapes);
            // Origin Rect에 대한 데이터 입력
        }

        #region Recipe Interface
        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
