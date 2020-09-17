using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace RootTools_Vision
{
    public class ParamData_Position : ObservableObject, IParamData
    {
        Parameter<int> _nPositionTrigger;
        public int m_nPositionTrigger
        {
            get { return _nPositionTrigger._value; }
            set
            {
                if (_nPositionTrigger._value == value) return;
                _nPositionTrigger._value = value;
            }
        }

        public ParamData_Position()
        {
            _nPositionTrigger = new Parameter<int>(0, 0, 100);
        }
        

    }
}
