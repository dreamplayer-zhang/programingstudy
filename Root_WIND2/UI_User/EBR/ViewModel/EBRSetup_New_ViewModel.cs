using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                SetProperty<int>(ref this.originX, value);
            }
        }

        private int originY = 0;
        public int OriginY
        {
            get => this.originY;
            set
            {
                SetProperty<int>(ref this.originY, value);
            }
        }

        private int originWidth = 0;
        public int OriginWidth
        {
            get => this.originWidth;
            set
            {
                SetProperty<int>(ref this.originWidth, value);
            }
        }

        private int originHeight = 0;
        public int OriginHeight
        {
            get => this.originHeight;
            set
            {
                SetProperty<int>(ref this.originHeight, value);
            }
        }

        private int pitchX = 0;
        public int PitchX
        {
            get => this.pitchX;
            set
            {
                SetProperty<int>(ref this.pitchX, value);
            }
        }

        private int pitchY = 0;
        public int PitchY
        {
            get => this.pitchY;
            set
            {
                SetProperty<int>(ref this.pitchY, value);
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
                GrabModeBase mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_EdgeSideVision.m_aGrabMode[value];
                
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
            EBRParameter parameter = recipeEBR.GetItem<EBRParameter>();

            //this.OriginX = originRecipe.OriginX;
            //this.OriginY = originRecipe.OriginY;
            //this.OriginWidth = originRecipe.OriginWidth;
            //this.OriginHeight = originRecipe.OriginHeight;
            //this.PitchX = originRecipe.DiePitchX;
            //this.PitchY = originRecipe.DiePitchY;

            this.ParameterListVM.Init(parameter);
        }
    }
}
