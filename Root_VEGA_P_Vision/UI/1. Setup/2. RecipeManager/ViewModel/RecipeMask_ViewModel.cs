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
    public class RecipeMask_ViewModel:ObservableObject
    {
        Recipe6um_ViewModel recipeTDIVM;
        Recipe1um_ViewModel recipeStackingVM;
        RecipeSide_ViewModel recipeSideVM;
        RecipeStain_ViewModel recipeStainVM;

        public RecipeMask_Panel Main;
        private UserControl m_CurrentPanel;
        RecipeManager_ViewModel recipeManager;
        public UserControl p_MaskPanel
        {
            get => m_CurrentPanel;
            set => SetProperty(ref m_CurrentPanel, value);
        }

        public RecipeMask_ViewModel(RecipeManager_ViewModel recipeManager)
        {
            this.recipeManager = recipeManager;
            Main = new RecipeMask_Panel();
            Main.DataContext = this;
            recipeStainVM = new RecipeStain_ViewModel(this);
            recipeTDIVM = new Recipe6um_ViewModel(this);
            recipeSideVM = new RecipeSide_ViewModel(this);
            recipeStackingVM = new Recipe1um_ViewModel(this);
        }

        #region RelayCommand
        public void SetStain()
        {
            p_MaskPanel = recipeStainVM.Main;
            p_MaskPanel.DataContext = recipeStainVM;
        }
        public void SetTDI()
        {
            p_MaskPanel = recipeTDIVM.Main;
            p_MaskPanel.DataContext = recipeTDIVM;
        }
        public void SetStacking()
        {
            p_MaskPanel = recipeStackingVM.Main;
            p_MaskPanel.DataContext = recipeStackingVM;
        }
        public void SetSide()
        {
            p_MaskPanel = recipeSideVM.Main;
            p_MaskPanel.DataContext = recipeSideVM;              
        }
        public ICommand btnStain
        {
            get => new RelayCommand(() => SetStain());
        }
        public ICommand btn6um
        {
            get => new RelayCommand(() => SetTDI());
        }
        public ICommand btn1um
        {
            get => new RelayCommand(() => SetStacking());
        }
        public ICommand btnSide
        {
            get => new RelayCommand(() => SetSide());
        }
        public ICommand btnBack
        {
            get => new RelayCommand(()=>recipeManager.setup.SetRecipeWizard());
        }
        #endregion

        #region Mask
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

        #endregion
    }
}
