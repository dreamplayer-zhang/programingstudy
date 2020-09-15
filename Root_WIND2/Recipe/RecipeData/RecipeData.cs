using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    /// <summary>
    /// Edit 클래스에서 파생된 데이터를 검사 데이터로 옮김.
    /// 이 클래스는 Recipe의 하부로 들어감
    /// </summary>
    public class RecipeData
    {
        RecipeData_Origin m_ReicpeData_Origin;

        public RecipeData()
        {
            m_ReicpeData_Origin = new RecipeData_Origin();
        }


        public ref RecipeData_Origin GetRecipeOrigin() { return ref m_ReicpeData_Origin; }

    }
}
