using RootTools;
using RootTools_Vision;

namespace Root_WIND2.UI_Temp
{
    public class FrontsideInspect_ViewModel
    {
        private FrontsideInspect_ImageViewer_ViewModel imageViewerVM;
        public FrontsideInspect_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }

        public FrontsideInspect_ViewModel()
        {
            this.imageViewerVM = new FrontsideInspect_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());
        }
    }
}
