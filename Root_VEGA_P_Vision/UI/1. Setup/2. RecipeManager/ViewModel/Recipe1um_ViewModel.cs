using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class Recipe1um_ViewModel:ObservableObject
    {
        public RecipeSetting_ViewModel recipeSetting;
        public Recipe1um_Panel Main;
        public Recipe1um_ViewModel(RecipeSetting_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
            Main = new Recipe1um_Panel();
        }
    }
}
