using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
namespace Root_VEGA_P_Vision
{
    public class RecipeSideImageViewers_ViewModel:ObservableObject
    {
        RootViewer_ViewModel top_ViewerVM, bottom_ViewerVM, left_ViewerVM, right_ViewerVM;

        #region Property
        RootViewer_ViewModel Top_ViewerVM
        {
            get => top_ViewerVM;
            set => SetProperty(ref top_ViewerVM, value);
        }
        RootViewer_ViewModel Bottom_ViewerVM
        {
            get => bottom_ViewerVM;
            set => SetProperty(ref bottom_ViewerVM, value);
        }
        RootViewer_ViewModel Left_ViewerVM
        {
            get => left_ViewerVM;
            set => SetProperty(ref left_ViewerVM, value);
        }
        RootViewer_ViewModel Right_ViewerVM
        {
            get => right_ViewerVM;
            set => SetProperty(ref right_ViewerVM, value);
        }
        #endregion
        RecipeSideImageViewers_Panel Main;

        public RecipeSideImageViewers_ViewModel()
        {
            top_ViewerVM = new RootViewer_ViewModel();
            bottom_ViewerVM = new RootViewer_ViewModel();
            left_ViewerVM = new RootViewer_ViewModel();
            right_ViewerVM = new RootViewer_ViewModel();

            Main = new RecipeSideImageViewers_Panel();
        }
    }
}
