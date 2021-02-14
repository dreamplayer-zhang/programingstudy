using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    class FrontsideOrigin_ViewModel : ObservableObject, IPage
    {
        private readonly FrontsideOrigin_ImageViewer_ViewModel imageViewerVM;
        public FrontsideOrigin_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }


        public FrontsideOrigin_ViewModel()
        {
            if (GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr() == IntPtr.Zero && GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").m_eMode !=  ImageData.eMode.OtherPCMem)
                return;

            this.imageViewerVM = new FrontsideOrigin_ImageViewer_ViewModel();           
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());

            this.imageViewerVM.OriginBoxReset += OriginBoxReset_Callback;
            this.imageViewerVM.OriginPointDone += OriginPointDone_Callback;
            this.imageViewerVM.OriginBoxDone += OriginBoxDone_Callback;
            this.imageViewerVM.PitchPointDone += PitchPointDone_Callback;

        }

        public void LoadRecipe()
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            this.OriginX = originRecipe.OriginX;
            this.OriginY =  originRecipe.OriginY;

            this.OriginWidth = originRecipe.OriginWidth;
            this.OriginHeight = originRecipe.OriginHeight;

            this.PitchX = originRecipe.DiePitchX;
            this.PitchY = originRecipe.DiePitchY;

            ImageViewerVM.SetOriginBox(new CPoint(this.OriginX, this.OriginY), this.OriginWidth, this.OriginHeight, this.PitchX, this.PitchY);

            WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());
        }


        public void OriginBoxReset_Callback()
        {
            Clear();
        }

        public void OriginPointDone_Callback()
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            this.OriginX = originRecipe.OriginX;
            this.OriginY = originRecipe.OriginY;
        }

        public void PitchPointDone_Callback()
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            this.PitchX = originRecipe.DiePitchX;
            this.PitchY = originRecipe.DiePitchY;

            WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());
        }

        public void OriginBoxDone_Callback()
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            this.OriginX = originRecipe.OriginX;
            this.OriginY = originRecipe.OriginX;

            this.OriginWidth = originRecipe.OriginWidth;
            this.OriginHeight = originRecipe.OriginHeight;

            this.PitchX = originRecipe.DiePitchX;
            this.PitchY = originRecipe.DiePitchY;

            WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());
        }

        public void Clear()
        {
            this.ImageViewerVM.ClearOrigin(true);

            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
            originRecipe.Clear();

            this.OriginX = 0;
            this.OriginY = 0;
            this.OriginWidth = 0;
            this.OriginHeight = 0;
            this.PitchX = 0;
            this.PitchY = 0;

            WIND2EventManager.OnRecipeUpdated(this, new RecipeEventArgs());
        }



        #region [Properties]
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

        #region [Command]
        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    LoadRecipe();
                });
            }
        }

        public RelayCommand btnOriginClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Clear();
                });
            }
        }
        #endregion

    }
}
