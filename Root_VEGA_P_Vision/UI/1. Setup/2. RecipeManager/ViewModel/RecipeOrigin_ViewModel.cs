using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Module;
using RootTools_Vision;
namespace Root_VEGA_P_Vision
{
    public enum PositionFeature
    {
        COVERTOP=0,COVERBTM,BASETOP,BASEBTM
    }
    public class RecipeOrigin_ViewModel : ObservableObject
    {
        public RecipeOrigin_Panel Main;
        RecipeManager_ViewModel recipeManager;
        UserControl m_panel;
        ImageViewerBase_ViewModel mBase;
        OriginViewerTab_ViewModel originviewerTab;
        PositionViewerTab_ViewModel positionviewerTab;

        AlignFeatureInfo_ViewModel selectedFeatureInfo;
        OriginInfo_ViewModel tdiOrigin, stainOrigin,sideTBOrigin,sideLROrigin;
        public ImageData boxImage;
        public CRect memRect;
        bool originInfoVisible,positionInfoVisible;
        double dRotateAngle;
        #region Property
        public bool OriginInfoVisible
        {
            get => originInfoVisible;
            set => SetProperty(ref originInfoVisible, value);
        }
        public bool PositionInfoVisible
        {
            get => positionInfoVisible;
            set => SetProperty(ref positionInfoVisible, value);
        }
        public OriginInfo_ViewModel TDIOrigin
        {
            get => tdiOrigin;
            set => SetProperty(ref tdiOrigin, value);
        }
        public OriginInfo_ViewModel StainOrigin
        {
            get => stainOrigin;
            set => SetProperty(ref stainOrigin, value);
        }
        public OriginInfo_ViewModel SideTBOrigin
        {
            get => sideTBOrigin;
            set => SetProperty(ref sideTBOrigin, value);
        }
        public OriginInfo_ViewModel SideLROrigin
        {
            get => sideLROrigin;
            set => SetProperty(ref sideLROrigin, value);
        }
        public AlignFeatureInfo_ViewModel SelectedFeatureInfo
        {
            get => selectedFeatureInfo;
            set => SetProperty(ref selectedFeatureInfo, value);
        }
        public OriginViewerTab_ViewModel OriginViewerTab
        {
            get => originviewerTab;
            set => SetProperty(ref originviewerTab, value);
        }
        public PositionViewerTab_ViewModel PositionViewerTab
        {
            get => positionviewerTab;
            set => SetProperty(ref positionviewerTab, value);
        }
        public ImageViewerBase_ViewModel p_BaseViewer
        {
            get => mBase;
            set => SetProperty(ref mBase, value);
        }
        public UserControl p_OriginViewerPanel
        {
            get => m_panel;
            set => SetProperty(ref m_panel, value);
        }

        #endregion
        public RecipeOrigin_ViewModel(RecipeManager_ViewModel recipeManager)
        {
            this.recipeManager = recipeManager;
            Main = new RecipeOrigin_Panel();
            Main.DataContext = this;
            mBase = new ImageViewerBase_ViewModel();
            originviewerTab = new OriginViewerTab_ViewModel();
            positionviewerTab = new PositionViewerTab_ViewModel();

            SelectedFeatureInfo = new AlignFeatureInfo_ViewModel(this, positionviewerTab.selectedLists, positionviewerTab.selectedViewer.recipe);
            EUVOriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeCoverFront>().GetItem<EUVOriginRecipe>();
            tdiOrigin = new OriginInfo_ViewModel(originRecipe.TDIOriginInfo, "2D TDI Origin");
            stainOrigin = new OriginInfo_ViewModel(originRecipe.StainOriginInfo, "Stain Origin");
            sideTBOrigin = new OriginInfo_ViewModel(originRecipe.SideTBOriginInfo, "Side Top -Bottom Origin");
            sideLROrigin = new OriginInfo_ViewModel(originRecipe.SideLROriginInfo,"Side Left - Right Origin");;

            positionviewerTab.p_EIPBaseBtm.FeatureBoxDone += FeatureBoxDoneUpdate;
            positionviewerTab.p_EIPBaseTop.FeatureBoxDone += FeatureBoxDoneUpdate;
            positionviewerTab.p_EIPCoverBtm.FeatureBoxDone += FeatureBoxDoneUpdate;
            positionviewerTab.p_EIPCoverTop.FeatureBoxDone += FeatureBoxDoneUpdate;

            positionviewerTab.p_EIPBaseBtm.ManualAlignDone += ManualAlignDoneUpdate;
            positionviewerTab.p_EIPBaseTop.ManualAlignDone += ManualAlignDoneUpdate;
            positionviewerTab.p_EIPCoverBtm.ManualAlignDone += ManualAlignDoneUpdate;
            positionviewerTab.p_EIPCoverTop.ManualAlignDone += ManualAlignDoneUpdate;

            originInfoVisible = true;
            positionInfoVisible = false;
            VegaPEventManager.RecipeUpdated += VegaPEventManager_RecipeUpdated;
        }

        private void VegaPEventManager_RecipeUpdated(object sender, RecipeEventArgs e)
        {
            EUVOriginRecipe originRecipe = e.recipe.GetItem<EUVOriginRecipe>();
            tdiOrigin.OriginInfo = originRecipe.TDIOriginInfo;
            stainOrigin.OriginInfo = originRecipe.StainOriginInfo;
            sideTBOrigin.OriginInfo = originRecipe.SideTBOriginInfo;
            sideLROrigin.OriginInfo = originRecipe.SideLROriginInfo;
        }

        public void ManualAlignDoneUpdate(CPoint Top, CPoint Btm)
        {
            dRotateAngle = Calc.CalcAngle(Top, Btm);

            EQ.p_bStop = false;
            Vision vision = GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().m_handler.m_vision;
            if (vision.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }

            vision.m_stage.Rotate(dRotateAngle);
        }

        public void FeatureBoxDoneUpdate(object e)
        {
            memRect = ((TRect)e).MemoryRect;
            boxImage = new ImageData(memRect.Width, memRect.Height, 1);
            boxImage.SetData(positionviewerTab.selectedViewer.p_ImageData, new CRect(memRect.Left, memRect.Top, memRect.Right, memRect.Bottom), (int)positionviewerTab.selectedViewer.p_ImageData.p_Stride, 1);
        }

        public void SetOriginViewerTab()
        {
            p_BaseViewer.p_SubViewer = OriginViewerTab.Main;
            p_BaseViewer.InspBtnVisibility = Visibility.Collapsed;
        }
        public void SetPositionViewerTab()
        {
            p_BaseViewer.p_SubViewer = PositionViewerTab.Main;
            p_BaseViewer.InspBtnVisibility = Visibility.Visible;
            PositionViewerTab.UpdateOriginBox();
        }
        public ICommand btnBack
        {
            get => new RelayCommand(() => { recipeManager.home.m_Setup.SetRecipeWizard(); });
        }

    }
}
