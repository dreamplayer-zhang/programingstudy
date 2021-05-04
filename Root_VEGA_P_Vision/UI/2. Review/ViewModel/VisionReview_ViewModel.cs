using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using RootTools.Memory;

namespace Root_VEGA_P_Vision
{
    public class VisionReview_ViewModel: ObservableObject
    {
        Review_ViewModel reviewVM;
        screwUI screwUITime;
        screwUI screwUIIllum;
        ScrewUI_ViewModel vm1, vm2;

        public VisionReview_ViewModel(Review_ViewModel reviewVM)
        {
            this.reviewVM = reviewVM;
            screwUITime = new screwUI();
            screwUIIllum = new screwUI();
            vm1 = (ScrewUI_ViewModel)screwUITime.DataContext;
            vm2 = (ScrewUI_ViewModel)screwUIIllum.DataContext;
        }
    }
}
