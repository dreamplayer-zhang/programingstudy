using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class RecipeSide_ViewModel:ObservableObject
    {
        public RecipeSide_Panel Main;
        RecipeSetting_ViewModel recipeSetting;
        RecipeSideImageViewers_ViewModel EIPCoverViewers, EIPBaseViewers;
        public RecipeSide_ViewModel(RecipeSetting_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
            EIPCoverViewers = new RecipeSideImageViewers_ViewModel();
            EIPBaseViewers = new RecipeSideImageViewers_ViewModel();

            Main = new RecipeSide_Panel();
        }
    }
}
