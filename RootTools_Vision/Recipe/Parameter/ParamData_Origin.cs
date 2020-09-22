using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;


namespace RootTools_Vision
{
    public class ParamData_Origin : ObservableObject, IParameterData
    {
        Parameter<int> _nOriginWidth;
        public int m_nOriginWidth
        {
            get => _nOriginWidth._value;
            set => _nOriginWidth._value = value;
        }

        Parameter<int> _nOriginHeight;
        public int m_nOriginHeight
        {
            get => _nOriginHeight._value;
            set => _nOriginHeight._value = value;
        }

        

        public ParamData_Origin()
        { 
            _nOriginWidth = new Parameter<int>(3000, 0, 5000);
            _nOriginHeight = new Parameter<int>(3000, 0, 5000);
        }
    }
}
