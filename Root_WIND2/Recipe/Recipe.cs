using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_WIND2
{
    /// <summary>
    /// Recipe 각 항목의 묶음.
    /// Recipe에 대한 기능 자체는 Manager에서 다룸.
    /// </summary>
    public class Recipe
    {
        // ORIGIN, Die Pitch
        // WAFER MAP
        // POSITION
        // ROI (SURFACE, D2D)
        // INSPECTION PARAMETER

        RecipeInfo m_RecipeInfo; // 레시피 정보 데이터
        RecipeEditor m_RecipeEditor; // 그리기 데이터
        RecipeData m_ReicpeData; // ROI 정보 데이터
        RecipeParameter m_RecipeParam; // 파라미터 데이터

        public Recipe()
        {
            Init();
        }

        public void Init()
        {
            m_ReicpeData = new RecipeData();
            m_RecipeInfo = new RecipeInfo();
            m_RecipeParam = new RecipeParameter();
            m_RecipeEditor = new RecipeEditor(m_ReicpeData);
        }
        

        public ref RecipeData GetRecipeData() { return ref m_ReicpeData; }
        public ref RecipeInfo GetRecipeInfo() { return ref m_RecipeInfo; }
        public ref RecipeParameter GetRecipeParameter() { return ref m_RecipeParam; }
        public ref RecipeEditor GetRecipeEditor() { return ref m_RecipeEditor; }

    }
}
