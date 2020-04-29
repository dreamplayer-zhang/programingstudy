using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_LogViewer
{
    public class LogViewer : NotifyProperty
    {
        string _sInfo = ""; 
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                OnPropertyChanged(); 
            }
        }
    }
}
