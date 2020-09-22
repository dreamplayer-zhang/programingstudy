using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace RootTools_Vision
{
    public class RecipeInfo : ObservableObject
    {
        public string m_sRecipeName; // 레시피명
        public string m_sEqpID; // 장비명
        public Parameter m_RecipeParam; // 파라미터 데이터
        RecipeInfo_ProductMap m_RecipeInfoMap; // 제품맵
        RecipeInfo_MotionData m_RecipeInfoMotionData; // 모션

        public RecipeInfo()
        {
            m_sRecipeName = "Wind2_Recipe";
            m_sEqpID = "Wind2";
            m_RecipeParam = new Parameter();
        }
    }
}
