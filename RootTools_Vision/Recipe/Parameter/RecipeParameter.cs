using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class RecipeParameter
    {
        public ParamData_Origin m_ParamData_Origin;
        public ParamData_Position m_ParamData_Position;

        public RecipeParameter()
        {
            Init();
        }
        public void Init()
        {
            m_ParamData_Origin = new ParamData_Origin();
            m_ParamData_Position = new ParamData_Position();
        }
    }
}
