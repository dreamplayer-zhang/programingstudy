using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class RecipeSetting_ViewModel : ObservableObject
    {
        public RecipeManager_ViewModel RecipeManager;
        public RecipeSetting_Panel Main;
        private UserControl m_CurrentPanel;
        public UserControl p_SubPanel
        {
            get => m_CurrentPanel;
            set => SetProperty(ref m_CurrentPanel, value);
        }
        public RecipeSetting_ViewModel(RecipeManager_ViewModel RecipeManager)
        {
            this.RecipeManager = RecipeManager;
            Main = new RecipeSetting_Panel();
            recipeStainVM = new RecipeStain_ViewModel(this);
            recipe6umVM = new Recipe6um_ViewModel(this);
            recipe1umVM = new Recipe1um_ViewModel(this);
            recipeSideVM = new RecipeSide_ViewModel(this);
            recipeSummaryVM = new RecipeSummary_ViewModel(this);
        }


        #region Recipe Wizard
        private RecipeStain_ViewModel recipeStainVM;
        private Recipe6um_ViewModel recipe6umVM;
        private Recipe1um_ViewModel recipe1umVM;
        private RecipeSide_ViewModel recipeSideVM;
        private RecipeSummary_ViewModel recipeSummaryVM;
        public void SetStain()
        {
            p_SubPanel = recipeStainVM.Main;
            p_SubPanel.DataContext = recipeStainVM;
            pGBsimilar = Visibility.Collapsed;
        }
        public void Set6um()
        {
            p_SubPanel = recipe6umVM.Main;
            p_SubPanel.DataContext = recipe6umVM;
            pGBsimilar = Visibility.Collapsed;
        }

        public void Set1um()
        {
            p_SubPanel = recipe1umVM.Main;
            p_SubPanel.DataContext = recipe1umVM;
            pGBsimilar = Visibility.Collapsed;
        }
        public void SetSide()
        {
            p_SubPanel = recipeSideVM.Main;
            p_SubPanel.DataContext = recipeSideVM;
            pGBsimilar = Visibility.Visible;
        }

        public void SetSummary()
        {
            p_SubPanel = recipeSummaryVM.Main;
            p_SubPanel.DataContext = recipeSummaryVM;
        }
        #endregion

        #region RelayCommand
        public ICommand btnBack
        {
            get => new RelayCommand(() => { RecipeManager.m_Setup.SetRecipeWizard(); });
        }
        #endregion

        #region Property
        public ToolType m_eToolType;
        ThresholdMode m_eThresholdMode;
        public ObservableCollection<UIElement> p_UIElements
        {
            get
            {
                return m_UIElements;
            }
            set
            {
                m_UIElements = value;
            }
        }
        private ObservableCollection<UIElement> m_UIElements;

        public int p_nThickness
        {
            get
            {
                return _nThickness;
            }
            set
            {
                SetProperty(ref _nThickness, value);
            }
        }
        private int _nThickness = 5;

        public int p_nThreshold
        {
            get
            {
                return _nThreshold;
            }
            set
            {
                SetProperty(ref _nThreshold, value);
            }
        }
        private int _nThreshold = 50;

        public int p_nThresholdMode
        {
            get
            {
                return _nThresholdMode;
            }
            set
            {
                m_eThresholdMode = (ThresholdMode)value;
                SetProperty(ref _nThresholdMode, value);
            }
        }
        private int _nThresholdMode = 0;

        public bool bPenCheck
        {
            get
            {
                return _bPenCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Pen;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bPenCheck, value);
            }
        }
        private bool _bPenCheck;

        public bool bEraserCheck
        {
            get
            {
                return _bEraserCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Eraser;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bEraserCheck, value);
            }
        }
        private bool _bEraserCheck;

        public bool bRectCheck
        {
            get
            {
                return _bRectCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Rect;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bRectCheck, value);
            }
        }
        private bool _bRectCheck;

        public bool bCircleCheck
        {
            get
            {
                return _bCircleCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Circle;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bCircleCheck, value);
            }
        }
        private bool _bCircleCheck;

        public bool bCropCheck
        {
            get
            {
                return _bCropCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Crop;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bCropCheck, value);
            }
        }
        private bool _bCropCheck;

        public bool bThresholdCheck
        {
            get
            {
                return _bThresholdCheck;
            }
            set
            {
                if (value)
                {
                    m_eToolType = ToolType.Threshold;
                    UncheckTool(m_eToolType);
                }
                SetProperty(ref _bThresholdCheck, value);
            }
        }
        private bool _bThresholdCheck;

        public bool bPenVisibility;

        private Visibility _bGBsimilar;
        public Visibility pGBsimilar
        {
            get => _bGBsimilar;
            set => SetProperty(ref _bGBsimilar, value);
        }
        #endregion

        void UncheckTool(ToolType type)
        {
            switch (type)
            {
                case ToolType.Pen:
                    bEraserCheck = false;
                    bRectCheck = false;
                    bCircleCheck = false;
                    bCropCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Eraser:
                    bPenCheck = false;
                    bRectCheck = false;
                    bCircleCheck = false;
                    bCropCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Rect:
                    bPenCheck = false;
                    bEraserCheck = false;
                    bCircleCheck = false;
                    bCropCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Circle:
                    bPenCheck = false;
                    bEraserCheck = false;
                    bRectCheck = false;
                    bCropCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Crop:
                    bPenCheck = false;
                    bEraserCheck = false;
                    bRectCheck = false;
                    bCircleCheck = false;
                    bThresholdCheck = false;
                    break;
                case ToolType.Threshold:
                    bPenCheck = false;
                    bEraserCheck = false;
                    bRectCheck = false;
                    bCircleCheck = false;
                    bCropCheck = false;
                    break;
            }
        }
    }
}
