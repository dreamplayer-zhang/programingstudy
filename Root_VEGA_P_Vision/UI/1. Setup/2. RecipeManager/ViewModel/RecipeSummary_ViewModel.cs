using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class RecipeSummary_ViewModel: ObservableObject
    {
        public RecipeSummary Main;
        RecipeSetting_ViewModel recipeSetting;
        public RecipeSummary_ViewModel(RecipeSetting_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
        }
    }
}
