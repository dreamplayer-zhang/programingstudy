using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RootTools;
using RootTools.Memory;

namespace Root_VEGA_P_Vision
{
    public class VisionReview_ViewModel: ObservableObject
    {
        public VisionReview_Panel Main;
        ScrewUI_ViewModel screwUITime, screwUIIllum;
        RootViewer_ViewModel defectImage, reviewImage;

        #region Property
        public RootViewer_ViewModel DefectImage
        {
            get => defectImage;
            set => SetProperty(ref defectImage, value);
        }
        public RootViewer_ViewModel ReviewImage
        {
            get => reviewImage;
            set => SetProperty(ref reviewImage, value);
        }
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

        public VisionReview_ViewModel()
        {
            Main = new VisionReview_Panel();
            screwUITime = new ScrewUI_ViewModel("EIP_Plate.Stack.Front");
            screwUIIllum = new ScrewUI_ViewModel("EIP_Plate.Stack.Front");
            reviewImage = new RootViewer_ViewModel();
            defectImage = new RootViewer_ViewModel();
        }

        #region RelayCommand
        ICommand btnSearch
        {
            get => new RelayCommand(()=> { });
        }
        #endregion
    }
}
