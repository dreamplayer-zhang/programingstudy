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
    public class ImageNROI_ViewModel:ObservableObject
    {
        public ImageNROI_Panel Main;
        public PodInfo_ViewModel podInfo;
        MaskTools_ViewModel maskTools;
        MaskRootViewer_ViewModel selectedViewer;

        public MaskRootViewer_ViewModel SelectedViewer
        {
            get => selectedViewer;
            set => SetProperty(ref selectedViewer, value);
        }
        public MaskTools_ViewModel MaskTools
        {
            get => maskTools;
            set => SetProperty(ref maskTools, value);
        }
        public ImageNROI_ViewModel(PodInfo_ViewModel podInfo)
        {
            this.podInfo = podInfo;
            Main = new ImageNROI_Panel();
            Main.DataContext = this;
            maskTools = new MaskTools_ViewModel();
            SelectedViewer = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front", maskTools);
            VegaPEventManager.ImageROIBtn += VegaPEventManager_ImageROIBtn;
        }

        private void VegaPEventManager_ImageROIBtn(object sender, ImageROIEventArgs e)
        {
            selectedViewer.SetImageData(GlobalObjects.Instance.GetNamed<ImageData>(e.memstr));
        }

        public ICommand btnSaveMasterImage
        {
            get => new RelayCommand(() => { });
        }
        public ICommand btnLoadMasterImage
        {
            get => new RelayCommand(() => { });
        }
        public ICommand btnDot
        {
            get => new RelayCommand(() => { });
        
        }
        public ICommand btnRect
        {
            get => new RelayCommand(() => { });
        }
        public ICommand btnSelect
        {
            get => new RelayCommand(() => { });
        }
    }
}
