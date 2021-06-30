using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class ImageNROI_ViewModel:ObservableObject
    {
        public ImageNROI_Panel Main;
        public PodInfo_ViewModel podInfo;
        MaskTools_ViewModel maskTools;
        MaskRootViewer_ViewModel selectedViewer;
        SurfaceParam_Tree_ViewModel surfaceParamTree;
        RecipeItemListView_ViewModel selectedItemList;
        RecipeItemBase recipeItem;
        public RecipeItemBase RecipeItem
        {
            get => recipeItem;
            set => SetProperty(ref recipeItem, value);
        }
        public RecipeItemListView_ViewModel SelectedItemList
        {
            get => selectedItemList;
            set => SetProperty(ref selectedItemList, value);
        }
        public SurfaceParam_Tree_ViewModel SurfaceParamTree
        {
            get => surfaceParamTree;
            set => SetProperty(ref surfaceParamTree, value);
        }

        public MaskRootViewer_ViewModel SelectedViewer
        {
            get => selectedViewer;
            set => SetProperty(ref selectedViewer, value);
        }
        public MaskTools_ViewModel MaskTools
        {
            get => maskTools;
            set => SetProperty(ref maskTools, value);
        }
        public ImageNROI_ViewModel(PodInfo_ViewModel podInfo)
        {
            this.podInfo = podInfo;
            Main = new ImageNROI_Panel();
            Main.DataContext = this;
            maskTools = new MaskTools_ViewModel();
            RecipeBase recipe = GlobalObjects.Instance.Get<RecipeCoverFront>();
            SelectedViewer = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front", maskTools,
                recipe,recipe.GetItem<EUVOriginRecipe>().TDIOriginInfo,recipe.GetItem<EUVPodSurfaceParameter>().PodTDI.MaskIndex);

            surfaceParamTree = new SurfaceParam_Tree_ViewModel(GlobalObjects.Instance.Get<RecipeCoverFront>().GetItem<EUVPodSurfaceParameter>().PodStain);
            selectedItemList = new RecipeItemListView_ViewModel();
            selectedItemList.Init(recipe.GetItem<LowResRecipe>(), true);
            selectedItemList.RecipeItemChanged += SelectedItemList_RecipeItemChanged;
            VegaPEventManager.ImageROIBtn += VegaPEventManager_ImageROIBtn;
        }

        private void SelectedItemList_RecipeItemChanged(RecipeItem item)
        {
            RecipeItemBase  _item;

            _item = (LowResRecipe)selectedItemList.RecipeObj;

            switch (podInfo.SelectedItem)
            {
                case "Particle":
                    _item = (LowResRecipe)selectedItemList.RecipeObj;
                    break;
                case "Stain":
                    _item = (StainRecipe)selectedItemList.RecipeObj;
                    break;
                case "HighRes":
                    _item = (HighResRecipe)selectedItemList.RecipeObj;
                    break;
                case "Side":
                    _item = (SideRecipe)selectedItemList.RecipeObj;
                    break;
            }

            recipeItem = _item;
        }

        private void VegaPEventManager_ImageROIBtn(object sender, ImageROIEventArgs e)
        {
            //selectedViewer.SetImageData(GlobalObjects.Instance.GetNamed<ImageData>(e.memstr));
            selectedViewer.Recipe = e.recipe;
            selectedItemList.Init(e.recipeItem, true);
            surfaceParamTree.SurfaceParameter = e.parameterBase;
        }

        public ICommand btnSaveMasterImage
        {
            get => new RelayCommand(() => {
                //RecipeVision recipe = GlobalObjects.Instance.Get<RecipeVision>();
                //recipe.SaveMasterImage()
            });
        }
        public ICommand btnLoadMasterImage
        {
            get => new RelayCommand(() => { });
        }
        public ICommand btnDot
        {
            get => new RelayCommand(() => { });
        
        }
        public ICommand btnRect
        {
            get => new RelayCommand(() => { });
        }
        public ICommand btnSelect
        {
            get => new RelayCommand(() => { });
        }
    }
}
