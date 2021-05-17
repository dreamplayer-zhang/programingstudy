using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Root_WIND2.UI_User
{
	public class EBRSetup_New_ViewModel : ObservableObject
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
        private int roiWidth;
        public int ROIWidth
        {
            get => this.roiWidth;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRParameter>().ROIWidth = value;
                
                SetProperty(ref this.roiWidth, value);
            }
        }

        private int roiHeight;
        public int ROIHeight
        {
            get => this.roiHeight;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRParameter>().ROIHeight = value;

                SetProperty(ref this.roiHeight, value);
            }
        }

        private int notchY;
        public int NotchY
        {
            get => this.notchY;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRParameter>().NotchY = value;

                SetProperty(ref this.notchY, value);
            }
        }

        private double stepDegree;
        public double StepDegree
        {
            get => this.stepDegree;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRParameter>().StepDegree = value;

                SetProperty(ref this.stepDegree, value);
            }
        }

        private int xRange;
        public int XRange
        {
            get => this.xRange;
			set
			{
				RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
				recipeEBR.GetItem<EBRParameter>().XRange = value;

				SetProperty(ref this.xRange, value);
			}
		}

		private int diffEdge;
        public int DiffEdge
        {
            get => this.diffEdge;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRParameter>().DiffEdge = value;

                SetProperty(ref this.diffEdge, value);
            }
        }

        private int diffBevel;
        public int DiffBevel
        {
            get => this.diffBevel;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRParameter>().DiffBevel = value;

                SetProperty(ref this.diffBevel, value);
            }
        }

        private int diffEBR;
        public int DiffEBR
        {
            get => this.diffEBR;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRParameter>().DiffEBR = value;

                SetProperty(ref this.diffEBR, value);
            }
        }

        private int offsetBevel;
        public int OffsetBevel
        {
            get => this.offsetBevel;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRParameter>().OffsetBevel = value;

                SetProperty(ref this.offsetBevel, value);
            }
        }

        private int offsetEBR;
        public int OffsetEBR
        {
            get => this.offsetEBR;
            set
            {
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
                recipeEBR.GetItem<EBRParameter>().OffsetEBR = value;

                SetProperty(ref this.offsetEBR, value);
            }
        }
        #endregion

        #region [Grab Mode]
        public List<string> GrabModeList
        {
            get
            {
                return ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.p_asGrabMode;
            }
            set
            {

            }
        }

        private int selectedGrabModeIndex = 0;
        public int SelectedGrabModeIndex
        {
            get => this.selectedGrabModeIndex;
            set
            {
                //GrabModeBase mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];

                WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
                RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();

                recipeEBR.CameraInfoIndex = value;

                CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_EdgeSideVision.GetGrabMode(recipeEBR.CameraInfoIndex));
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
        
        private DataListView_ViewModel parameterListVM;
        public DataListView_ViewModel ParameterListVM
        {
            get => this.parameterListVM;
            set
            {
                SetProperty(ref this.parameterListVM, value);
            }
        }
        #endregion

        #region [Command]
        public RelayCommand btnCreateMap
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //this.CreateMap();
                });
            }
        }
        #endregion

        public EBRSetup_New_ViewModel()
		{
			if (GlobalObjects.Instance.GetNamed<ImageData>("EBRImage").GetPtr() == IntPtr.Zero)
				return;

			ImageViewerVM = new EBR_ImageViewer_ViewModel();
			ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EBRImage"), GlobalObjects.Instance.Get<DialogService>());

            this.camInfoDataListVM = new DataListView_ViewModel();
            this.parameterListVM = new DataListView_ViewModel();
        }

        public void LoadParameter()
		{
            RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
            OriginRecipe originRecipe = recipeEBR.GetItem<OriginRecipe>();
            EBRRecipe recipe = recipeEBR.GetItem<EBRRecipe>();
            EBRParameter parameter = recipeEBR.GetItem<EBRParameter>();

			this.OriginX = originRecipe.OriginX;
			this.OriginY = originRecipe.OriginY;
			this.OriginWidth = originRecipe.OriginWidth;
			this.OriginHeight = originRecipe.OriginHeight;
			this.DiePitchX = originRecipe.DiePitchX;
			this.DiePitchY = originRecipe.DiePitchY;

            this.FirstNotch = recipe.FirstNotch;
            this.LastNotch = recipe.LastNotch;

			this.ROIWidth = parameter.ROIWidth;
            this.ROIHeight = parameter.ROIHeight;
            this.NotchY = parameter.NotchY;
            this.StepDegree = parameter.StepDegree;
            this.XRange = parameter.XRange;
            this.DiffEdge = parameter.DiffEdge;
            this.DiffBevel = parameter.DiffBevel;
            this.DiffEBR = parameter.DiffEBR;
            this.OffsetBevel = parameter.OffsetBevel;
            this.OffsetEBR = parameter.OffsetEBR;

            this.SelectedGrabModeIndex = recipeEBR.CameraInfoIndex;
            //this.ParameterListVM.Init(parameter);
        }

        public void CreateMap()
		{
            return;

            RecipeEBR recipeEBR = GlobalObjects.Instance.Get<RecipeEBR>();
            EBRRecipe recipe = recipeEBR.GetItem<EBRRecipe>();
            EBRParameter parameter = recipeEBR.GetItem<EBRParameter>();

            int firstNotch = recipe.FirstNotch;
            int lastNotch = recipe.LastNotch;

            if (firstNotch == 0 || lastNotch == 0)
			{
                MessageBox.Show("Register Notch Postion First");
                return;
			}              
            
            if (OriginWidth == 0 || OriginHeight == 0)
			{
                MessageBox.Show("Register Origin Size First");
                return;
            }

            int bufferHeight = lastNotch - firstNotch;
            //int bufferHeightPerDegree = bufferHeight / 360;

            double stepDegree = parameter.StepDegree;
            int mapSizeY = (int)(360 / stepDegree);

            RecipeType_WaferMap waferMap = recipeEBR.WaferMap;
            waferMap.CreateWaferMap(1, mapSizeY, CHIP_TYPE.NORMAL);
        }
    }
}
