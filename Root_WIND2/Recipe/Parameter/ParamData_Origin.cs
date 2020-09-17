using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;


namespace Root_WIND2
{
	public class ParamData_Origin : ObservableObject, IParamData
	{
        Parameter<int> _nOriginWidth;
        Parameter<int> _nOriginHeight;

        public ParamData_Origin()
        { 
            _nOriginWidth = new Parameter<int>(1000, 0, 5000);
            _nOriginHeight = new Parameter<int>(1000, 0, 5000);
        }

        public int m_nOriginWidth
        {
            get { return _nOriginWidth._value;}
            set
            {
                if (_nOriginWidth._value == value)
                    return;
                _nOriginWidth._value = value;
                RaisePropertyChanged();
            }
        }
        public int m_nOriginHeight
        {
            get { return _nOriginHeight._value; }
            set
            {
                if (_nOriginWidth._value == value)
                    return;
                _nOriginHeight._value = value;
                RaisePropertyChanged();
            }
        }




    }
}
