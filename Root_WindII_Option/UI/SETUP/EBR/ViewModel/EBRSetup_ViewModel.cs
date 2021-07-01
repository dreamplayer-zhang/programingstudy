using Root_WindII_Option.Engineer;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Root_WindII_Option.UI
{
	public class EBRSetup_ViewModel : ObservableObject
	{
        #region [Properties]
        private EBR_ImageViewer_ViewModel imageViewerVM;
        public EBR_ImageViewer_ViewModel ImageViewerVM
        {
            get { return imageViewerVM; }
            set { SetProperty(ref imageViewerVM, value); }
        }

        #region [Origin Recipe]
        private int originX = 0;
        public int OriginX
        {
            get => this.originX;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<OriginRecipe>().OriginX = value;

                SetProperty<int>(ref this.originX, value);
            }
        }

        private int originY = 0;
        public int OriginY
        {
            get => this.originY;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<OriginRecipe>().OriginY = value;

                SetProperty<int>(ref this.originY, value);
            }
        }

        private int originWidth = 0;
        public int OriginWidth
        {
            get => this.originWidth;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<OriginRecipe>().OriginWidth = value;
                recipeEBR.GetItem<OriginRecipe>().DiePitchX = value;

                DiePitchX = value;
                SetProperty<int>(ref this.originWidth, value);
            }
        }

        private int originHeight = 0;
        public int OriginHeight
        {
            get => this.originHeight;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<OriginRecipe>().OriginHeight = value;
                recipeEBR.GetItem<OriginRecipe>().DiePitchY = value;

                DiePitchY = value;
                SetProperty<int>(ref this.originHeight, value);
            }
        }

        private int diePitchX = 0;
        public int DiePitchX
        {
            get => this.diePitchX;
            set
            {
                SetProperty<int>(ref this.diePitchX, value);
            }
        }

        private int diePitchY = 0;
        public int DiePitchY
        {
            get => this.diePitchY;
            set
            {
                SetProperty<int>(ref this.diePitchY, value);
            }
        }
        #endregion

        #region [Recipe]
        private int firstNotch = 0;
        public int FirstNotch
        {
            get => this.firstNotch;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRRecipe>().FirstNotch = value;

                SetProperty<int>(ref this.firstNotch, value);
            }
        }

        private int lastNotch = 0;
        public int LastNotch
        {
            get => this.lastNotch;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRRecipe>().LastNotch = value;

                SetProperty<int>(ref this.lastNotch, value);
            }
        }
		#endregion

		#region [Parameter]
		private EBRParameter parameter;
        public EBRParameter Parameter
        {
            get
            {
                return parameter;
            }
            set
            {
                SetProperty(ref parameter, value);
            }
        }

        private ProcessMeasurementParameter processParameter;
        public ProcessMeasurementParameter ProcessParameter
        {
            get
            {
                return processParameter;
            }
            set
            {
                SetProperty(ref processParameter, value);
            }
        }
        #endregion

        #region [Grab Mode]
        public List<string> GrabModeList
        {
            get
            {
                return ((WindII_Option_Handler)GlobalObjects.Instance.Get<WindII_Option_Engineer>().ClassHandler()).p_VisionEdge.p_asGrabMode;
            }
        }

        private int selectedGrabModeIndex = 0;
        public int SelectedGrabModeIndex
        {
            get => this.selectedGrabModeIndex;
            set
            {
                WindII_Option_Engineer engineer = GlobalObjects.Instance.Get<WindII_Option_Engineer>();
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();

                recipeEBR.CameraInfoIndex = value;

                CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_VisionEdge.GetGrabMode(recipeEBR.CameraInfoIndex));
                this.CamInfoDataListVM.Init(camInfo);

                SetProperty<int>(ref this.selectedGrabModeIndex, value);
            }
        }

        private DataListView_ViewModel camInfoDataListVM;

        public DataListView_ViewModel CamInfoDataListVM
        {
            get => this.camInfoDataListVM;
            set
            {
                SetProperty(ref this.camInfoDataListVM, value);
            }
        }
        #endregion
        
        #endregion

        public EBRSetup_ViewModel()
		{
			if (GlobalObjects.Instance.GetNamed<ImageData>("EBRImage").GetPtr() == IntPtr.Zero)
				return;

			ImageViewerVM = new EBR_ImageViewer_ViewModel();
			ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EBRImage"), GlobalObjects.Instance.Get<DialogService>());

            RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();
            Parameter = recipe.GetItem<EBRParameter>();
            ProcessParameter = recipe.GetItem<ProcessMeasurementParameter>();

            this.camInfoDataListVM = new DataListView_ViewModel();
        }

        public void LoadParameter()
		{
            RecipeEBR recipe = GlobalObjects.Instance.Get<RecipeEBR>();
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
            EBRRecipe ebrRecipe = recipe.GetItem<EBRRecipe>();

            if (originRecipe == null || ebrRecipe == null)
                return;

			this.OriginX = originRecipe.OriginX;
			this.OriginY = originRecipe.OriginY;
			this.OriginWidth = originRecipe.OriginWidth;
			this.OriginHeight = originRecipe.OriginHeight;
			this.DiePitchX = originRecipe.DiePitchX;
			this.DiePitchY = originRecipe.DiePitchY;

            this.FirstNotch = ebrRecipe.FirstNotch;
            this.LastNotch = ebrRecipe.LastNotch;

            Parameter = recipe.GetItem<EBRParameter>();
            ProcessParameter = recipe.GetItem<ProcessMeasurementParameter>();

            this.SelectedGrabModeIndex = recipe.CameraInfoIndex;
        }
    }
}
