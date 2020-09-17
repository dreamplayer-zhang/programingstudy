using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public class RecipeParameter
    {
        public ParamData_Origin m_ParamData_Origin;

        public RecipeParameter()
        {
            Init();
        }
        public void Init()
        {
            m_ParamData_Origin = new ParamData_Origin();
        }
    }
}
