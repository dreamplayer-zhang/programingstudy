using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Module;
using RootTools_CLR;
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
        int selectedIdx;
        List<int> numList;

        #region Property
        public int SelectedIdx
        {
            get => selectedIdx;
            set
            {
                SetProperty(ref selectedIdx, value);
                selectedViewer.SelectedIdx = value;
            }
        }
        public List<int> MemNumList
        {
            get => numList;
            set => SetProperty(ref numList, value);
        }

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
            mEIPCoverTop = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front",
                GlobalObjects.Instance.Get<RecipeCoverFront>());
            mEIPCoverBtm = new PositionImageViewer_ViewModel("EIP_Cover.Main.Back",
                GlobalObjects.Instance.Get<RecipeCoverBack>());
            mEIPBaseTop = new PositionImageViewer_ViewModel("EIP_Plate.Main.Front",
                GlobalObjects.Instance.Get<RecipePlateFront>());
            mEIPBaseBtm = new PositionImageViewer_ViewModel("EIP_Plate.Main.Back",
                GlobalObjects.Instance.Get<RecipeCoverBack>());

            numList = new List<int>();
            for (int i = 0; i < mEIPCoverTop.p_ImageData.p_nPlane; i++)
                MemNumList.Add(i + 1);

            selectedViewer = mEIPCoverTop;
            curTab = PositionFeature.COVERTOP;
        }

        public void UpdateOriginBox()
        {
            p_EIPCoverTop.RedrawOriginBox();
            p_EIPCoverBtm.RedrawOriginBox();
            p_EIPBaseTop.RedrawOriginBox();
            p_EIPBaseBtm.RedrawOriginBox();
        }

        #region ICommand
        public PositionImageViewer_ViewModel selectedViewer { get; set; } = new PositionImageViewer_ViewModel("EIP_Cover.Main.Front",
            GlobalObjects.Instance.Get<RecipeCoverFront>());
        public FeatureLists selectedLists { get; set; } = new FeatureLists();
        public ICommand TabChanged
        {
            get => new RelayCommand(() => {
                curTab = (PositionFeature)Main.ViewerTab.SelectedIndex;
                switch (curTab)
                {
                    case PositionFeature.COVERTOP:
                        selectedViewer = p_EIPCoverTop;
                        selectedLists = GlobalObjects.Instance.Get<RecipeCoverFront>().GetItem<EUVPositionRecipe>().PartsFeatureList;
                        break;
                    case PositionFeature.COVERBTM:
                        selectedViewer = p_EIPCoverBtm;
                        selectedLists = GlobalObjects.Instance.Get<RecipeCoverBack>().GetItem<EUVPositionRecipe>().PartsFeatureList;
                        break;
                    case PositionFeature.BASETOP:
                        selectedViewer = p_EIPBaseTop;
                        selectedLists = GlobalObjects.Instance.Get<RecipePlateFront>().GetItem<EUVPositionRecipe>().PartsFeatureList;
                        break;
                    case PositionFeature.BASEBTM:
                        selectedViewer = p_EIPBaseBtm;
                        selectedLists = GlobalObjects.Instance.Get<RecipePlateBack>().GetItem<EUVPositionRecipe>().PartsFeatureList;
                        break;
                }
            });
        }
        public ICommand ImageOpen
        {
            get => new RelayCommand(() => selectedViewer._openImage(SelectedIdx));
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
        public ICommand btnInsp
        {
            get => new RelayCommand(() => GetAlignAngle());
        }

        public class Result
        {
            public CPoint Pos;
            public double Score;

            public Result(CPoint Pos, double Score)
            {
                this.Pos = Pos;
                this.Score = Score;
            }
        }

        void GetAlignAngle()
        {
            //Calc.GetAlignAngle(selectedViewer.p_ImageData, 50);
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

            vision.StartRun((Run_MainGrab)vision.CloneModuleRun(App.mMainGrab));
        }
        #endregion
    }
}
