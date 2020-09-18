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
            get => _nPositionTrigger._value;
            set => _nPositionTrigger._value = value;
        }

        Parameter<int> _nPositionScore;
        public int m_nPositionScore
        {
            get => _nPositionScore._value;
            set => _nPositionScore._value = value;
        }

        public ParamData_Position()
        {
            _nPositionTrigger = new Parameter<int>(0, 0, 500);
            _nPositionScore = new Parameter<int>(50, 0, 100);
        }
        

    }
}
