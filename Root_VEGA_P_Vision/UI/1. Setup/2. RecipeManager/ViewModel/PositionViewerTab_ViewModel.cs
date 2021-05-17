using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class PositionViewerTab_ViewModel:ObservableObject
    {
        public PositionViewerTab_Panel Main;
        RootViewer_ViewModel mEIPCoverTop, mEIPCoverBtm, mEIPBaseTop, mEIPBaseBtm;
        #region Property
        public RootViewer_ViewModel p_EIPCoverTop
        {
            get => mEIPCoverTop;
            set => SetProperty(ref mEIPCoverTop, value);
        }
        public RootViewer_ViewModel p_EIPCoverBtm
        {
            get => mEIPCoverTop;
            set => SetProperty(ref mEIPCoverBtm, value);
        }
        public RootViewer_ViewModel p_EIPBaseTop
        {
            get => mEIPCoverTop;
            set => SetProperty(ref mEIPBaseTop, value);
        }
        public RootViewer_ViewModel p_EIPBaseBtm
        {
            get => mEIPCoverTop;
            set => SetProperty(ref mEIPBaseBtm, value);
        }
        #endregion

        public PositionViewerTab_ViewModel()
        {
            Main = new PositionViewerTab_Panel();
            Main.DataContext = this;
            mEIPCoverTop = new RootViewer_ViewModel();
            mEIPCoverBtm = new RootViewer_ViewModel();
            mEIPBaseTop = new RootViewer_ViewModel();
            mEIPBaseBtm = new RootViewer_ViewModel();

            ImageData CoverTopimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Front");
            ImageData CoverBtmimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Cover.Main.Back");
            ImageData BaseTopimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Plate.Main.Front");
            ImageData BaseBtmimage = GlobalObjects.Instance.GetNamed<ImageData>("EIP_Plate.Main.Back");

            p_EIPCoverTop.init(CoverTopimage);
            p_EIPCoverBtm.init(CoverBtmimage);
            p_EIPBaseTop.init(BaseTopimage);
            p_EIPBaseBtm.init(BaseBtmimage);

        }
    }
}
