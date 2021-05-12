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
        ScrewUI_ViewModel screwUITime, screwUIIllum;

        #region Property
        public ScrewUI_ViewModel ScrewUITime
        {
            get => screwUITime;
            set => SetProperty(ref screwUITime, value);
        }
        public ScrewUI_ViewModel ScrewUIIllum
        {
            get => screwUIIllum;
            set => SetProperty(ref screwUIIllum, value);
        }
        #endregion

        public VisionReview_ViewModel(Review_ViewModel reviewVM)
        {
            this.reviewVM = reviewVM;
            screwUITime = new ScrewUI_ViewModel("EIP_Plate.Stack.Front");
            screwUIIllum = new ScrewUI_ViewModel("EIP_Plate.Stack.Front");
        }
    }
}
