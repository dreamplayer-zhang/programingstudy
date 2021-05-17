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
        public ParticleReview_Panel Main;
        RootViewer_ViewModel domeViewer_VM,doorViewer_VM;
        #region Property
        public RootViewer_ViewModel DomeViewer
        {
            get => domeViewer_VM;
            set => SetProperty(ref domeViewer_VM, value);
        }
        public RootViewer_ViewModel DoorViewer
        {
            get => doorViewer_VM;
            set => SetProperty(ref doorViewer_VM, value);
        }
        #endregion
        public ParticleReview_ViewModel()
        {
            Main = new ParticleReview_Panel();
            domeViewer_VM = new RootViewer_ViewModel();
            doorViewer_VM = new RootViewer_ViewModel();
        }
    }
}
