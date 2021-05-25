using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class ResultSummary_ViewModel:ObservableObject
    {
        public ResultSummary_Panel Main;
        
        public ResultSummary_ViewModel()
        {
            Main = new ResultSummary_Panel();
        }
    }
}
