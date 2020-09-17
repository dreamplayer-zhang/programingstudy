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

        RecipeInfo_ProductMap m_RecipeInfoMap;
        RecipeInfo_MotionData m_RecipeInfoMotionData;

        public RecipeInfo()
        {

        }
    }
}
