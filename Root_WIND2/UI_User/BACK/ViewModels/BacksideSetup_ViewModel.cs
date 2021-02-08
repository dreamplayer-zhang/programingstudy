using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    public class BacksideSetup_ViewModel : ObservableObject, IPage
    {

        private BacksideSetup_ImageViewer_ViewModel imageViewerVM;
        public BacksideSetup_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
            set
            {
                SetProperty<BacksideSetup_ImageViewer_ViewModel>(ref this.imageViewerVM, value);
            }
        }

        public BacksideSetup_ViewModel()
        {
            this.imageViewerVM = new BacksideSetup_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("BackImage"), GlobalObjects.Instance.Get<DialogService>());
        }

        public void LoadRecipe()
        {

        }

        #region [Command]
        public ICommand LoadedCommand
        {
            get => new RelayCommand(() =>
             {

             });
        }

        #endregion
    }
}
