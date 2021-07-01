using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Module;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class Recipe1um_ViewModel : ObservableObject
    {
        public RecipeMask_ViewModel recipeMask;
        public Recipe1um_Panel Main;

        MaskRootViewer_ViewModel EIPcoverBottom_TDI, EIPbasePlateTop_TDI, EIPcoverBottom_Stacking, basePlateROI1, basePlateROI2, selectedViewer;
        int selectedTab;
        List<int> numList;
        public List<int> MemNumList
        {
            get => numList;
            set => SetProperty(ref numList, value);
        }

        #region Property
        bool selectedViewerVisibility;
        public bool SelectedViewerVisibility
        {
            get => selectedViewerVisibility;
            set => SetProperty(ref selectedViewerVisibility,value);
        }
        private ObservableCollection<UIElement> ROIlist = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> ROIList
        {
            get => ROIlist;
            set => SetProperty(ref ROIlist, value);
        }
        int ROIlistIdx = -1;
        public int ROIListIdx
        {
            get => ROIlistIdx;
            set => SetProperty(ref ROIlistIdx, value);
        }
        public int SelectedTab
        {
            get => selectedTab;
            set
            {
                SetProperty(ref selectedTab, value);
                if (value == 0)
                    selectedViewer = EIPCoverBottom_TDI;
                else
                    selectedViewer = EIPBasePlateTop_TDI;

                recipeMask.SurfaceParameterBase = selectedViewer.Recipe.GetItem<EUVPodSurfaceParameter>().PodStacking;

                selectedViewer.SetMask();
            }
        }
        public MaskRootViewer_ViewModel SelectedViewer
        {
            get => selectedViewer;
            set => SetProperty(ref selectedViewer, value);
        }
        public MaskRootViewer_ViewModel EIPCoverBottom_TDI
        {
            get => EIPcoverBottom_TDI;
            set => SetProperty(ref EIPcoverBottom_TDI, value);
        }
        public MaskRootViewer_ViewModel EIPBasePlateTop_TDI
        {
            get => EIPbasePlateTop_TDI;
            set => SetProperty(ref EIPbasePlateTop_TDI, value);
        }
        public MaskRootViewer_ViewModel EIPCoverBottom_Stacking
        {
            get => EIPcoverBottom_Stacking;
            set => SetProperty(ref EIPcoverBottom_Stacking, value);
        }
        public MaskRootViewer_ViewModel BasePlateROI1
        {
            get => basePlateROI1;
            set => SetProperty(ref basePlateROI1, value);
        }
        public MaskRootViewer_ViewModel BasePlateROI2
        {
            get => basePlateROI2;
            set => SetProperty(ref basePlateROI2, value);
        }
        #endregion
        public Recipe1um_ViewModel(RecipeMask_ViewModel recipeMask)
        {
            this.recipeMask = recipeMask;
            Main = new Recipe1um_Panel();
            Main.DataContext = this;
            SelectedViewerVisibility = false;
            RecipeCoverBack recipeCoverBack = GlobalObjects.Instance.Get<RecipeCoverBack>();
            EIPcoverBottom_TDI = new MaskRootViewer_ViewModel("EIP_Cover.Main.Back", recipeMask.MaskTools,
                recipeCoverBack, recipeCoverBack.GetItem<EUVOriginRecipe>().TDIOriginInfo,recipeCoverBack.GetItem<EUVPodSurfaceParameter>().PodTDI.MaskIndex);

            RecipePlateFront recipePlateFront = GlobalObjects.Instance.Get<RecipePlateFront>();
            EIPbasePlateTop_TDI = new MaskRootViewer_ViewModel("EIP_Plate.Main.Front", recipeMask.MaskTools,
                recipePlateFront, recipePlateFront.GetItem<EUVOriginRecipe>().TDIOriginInfo,recipePlateFront.GetItem<EUVPodSurfaceParameter>().PodTDI.MaskIndex);

            EIPcoverBottom_Stacking = new MaskRootViewer_ViewModel("EIP_Cover.Stack.Back", recipeMask.MaskTools,
                recipeCoverBack,recipeCoverBack.GetItem<EUVOriginRecipe>().TDIOriginInfo,recipeCoverBack.GetItem<EUVPodSurfaceParameter>().PodStacking.MaskIndex);

            basePlateROI1 = new MaskRootViewer_ViewModel("EIP_Cover.Stack.Back", recipeMask.MaskTools,
                recipeCoverBack, recipeCoverBack.GetItem<EUVOriginRecipe>().TDIOriginInfo, recipeCoverBack.GetItem<EUVPodSurfaceParameter>().PodStacking.MaskIndex);

            basePlateROI2 = new MaskRootViewer_ViewModel("EIP_Plate.Stack.Front", recipeMask.MaskTools,
                recipePlateFront, recipePlateFront.GetItem<EUVOriginRecipe>().TDIOriginInfo, recipePlateFront.GetItem<EUVPodSurfaceParameter>().PodTDI.MaskIndex);

            SelectedViewer = EIPCoverBottom_TDI;
            curROIListItem = new ROIListItem();

            numList = new List<int>();
            for (int i = 0; i < EIPcoverBottom_TDI.p_ImageData.p_nPlane; i++)
                MemNumList.Add(i + 1);

            selectedViewer.CapturedAreaDone += SelectedViewer_CapturedAreaDone;
        }
        public CRect memRect;
        ROIListItem curROIListItem;
        private void SelectedViewer_CapturedAreaDone(object e,string Parts)
        {
            memRect = ((TRect)e).MemoryRect;
            curROIListItem.SetData(Parts, memRect.Left.ToString(), memRect.Top.ToString());
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
        public ICommand btnDraw
        {
            get => new RelayCommand(() => 
            selectedViewer.m_eCurMode = ViewerMode.CaptureROI);
        }
        public ICommand btnAdd
        {
            get => new RelayCommand(() =>
              {
                  ROIListItem item = new ROIListItem(curROIListItem);
                  ROIList.Add(item);
                  recipeMask.Main.btnAdd.IsChecked = false;
              });
        }
        public ICommand btnDelete
        {
            get => new RelayCommand(() =>
            {
                ROIList.RemoveAt(ROIListIdx);
                if (ROIList.Count == 0)
                    ROIListIdx = -1;
                recipeMask.Main.btnDelete.IsChecked = false;

            });
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

            vision.StartRun((Run_ZStack)vision.CloneModuleRun(App.mZStack));
        }
    }
}
