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
    public class RecipeSide_ViewModel:ObservableObject
    {
        public RecipeSide_Panel Main;
        public RecipeMask_ViewModel recipeMask;
        RecipeSideImageViewers_ViewModel EIPcoverViewers, EIPbaseViewers;
        RecipeSideImageViewers_ViewModel selectedSideTab;
        int selectedIdx;
        public int SelectedIdx
        {
            get => selectedIdx;
            set
            {
                SelectedSideTab.SelectedViewer.SelectedIdx = value;
                SetProperty(ref selectedIdx, value);
            }
        }
        int selectedTab;
        public int SelectedTab
        {
            get => selectedTab;
            set
            {
                switch(value)
                {
                    case 0:
                        SelectedSideTab = EIPCoverViewers;
                        break;
                    case 1:
                        selectedSideTab = EIPBaseViewers;
                        break;
                }
                SetProperty(ref selectedTab, value);
            } 
        }
        List<int> numList;
        public List<int> MemNumList
        {
            get => numList;
            set => SetProperty(ref numList, value);
        }
        public RecipeSideImageViewers_ViewModel SelectedSideTab
        {
            get => selectedSideTab;
            set => SetProperty(ref selectedSideTab, value);
        }
        #region Property
        public RecipeSideImageViewers_ViewModel EIPCoverViewers
        {
            get => EIPcoverViewers;
            set => SetProperty(ref EIPcoverViewers, value);
        }
        public RecipeSideImageViewers_ViewModel EIPBaseViewers
        {
            get => EIPbaseViewers;
            set => SetProperty(ref EIPbaseViewers, value);
        }
        #endregion
        public RecipeSide_ViewModel(RecipeMask_ViewModel recipeMask)
        {
            this.recipeMask = recipeMask;
            Main = new RecipeSide_Panel();
            Main.DataContext = this;

            EIPcoverViewers = new RecipeSideImageViewers_ViewModel("EIP_Cover",this);
            EIPbaseViewers = new RecipeSideImageViewers_ViewModel("EIP_Plate",this);
            SelectedSideTab = EIPcoverViewers;

            numList = new List<int>();
            for (int i = 0; i < EIPcoverViewers.Bottom_ViewerVM.p_ImageData.p_nPlane; i++)
                MemNumList.Add(i + 1);
        }

        public ICommand btnSnap
        {
            get => new RelayCommand(() => Snap());
        }
        public ICommand btnInsp
        {
            get => new RelayCommand(() => { });
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

            vision.StartRun((Run_SideGrab)vision.CloneModuleRun(App.mSideGrab));
        }
    }
}
