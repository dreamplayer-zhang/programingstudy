using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    class RecipeStain_ViewModel: ObservableObject
    {
        public RecipeSetting_ViewModel recipeSetting;
        public RecipeStain_Panel Main;

        #region [ImageViewer ViewModel]
        private readonly EIPCoverTop_ImageViewer_ViewModel EIPcovertop_ImageViewerVM;
        private readonly EIPCoverBottom_ImageViewer_ViewModel EIPcoverbottom_ImageViewerVM;
        private readonly EIPBaseTop_ImageViewer_ViewModel EIPbasetop_ImageViewerVM;
        private readonly EIPBaseBottom_ImageViewer_ViewModel EIPbasebottom_ImageViewerVM;

        public EIPCoverTop_ImageViewer_ViewModel EIPCoverTop_ImageViewerVM
        {
            get => EIPcovertop_ImageViewerVM;
        }
        public EIPCoverBottom_ImageViewer_ViewModel EIPCoverBottom_ImaageViewerVM
        {
            get => EIPcoverbottom_ImageViewerVM;
        }
        public EIPBaseTop_ImageViewer_ViewModel EIPBaseTop_ImageViewerVM
        {
            get => EIPbasetop_ImageViewerVM;
        }
        public EIPBaseBottom_ImageViewer_ViewModel EIPBaseBottom_ImageViewerVM
        {
            get => EIPbasebottom_ImageViewerVM;
        }
        #endregion

        public RecipeStain_ViewModel(RecipeSetting_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
            Main = new RecipeStain_Panel();

            EIPcovertop_ImageViewerVM = new EIPCoverTop_ImageViewer_ViewModel();
            EIPcovertop_ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Front"), GlobalObjects.Instance.Get<DialogService>());

            EIPcoverbottom_ImageViewerVM = new EIPCoverBottom_ImageViewer_ViewModel();
            EIPcovertop_ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Back"), GlobalObjects.Instance.Get<DialogService>());

            EIPbasetop_ImageViewerVM = new EIPBaseTop_ImageViewer_ViewModel();
            EIPbasetop_ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EIP_Plate.Main.Front"), GlobalObjects.Instance.Get<DialogService>());

            EIPbasebottom_ImageViewerVM = new EIPBaseBottom_ImageViewer_ViewModel();
            EIPbasebottom_ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EIP_Plate.Main.Back"), GlobalObjects.Instance.Get<DialogService>());
        }

        public ICommand btnBack
        {
            get => new RelayCommand(()=> {
                //recipeSetting.SetRecipeWizard();
            });
        }

    }
}
