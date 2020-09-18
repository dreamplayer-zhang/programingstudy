using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    /// <summary>
    /// Edit 클래스에서 파생된 데이터를 검사 데이터로 옮김.
    /// 이 클래스는 Recipe의 하부로 들어감
    /// </summary>
    public class RecipeData
    {
        public RecipeData_Origin m_ReicpeData_Origin;
        public RecipeData_Position m_RecipeData_Position;
        
        public RecipeData()
        {
            m_ReicpeData_Origin = new RecipeData_Origin();

            int nCount = Enum.GetNames(typeof(Position_Type)).Length;
            m_RecipeData_Position = new RecipeData_Position(nCount);
        }

        public ref RecipeData_Origin GetRecipeOrigin() { return ref m_ReicpeData_Origin; }

    }
}
