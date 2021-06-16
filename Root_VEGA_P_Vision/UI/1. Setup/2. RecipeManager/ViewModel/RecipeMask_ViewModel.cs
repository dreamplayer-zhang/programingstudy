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
            surfaceParameter = GlobalObjects.Instance.Get<RecipeCoverFront>().GetItem<EUVPodSurfaceParameter>();
            maskTools = new MaskTools_ViewModel();

            recipeStainVM = new RecipeStain_ViewModel(this);
            recipeTDIVM = new Recipe6um_ViewModel(this);
            recipeSideVM = new RecipeSide_ViewModel(this);
            recipeStackingVM = new Recipe1um_ViewModel(this);
            surfaceParamTree = new SurfaceParam_Tree_ViewModel(GlobalObjects.Instance.Get<RecipeCoverFront>().GetItem<EUVPodSurfaceParameter>().PodStain);
            mBase = new ImageViewerBase_ViewModel();

            //CurBaseParam = SurfaceParameter.PodStain;
            IsSide = false;
            SetStain();
        }

        #region Property
        public EUVPodSurfaceParameterBase CurBaseParam
        {
            get => curBaseParam;
            set => SetProperty(ref curBaseParam, value);
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
            //CurBaseParam = surfaceParameter.PodSide;
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


        public bool bPenVisibility;

        private Visibility _bGBsimilar;
        public Visibility pGBsimilar
        {
            get => _bGBsimilar;
            set => SetProperty(ref _bGBsimilar, value);
        }
        #endregion
    }
}
