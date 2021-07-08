using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools_Vision;
using RootTools_Vision.WorkManager3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_VEGA_P_Vision
{
    public class RecipeStain_ViewModel : ObservableObject
    {       
        public RecipeMask_ViewModel recipeMask;
        public RecipeStain_Panel Main;
        MaskRootViewer_ViewModel selectedViewer;

        List<int> numList;
        int selectedIdx;
        public int SelectedIdx
        {
            get => selectedIdx;
            set
            {
                SetProperty(ref selectedIdx, value);
                selectedViewer.SelectedIdx = value;
                selectedViewer.SetMask();
            }
        }
        public List<int> MemNumList
        {
            get => numList;
            set => SetProperty(ref numList, value);
        }

        #region [ImageViewer ViewModel]
        MaskRootViewer_ViewModel EIPcovertop_ImageViewerVM, EIPcoverbottom_ImageViewerVM;
        MaskRootViewer_ViewModel EIPbasetop_ImageViewerVM, EIPbasebottom_ImageViewerVM;

        public MaskRootViewer_ViewModel EIPCoverTop_ImageViewerVM
        {
            get => EIPcovertop_ImageViewerVM;
            set => SetProperty(ref EIPcovertop_ImageViewerVM, value);
        }
        public MaskRootViewer_ViewModel EIPCoverBottom_ImaageViewerVM
        {
            get => EIPcoverbottom_ImageViewerVM;
            set => SetProperty(ref EIPcoverbottom_ImageViewerVM, value);
        }
        public MaskRootViewer_ViewModel EIPBaseTop_ImageViewerVM
        {
            get => EIPbasetop_ImageViewerVM;
            set => SetProperty(ref EIPbasetop_ImageViewerVM, value);
        }
        public MaskRootViewer_ViewModel EIPBaseBottom_ImageViewerVM
        {
            get => EIPbasebottom_ImageViewerVM;
            set => SetProperty(ref EIPbasebottom_ImageViewerVM, value);
        }
        public MaskRootViewer_ViewModel SelectedViewer
        {
            get => selectedViewer;
            set => SetProperty(ref selectedViewer, value);
        }
        #endregion

        public RecipeStain_ViewModel(RecipeMask_ViewModel recipeMask)
        {
            this.recipeMask = recipeMask;
            Main = new RecipeStain_Panel();
            Main.DataContext = this;
            RecipeCoverFront recipeCoverFront = GlobalObjects.Instance.Get<RecipeCoverFront>();

            EIPcovertop_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Cover.Stain.Front", recipeMask.MaskTools,
                recipeCoverFront, recipeCoverFront.GetItem<EUVOriginRecipe>().StainOriginInfo, recipeCoverFront.GetItem<EUVPodSurfaceParameter>().PodStain.MaskIndex);

            RecipeCoverBack recipeCoverBack = GlobalObjects.Instance.Get<RecipeCoverBack>();

            EIPcoverbottom_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Cover.Stain.Back", recipeMask.MaskTools,
                recipeCoverBack, recipeCoverBack.GetItem<EUVOriginRecipe>().StainOriginInfo, recipeCoverBack.GetItem<EUVPodSurfaceParameter>().PodStain.MaskIndex);

            RecipePlateFront recipePlateFront = GlobalObjects.Instance.Get<RecipePlateFront>();
            EIPbasetop_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Plate.Stain.Front", recipeMask.MaskTools,
                recipePlateFront, recipePlateFront.GetItem<EUVOriginRecipe>().StainOriginInfo, recipePlateFront.GetItem<EUVPodSurfaceParameter>().PodStain.MaskIndex);

            RecipePlateBack recipePlateBack = GlobalObjects.Instance.Get<RecipePlateBack>();
            EIPbasebottom_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Plate.Stain.Back", recipeMask.MaskTools,
                recipePlateBack, recipePlateBack.GetItem<EUVOriginRecipe>().StainOriginInfo, recipePlateBack.GetItem<EUVPodSurfaceParameter>().PodStain.MaskIndex);

            selectedViewer = EIPCoverTop_ImageViewerVM;
            numList = new List<int>();
            for (int i = 0; i < EIPcovertop_ImageViewerVM.p_ImageData.p_nPlane; i++)
                MemNumList.Add(i+1);
        }

        #region RelayCommand

        public ICommand btnBack
        {
            get => new RelayCommand(() =>
            {
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
            get => new RelayCommand(() => selectedViewer.Inspection());
        }
        public ICommand TabChanged
        {
            get => new RelayCommand(() => {
                switch (Main.MaskViewerTab.SelectedIndex)
                {
                    case 0:
                        selectedViewer = EIPCoverTop_ImageViewerVM;
                        break;
                    case 1:
                        selectedViewer = EIPCoverBottom_ImaageViewerVM;
                        break;
                    case 2:
                        selectedViewer = EIPBaseTop_ImageViewerVM;
                        break;
                    case 3:
                        selectedViewer = EIPBaseBottom_ImageViewerVM;
                        break;
                }
                recipeMask.SurfaceParameterBase = selectedViewer.Recipe.GetItem<EUVPodSurfaceParameter>().PodStain;
                selectedViewer.SetMask();
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

            vision.StartRun((Run_StainGrab)vision.CloneModuleRun(App.mStainGrab));
        }
        #endregion


    }
}
