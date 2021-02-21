using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    public class CameraAlign_ViewModel : ObservableObject
    {
        private CameraAlign_ImageViewer_ViewModel imageViewerVM;
        public CameraAlign_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
            set
            {
                SetProperty<CameraAlign_ImageViewer_ViewModel>(ref this.imageViewerVM, value);
            }
        }

        public CameraAlign_ViewModel()
        {
            this.imageViewerVM = new CameraAlign_ImageViewer_ViewModel();

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
