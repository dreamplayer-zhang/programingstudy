using Root_VEGA_P_Vision.Engineer;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class ScrewUI_ViewModel:ObservableObject
    {
        private readonly RootViewer_ViewModel screwUI_ImageViewer_ViewModel;
        screwUI screwUI;
        int idx = 0;
        public List<ImageData> ImageDatas;
        public RootViewer_ViewModel ScrewUI_ImageViewerVM
        {
            get => screwUI_ImageViewer_ViewModel;
        }
        public ScrewUI_ViewModel(screwUI screwUI)
        {
            this.screwUI = screwUI;
            screwUI_ImageViewer_ViewModel = new RootViewer_ViewModel();
            screwUI_ImageViewer_ViewModel.p_VisibleMenu = Visibility.Collapsed;
            screwUI_ImageViewer_ViewModel.p_VisibleSlider = Visibility.Collapsed;
            screwUI_ImageViewer_ViewModel.init(new ImageData(GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().ClassMemoryTool().GetMemory("Vision.Memory", "Vision", "EIP_Plate.Main.Front")), GlobalObjects.Instance.Get<DialogService>());

            ImageDatas = new List<ImageData>();
        }

        public ICommand btnPrev
        {
            get => new RelayCommand(() => btnPrevCommand());
        }
        public ICommand btnNext
        {
            get => new RelayCommand(() => btnNextCommand());
        }

        void btnNextCommand()
        {
            if (idx >= ImageDatas.Count) return;
            
            idx++;
            SetCurrentImage(idx);
        }
        void btnPrevCommand()
        {
            if (idx <= 0) return;
            idx--;
            SetCurrentImage(idx);
        }
        public void SetCurrentImage(int idx)
        {
            screwUI_ImageViewer_ViewModel.SetImageData(ImageDatas[idx]);
        }
    }
}
