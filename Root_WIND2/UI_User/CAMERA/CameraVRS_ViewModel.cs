
namespace Root_WIND2.UI_User
{
    class CameraVRS_ViewModel : ObservableObject
    {
        private readonly CameraVRS_ImageViewer_ViewModel imageViewerVM;
        public CameraVRS_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }
        

        public CameraVRS_ViewModel()
        {
            imageViewerVM = new CameraVRS_ImageViewer_ViewModel();
            imageViewerVM.init();
        }

        #region [Command]
        public RelayCommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {


            });
        }
        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {

            });
        }
        #endregion
    }
}
