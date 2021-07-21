using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Root_VEGA_P_Vision.Engineer;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class RecipeSideImageViewers_ViewModel : ObservableObject
    {
        MaskRootViewer_ViewModel top_ViewerVM, bottom_ViewerVM, left_ViewerVM, right_ViewerVM;
        MaskRootViewer_ViewModel selectedViewer;
        RecipeSide_ViewModel recipeSide;
        EUVPodSurfaceParameterBase selectedParamBase;
        public EUVPodSurfaceParameterBase SelectedParamBase
        {
            get => selectedParamBase;
            set => SetProperty(ref selectedParamBase, value);
        }
        public RecipeSide_ViewModel RecipeSide
        {
            get => recipeSide;
            set => SetProperty(ref recipeSide, value);
        }
        int selectedTab;
        public int SelectedTab
        {
            get => selectedTab;
            set
            {
                switch (value)
                {
                    case 0:
                        SelectedViewer = top_ViewerVM;
                        break;
                    case 1:
                        SelectedViewer = bottom_ViewerVM;
                        break;
                    case 2:
                        SelectedViewer = left_ViewerVM;
                        break;
                    case 3:
                        SelectedViewer = right_ViewerVM;
                        break;
                }

                switch(value)
                {
                    case 0:
                    case 1:
                        SelectedParamBase = selectedViewer.Recipe.GetItem<EUVPodSurfaceParameter>().PodSideTB;
                        break;
                    case 2:
                    case 3:
                        SelectedParamBase = selectedViewer.Recipe.GetItem<EUVPodSurfaceParameter>().PodSideLR;
                        break;
                }
                if(recipeSide!=null)
                    recipeSide.recipeMask.SurfaceParameterBase = SelectedParamBase;
                SelectedViewer.SetMask();
                SetProperty(ref selectedTab, value);
            }
        }
        int selectedIdx;
        public int SelectedIdx
        {
            get => selectedIdx;
            set
            {
                SetProperty(ref selectedIdx, value);
                selectedViewer.SelectedIdx = value;
            }
        }
        public MaskRootViewer_ViewModel SelectedViewer
        {
            get => selectedViewer;
            set => SetProperty(ref selectedViewer, value);
        }
        #region Property
        public MaskRootViewer_ViewModel Top_ViewerVM
        {
            get => top_ViewerVM;
            set => SetProperty(ref top_ViewerVM, value);
        }
        public MaskRootViewer_ViewModel Bottom_ViewerVM
        {
            get => bottom_ViewerVM;
            set => SetProperty(ref bottom_ViewerVM, value);
        }
        public MaskRootViewer_ViewModel Left_ViewerVM
        {
            get => left_ViewerVM;
            set => SetProperty(ref left_ViewerVM, value);
        }
        public MaskRootViewer_ViewModel Right_ViewerVM
        {
            get => right_ViewerVM;
            set => SetProperty(ref right_ViewerVM, value);
        }
        #endregion
        RecipeSideImageViewers_Panel Main;
        public RecipeSideImageViewers_ViewModel(string parts,RecipeSide_ViewModel recipeSide)
        {
            Main = new RecipeSideImageViewers_Panel();
            Main.DataContext = this;
            RecipeBase recipe = GlobalObjects.Instance.Get<RecipeCoverFront>();
            this.recipeSide = recipeSide;
            if (parts.Contains("Cover"))
                recipe = GlobalObjects.Instance.Get<RecipeCoverFront>();
            else if (parts.Contains("Plate"))
                recipe = GlobalObjects.Instance.Get<RecipePlateFront>();

            top_ViewerVM = new MaskRootViewer_ViewModel(parts + ".Top", recipeSide.recipeMask.MaskTools,recipe,
                recipe.GetItem<EUVOriginRecipe>().SideTBOriginInfo,recipe.GetItem<EUVPodSurfaceParameter>().PodSideTB.MaskIndex);

            bottom_ViewerVM = new MaskRootViewer_ViewModel(parts+".Bottom", recipeSide.recipeMask.MaskTools, recipe,
                recipe.GetItem<EUVOriginRecipe>().SideTBOriginInfo,recipe.GetItem<EUVPodSurfaceParameter>().PodSideTB.MaskIndex);

            left_ViewerVM = new MaskRootViewer_ViewModel(parts+".Left", recipeSide.recipeMask.MaskTools, recipe,
                recipe.GetItem<EUVOriginRecipe>().SideLROriginInfo,recipe.GetItem<EUVPodSurfaceParameter>().PodSideLR.MaskIndex);

            right_ViewerVM = new MaskRootViewer_ViewModel(parts+".Right", recipeSide.recipeMask.MaskTools, recipe,
                recipe.GetItem<EUVOriginRecipe>().SideLROriginInfo, recipe.GetItem<EUVPodSurfaceParameter>().PodSideLR.MaskIndex);

            selectedViewer = Top_ViewerVM;
        }
        public RecipeSideImageViewers_ViewModel(string parts)
        {
            Main = new RecipeSideImageViewers_Panel();
            Main.DataContext = this;
            RecipeBase recipe = GlobalObjects.Instance.Get<RecipeCoverFront>();
            MaskTools_ViewModel maskTools = new MaskTools_ViewModel();
            if (parts.Contains("Cover"))
                recipe = GlobalObjects.Instance.Get<RecipeCoverFront>();
            else if (parts.Contains("Plate"))
                recipe = GlobalObjects.Instance.Get<RecipePlateFront>();

            top_ViewerVM = new MaskRootViewer_ViewModel(parts + ".Top", maskTools, recipe,
                recipe.GetItem<EUVOriginRecipe>().SideTBOriginInfo, recipe.GetItem<EUVPodSurfaceParameter>().PodSideTB.MaskIndex);

            bottom_ViewerVM = new MaskRootViewer_ViewModel(parts + ".Bottom", maskTools, recipe,
                recipe.GetItem<EUVOriginRecipe>().SideTBOriginInfo, recipe.GetItem<EUVPodSurfaceParameter>().PodSideTB.MaskIndex);

            left_ViewerVM = new MaskRootViewer_ViewModel(parts + ".Left", maskTools, recipe,
                recipe.GetItem<EUVOriginRecipe>().SideLROriginInfo, recipe.GetItem<EUVPodSurfaceParameter>().PodSideLR.MaskIndex);

            right_ViewerVM = new MaskRootViewer_ViewModel(parts + ".Right", maskTools, recipe,
                recipe.GetItem<EUVOriginRecipe>().SideLROriginInfo, recipe.GetItem<EUVPodSurfaceParameter>().PodSideLR.MaskIndex);

            selectedViewer = Top_ViewerVM;
        }
    }
}
