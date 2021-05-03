using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class Recipe6um_ViewModel:ObservableObject
    {
        RecipeSetting_ViewModel recipeSetting;
        public Recipe6um_Panel Main;

        public Recipe6um_ViewModel(RecipeSetting_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
            Main = new Recipe6um_Panel();
        }
    }
}
