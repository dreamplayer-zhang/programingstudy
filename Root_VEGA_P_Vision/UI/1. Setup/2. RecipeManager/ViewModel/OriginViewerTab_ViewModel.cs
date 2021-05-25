using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_VEGA_P_Vision
{
    public class OriginViewerTab_ViewModel : ObservableObject
    {
        public OriginViewerTab_Panel Main;
        RootViewer_ViewModel m2DTDIViewer, mStainViewer, mSideTopBtmViewer,mSideLeftRightViewer;
        RootViewer_ViewModel selectedViewer;

        int selectedIdx, sideselectedIdx;

        #region Property
        public RootViewer_ViewModel p_2DOriginViewer
        {
            get => m2DTDIViewer;
            set => SetProperty(ref m2DTDIViewer, value);
        }
        public RootViewer_ViewModel p_StainOriginViewer
        {
            get => mStainViewer;
            set => SetProperty(ref mStainViewer, value);
        }
        public RootViewer_ViewModel p_SideOriginTopBtmViewer
        {
            get => mSideTopBtmViewer;
            set => SetProperty(ref mSideTopBtmViewer, value);
        }
        public RootViewer_ViewModel p_SideOriginLeftRightViewer
        {
            get => mSideLeftRightViewer;
            set => SetProperty(ref mSideLeftRightViewer, value);
        }
        #endregion
        public OriginViewerTab_ViewModel()
        {
            Main = new OriginViewerTab_Panel();
            Main.DataContext = this;
            m2DTDIViewer = new RootViewer_ViewModel();
            mStainViewer = new RootViewer_ViewModel();
            mSideTopBtmViewer = new RootViewer_ViewModel();
            mSideLeftRightViewer = new RootViewer_ViewModel();

            ImageData TDIimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Front");
            ImageData Stainimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Stain.Front");
            ImageData SideTBimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Top");
            ImageData SideLRimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Left");

            p_2DOriginViewer.init(TDIimage);
            p_StainOriginViewer.init(Stainimage);
            p_SideOriginTopBtmViewer.init(SideTBimage);
            p_SideOriginLeftRightViewer.init(SideLRimage);

            selectedIdx = 0;
            sideselectedIdx = 0;
            selectedViewer = p_2DOriginViewer;
        }

        #region RelayCommand
        public ICommand TabChanged
        {
            get => new RelayCommand(() => {
                selectedIdx = Main.MainTab.SelectedIndex;
                sideselectedIdx = Main.SideTab.SelectedIndex;
                switch (selectedIdx)
                {
                    case 0:
                        selectedViewer = p_2DOriginViewer;
                        break;
                    case 1:
                        selectedViewer = p_StainOriginViewer;
                        break;
                    case 2:
                        if (selectedIdx + sideselectedIdx == 2)
                            selectedViewer = p_SideOriginTopBtmViewer;
                        else if (selectedIdx + sideselectedIdx == 3)
                            selectedViewer = p_SideOriginLeftRightViewer;
                        break;
                }
            });
        }

        public ICommand ImageOpen
        {
            get => new RelayCommand(() => selectedViewer._openImage());
        }
        public ICommand ImageSave
        {
            get => new RelayCommand(() => selectedViewer._saveImage());
        }
        public ICommand ImageClear
        {
            get => new RelayCommand(() => selectedViewer._clearImage());
        }
        public ICommand btnSnap
        {
            get => new RelayCommand(() => Snap());
        }
        void Snap()
        {
            EQ.p_bStop = false;
            Vision vision = GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().m_handler.m_vision;
            if (vision.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
        }
        #endregion
    }
}
