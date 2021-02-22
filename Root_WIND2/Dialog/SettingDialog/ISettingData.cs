using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public interface ISettingData
    {
        void Save();
        void Load(object obj);
    }
}
