using System;
using System.Collections.Generic;
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
    public class Recipe6um_ViewModel:ObservableObject
    {
        RecipeMask_ViewModel recipeSetting;
        MaskRootViewer_ViewModel coverTop_ImageViewer, coverBottom_ImageViewer, baseTop_ImageViewer, baseBottom_ImageViewer;
        RootViewer_ViewModel selectedViewer;

        List<int> numList;
        public List<int> MemNumList
        {
            get => numList;
            set => SetProperty(ref numList, value);
        }
        #region Property
        public MaskRootViewer_ViewModel CoverTop_ImageViewer
        {
            get => coverTop_ImageViewer;
            set => SetProperty(ref coverTop_ImageViewer, value);
        }
        public MaskRootViewer_ViewModel CoverBottom_ImageViewer
        {
            get => coverBottom_ImageViewer;
            set => SetProperty(ref coverBottom_ImageViewer, value);
        }
        public MaskRootViewer_ViewModel BaseTop_ImageViewer
        {
            get => baseTop_ImageViewer;
            set => SetProperty(ref baseTop_ImageViewer, value);
        }
        public MaskRootViewer_ViewModel BaseBottom_ImageViewer
        {
            get => baseBottom_ImageViewer;
            set => SetProperty(ref baseBottom_ImageViewer, value);
        }
        #endregion

        public Recipe6um_Panel Main;

        
        public Recipe6um_ViewModel(RecipeMask_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
            Main = new Recipe6um_Panel();
            Main.DataContext = this;
            RecipeCoverFront recipeCoverFront = GlobalObjects.Instance.Get<RecipeCoverFront>();
            coverTop_ImageViewer = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front",recipeSetting.MaskTools,
                recipeCoverFront,recipeCoverFront.GetItem<EUVOriginRecipe>().TDIOriginInfo,recipeCoverFront.GetItem<EUVPodSurfaceParameter>().PodTDI.MaskIndex);
            
            RecipeCoverBack recipeCoverBack = GlobalObjects.Instance.Get<RecipeCoverBack>();
            coverBottom_ImageViewer = new MaskRootViewer_ViewModel("EIP_Cover.Main.Back", recipeSetting.MaskTools,
                recipeCoverBack,recipeCoverBack.GetItem<EUVOriginRecipe>().TDIOriginInfo,recipeCoverBack.GetItem<EUVPodSurfaceParameter>().PodTDI.MaskIndex);

            RecipePlateFront recipePlateFront = GlobalObjects.Instance.Get<RecipePlateFront>();
            baseTop_ImageViewer = new MaskRootViewer_ViewModel("EIP_Plate.Main.Front", recipeSetting.MaskTools,
                recipePlateFront,recipePlateFront.GetItem<EUVOriginRecipe>().TDIOriginInfo,recipePlateFront.GetItem<EUVPodSurfaceParameter>().PodTDI.MaskIndex);

            RecipePlateBack recipePlateBack = GlobalObjects.Instance.Get<RecipePlateBack>();
            baseBottom_ImageViewer = new MaskRootViewer_ViewModel("EIP_Plate.Main.Back", recipeSetting.MaskTools,
                recipePlateBack, recipePlateBack.GetItem<EUVOriginRecipe>().TDIOriginInfo, recipePlateBack.GetItem<EUVPodSurfaceParameter>().PodTDI.MaskIndex);

            numList = new List<int>();
            for(int i=0;i<coverTop_ImageViewer.p_ImageData.p_nPlane;i++)
                MemNumList.Add(i+1);
        }
        public ICommand btnSnap
        {
            get => new RelayCommand(() => Snap());
        }
        public ICommand btnInsp
        {
            get => new RelayCommand(() => { });
        }
        public ICommand TabChanged
        {
            get => new RelayCommand(() => {
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

            vision.StartRun((Run_MainGrab)vision.CloneModuleRun(App.mMainGrab));
        }
    }
}
