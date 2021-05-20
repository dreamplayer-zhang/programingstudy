using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RootTools;
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
        EUVOriginRecipe originRecipe;
        EUVPositionRecipe positionRecipe;

        AlignFeatureInfo_ViewModel EIPCoverTopfeatureInfo, EIPCoverBtmfeatureInfo, EIPBaseTopfeatureInfo, EIPBaseBtmfeatureInfo;
        public ImageData boxImage;
        public CRect memRect;
        #region Property
        public AlignFeatureInfo_ViewModel EIPCoverTopFeatureInfo
        {
            get => EIPCoverTopfeatureInfo;
            set => SetProperty(ref EIPCoverTopfeatureInfo, value);
        }
        public AlignFeatureInfo_ViewModel EIPCoverBtmFeatureInfo
        {
            get => EIPCoverBtmfeatureInfo;
            set => SetProperty(ref EIPCoverBtmfeatureInfo, value);
        }
        public AlignFeatureInfo_ViewModel EIPBaseTopFeatureInfo
        {
            get => EIPBaseTopfeatureInfo;
            set => SetProperty(ref EIPBaseTopfeatureInfo, value);
        }
        public AlignFeatureInfo_ViewModel EIPBaseBtmFeatureInfo
        {
            get => EIPBaseBtmfeatureInfo;
            set => SetProperty(ref EIPBaseBtmfeatureInfo, value);
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

        public EUVOriginRecipe p_OriginRecipe
        {
            get => originRecipe;
            set => SetProperty(ref originRecipe, value);
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
            originRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVOriginRecipe>();
            positionRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVPositionRecipe>();

            EIPCoverTopfeatureInfo = new AlignFeatureInfo_ViewModel(this,positionRecipe.EIPCoverTopFeature);
            EIPCoverBtmfeatureInfo = new AlignFeatureInfo_ViewModel(this,positionRecipe.EIPCoverBtmFeature);
            EIPBaseTopfeatureInfo = new AlignFeatureInfo_ViewModel(this,positionRecipe.EIPBaseTopFeature);
            EIPBaseBtmfeatureInfo = new AlignFeatureInfo_ViewModel(this,positionRecipe.EIPBaseBtmFeature);

            //var property = typeof(PositionViewerTab_ViewModel).GetProperties();
            //int i = 0;
            //foreach(var pro in property)
            //{
            //    if(pro.PropertyType == typeof(PositionImageViewer_ViewModel))
            //    {
            //        ((PositionImageViewer_ViewModel)pro).FeatureBoxDone += FeatureBoxDoneUpdate;
            //    }
            //}

            ////positionviewerTab.selectedViewer.FeatureBoxDone += FeatureBoxDoneUpdate;
            ///
            positionviewerTab.p_EIPBaseBtm.FeatureBoxDone += FeatureBoxDoneUpdate;
            positionviewerTab.p_EIPBaseTop.FeatureBoxDone += FeatureBoxDoneUpdate;
            positionviewerTab.p_EIPCoverBtm.FeatureBoxDone += FeatureBoxDoneUpdate;
            positionviewerTab.p_EIPCoverTop.FeatureBoxDone += FeatureBoxDoneUpdate;

        }

        
        public void FeatureBoxDoneUpdate(object e)
        {
            memRect = ((TRect)e).MemoryRect;
            boxImage = new ImageData(memRect.Width,memRect.Height,1);
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
            get => new RelayCommand(() => { recipeManager.setup.SetRecipeWizard(); });
        }

    }
}
