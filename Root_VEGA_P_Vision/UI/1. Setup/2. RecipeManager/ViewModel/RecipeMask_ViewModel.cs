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
    public class RecipeMask_ViewModel:ObservableObject
    {
        Recipe6um_ViewModel recipeTDIVM;
        Recipe1um_ViewModel recipeStackingVM;
        RecipeSide_ViewModel recipeSideVM;
        RecipeStain_ViewModel recipeStainVM;

        public RecipeMask_Panel Main;
        private UserControl m_CurrentPanel;
        RecipeManager_ViewModel recipeManager;
        MaskTools_ViewModel maskTools;

        EUVPodSurfaceRecipe surfaceRecipe;
        EUVPodSurfaceParameter surfaceParameter;
        EUVPodSurfaceParameterBase curBaseParam;
        SurfaceParam_Tree_ViewModel surfaceParamTree;
        ImageViewerBase_ViewModel mBase;
        bool isSide, isHighRes;
        public bool IsHighRes
        {
            get => isHighRes;
            set => SetProperty(ref isHighRes, value);
        }
        public bool IsSide
        {
            get => isSide;
            set => SetProperty(ref isSide, value);
        }

        public RecipeManager_ViewModel RecipeManager
        {
            get => recipeManager;
            set => SetProperty(ref recipeManager, value);
        }
        public ImageViewerBase_ViewModel p_BaseViewer
        {
            get => mBase;
            set => SetProperty(ref mBase, value);
        }
        public SurfaceParam_Tree_ViewModel SurfaceParamTree
        {
            get => surfaceParamTree;
            set => SetProperty(ref surfaceParamTree, value);
        }

        
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
            surfaceRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVPodSurfaceRecipe>();
            surfaceParameter = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVPodSurfaceParameter>();
            maskTools = new MaskTools_ViewModel();

            recipeStainVM = new RecipeStain_ViewModel(this);
            recipeTDIVM = new Recipe6um_ViewModel(this);
            recipeSideVM = new RecipeSide_ViewModel(this);
            recipeStackingVM = new Recipe1um_ViewModel(this);
            surfaceParamTree = new SurfaceParam_Tree_ViewModel();
            mBase = new ImageViewerBase_ViewModel();

            CurBaseParam = SurfaceParameter.PodStain;
            IsSide = false;
            SetStain();
        }

        #region Property
        public EUVPodSurfaceParameterBase CurBaseParam
        {
            get => curBaseParam;
            set => SetProperty(ref curBaseParam, value);
        }
        public EUVPodSurfaceRecipe SurfaceRecipe
        {
            get => surfaceRecipe;
            set => SetProperty(ref surfaceRecipe, value);
        }
        public EUVPodSurfaceParameter SurfaceParameter
        {
            get => surfaceParameter;
            set => SetProperty(ref surfaceParameter, value);
        }
        public MaskTools_ViewModel MaskTools
        {
            get => maskTools;
            set => SetProperty(ref maskTools, value);
        }
        #endregion

        #region RelayCommand
        public void SetStain()
        {
            p_BaseViewer.p_SubViewer = recipeStainVM.Main;
            p_BaseViewer.p_SubViewer.DataContext = recipeStainVM;
            CurBaseParam = SurfaceParameter.PodStain;
            IsSide = false;
            IsHighRes = false;
        }
        public void SetTDI()
        {
            p_BaseViewer.p_SubViewer = recipeTDIVM.Main;
            p_BaseViewer.p_SubViewer.DataContext = recipeTDIVM;
            CurBaseParam = SurfaceParameter.PodTDI;
            IsSide = false;
            IsHighRes = false;

        }
        public void SetStacking()
        {
            p_BaseViewer.p_SubViewer = recipeStackingVM.Main;
            p_BaseViewer.p_SubViewer.DataContext = recipeStackingVM;
            CurBaseParam = SurfaceParameter.PodStacking;
            IsSide = false;
            IsHighRes = true;

        }
        public void SetSide()
        {
            p_BaseViewer.p_SubViewer = recipeSideVM.Main;
            p_BaseViewer.p_SubViewer.DataContext = recipeSideVM;
            CurBaseParam = surfaceParameter.PodSide;
            IsSide = true;
            IsHighRes = false;
        }
        public ICommand btnBack
        {
            get => new RelayCommand(()=>recipeManager.home.m_Setup.SetRecipeWizard());
        }
        public ICommand btnRect
        {
            get => new RelayCommand(() => { });
        }
        public ICommand btnDot
        {
            get => new RelayCommand(() => { });
        }
        public ICommand btnSelect
        {
            get => new RelayCommand(() => { });
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
