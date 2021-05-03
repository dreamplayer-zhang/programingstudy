using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;

namespace Root_VEGA_P_Vision
{
    public class ParticleReview_ViewModel: ObservableObject
    {
        RootViewer_ViewModel domeViewer_VM,doorViewer_VM;
        Review_ViewModel reviewVM;
        public ParticleReview_ViewModel(Review_ViewModel reviewVM)
        {
            this.reviewVM = reviewVM;
            domeViewer_VM = new RootViewer_ViewModel();
            doorViewer_VM = new RootViewer_ViewModel();
        }
    }
}
