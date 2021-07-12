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
        RootViewer_ViewModel screwUI_ImageViewer_ViewModel;
        screwUI screwUI;
        int idx = 0;
        public List<ImageData> ImageDatas;
        public RootViewer_ViewModel ScrewUI_ImageViewerVM
        {
            get => screwUI_ImageViewer_ViewModel;
            set => SetProperty(ref screwUI_ImageViewer_ViewModel, value);
        }
        public ScrewUI_ViewModel(string memstr)
        {
            screwUI = new screwUI();
            screwUI.DataContext = this;
            ScrewUI_ImageViewerVM = new RootViewer_ViewModel();
            ScrewUI_ImageViewerVM.p_VisibleMenu = Visibility.Collapsed;
            ScrewUI_ImageViewerVM.p_VisibleSlider = Visibility.Collapsed;
            ImageDatas = new List<ImageData>();
            ScrewUI_ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>(memstr));
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
