using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class RecipeOrigin_ViewModel : ObservableObject
    {
        public RecipeOrigin_Panel Main;
        RecipeManager_ViewModel recipeManager;
        UserControl m_panel;
        ImageViewerBase_ViewModel mBase;
        OriginViewerTab_ViewModel originviewerTab;
        PositionViewerTab_ViewModel positionviewerTab;
        #region Property
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

        }
        public ICommand btnBack
        {
            get => new RelayCommand(() => { recipeManager.setup.SetRecipeWizard(); });
        }

    }
}
