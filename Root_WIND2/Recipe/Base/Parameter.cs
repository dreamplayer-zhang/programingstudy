using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public class Parameter<T> where T : IComparable
    {
        T __value;
        T _min;
        T _max;
        public T _value
        {
            get { return __value; }
            set
            {
                if (_min.CompareTo(value) > 0)
                {
                    __value = _min;
                    return;
                }
                if (_max.CompareTo(value) < 0)
                {
                    __value = _max;
                    return;
                }
                __value = value;
            }
        }
        public Parameter(T Value, T Min, T Max)
        {
            _min = Min;
            _max = Max;
            _value = Value;
        }
    }

}
