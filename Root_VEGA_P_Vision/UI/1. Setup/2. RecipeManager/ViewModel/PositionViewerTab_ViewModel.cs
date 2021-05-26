using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Module;
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
    public class PositionViewerTab_ViewModel:ObservableObject
    {
        public PositionViewerTab_Panel Main;
        public PositionFeature curTab;
        PositionImageViewer_ViewModel mEIPCoverTop, mEIPCoverBtm, mEIPBaseTop, mEIPBaseBtm;
        #region Property
        public PositionImageViewer_ViewModel p_EIPCoverTop
        {
            get => mEIPCoverTop;
            set => SetProperty(ref mEIPCoverTop, value);
        }
        public PositionImageViewer_ViewModel p_EIPCoverBtm
        {
            get => mEIPCoverBtm;
            set => SetProperty(ref mEIPCoverBtm, value);
        }
        public PositionImageViewer_ViewModel p_EIPBaseTop
        {
            get => mEIPBaseTop;
            set => SetProperty(ref mEIPBaseTop, value);
        }
        public PositionImageViewer_ViewModel p_EIPBaseBtm
        {
            get => mEIPBaseBtm;
            set => SetProperty(ref mEIPBaseBtm, value);
        }
        #endregion

        public PositionViewerTab_ViewModel()
        { 
            Main = new PositionViewerTab_Panel();
            Main.DataContext = this;
            mEIPCoverTop = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");
            mEIPCoverBtm = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");
            mEIPBaseTop = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");
            mEIPBaseBtm = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");

            //ImageData CoverTopimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Front");
            //ImageData CoverBtmimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Back");
            //ImageData BaseTopimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Plate.Main.Front");
            //ImageData BaseBtmimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Plate.Main.Back");

            //p_EIPCoverTop.init(CoverTopimage,GlobalObjects.Instance.Get<DialogService>());
            //p_EIPCoverBtm.init(CoverBtmimage, GlobalObjects.Instance.Get<DialogService>());
            //p_EIPBaseTop.init(BaseTopimage, GlobalObjects.Instance.Get<DialogService>());
            //p_EIPBaseBtm.init(BaseBtmimage, GlobalObjects.Instance.Get<DialogService>());

            selectedViewer = mEIPCoverTop;
            curTab = PositionFeature.COVERTOP;
        }

        private void MEIPBaseBtm_FeatureBoxDone(object e)
        {
         
            MessageBox.Show(e.ToString());
        }

        public void UpdateOriginBox()
        {
            p_EIPCoverTop.RedrawOriginBox();
            p_EIPCoverBtm.RedrawOriginBox();
            p_EIPBaseTop.RedrawOriginBox();
            p_EIPBaseBtm.RedrawOriginBox();
        }

        #region ICommand
        public PositionImageViewer_ViewModel selectedViewer { get; set; } = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front");
        int selectedIdx;
        public ICommand TabChanged
        {
            get => new RelayCommand(() => {
                curTab = (PositionFeature)Main.ViewerTab.SelectedIndex;
                switch (curTab)
                {
                    case PositionFeature.COVERTOP:
                        selectedViewer = p_EIPCoverTop;
                        break;
                    case PositionFeature.COVERBTM:
                        selectedViewer = p_EIPCoverBtm;
                        break;
                    case PositionFeature.BASETOP:
                        selectedViewer = p_EIPBaseTop;
                        break;
                    case PositionFeature.BASEBTM:
                        selectedViewer = p_EIPBaseBtm;
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
