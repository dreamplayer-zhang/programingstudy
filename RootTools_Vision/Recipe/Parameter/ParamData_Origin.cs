using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;


namespace RootTools_Vision
{
    public class ParamData_Origin : ObservableObject, IParamData
    {
        //int temp;
        //public int Temp { get => temp; set => temp = value; }
        
        Parameter<int> _nOriginWidth;
        public int m_nOriginWidth
        {
            get { return _nOriginWidth._value; }
            set
            {
                if (_nOriginWidth._value == value) return;
                _nOriginWidth._value = value;
            }
        }

        Parameter<int> _nOriginHeight;
        public int m_nOriginHeight
        {
            get { return _nOriginHeight._value; }
            set
            {
                if (_nOriginWidth._value == value) return;
                _nOriginHeight._value = value;
            }
        }

        

        public ParamData_Origin()
        { 
            _nOriginWidth = new Parameter<int>(6000, 0, 5000);
            _nOriginHeight = new Parameter<int>(1000, 0, 5000);
        }
    }
}
