using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_VEGA_P_Vision
{
    public class OriginViewerTab_ViewModel : ObservableObject
    {
        public OriginViewerTab_Panel Main;
        OriginImageViewer_ViewModel m2DTDIViewer, mStainViewer, mSideTopBtmViewer,mSideLeftRightViewer;
        OriginImageViewer_ViewModel selectedViewer;
        OriginInfo TDIorigin,stainOrigin,sideLROrigin,sideTBOrigin;
        EUVOriginRecipe originRecipe;

        int selectedIdx, sideselectedIdx;
        List<int> numList;
        OriginInfo TDIOrigin
        {
            get => TDIorigin;
            set => SetProperty(ref TDIorigin, value);
        }
        OriginInfo StainOrigin
        {
            get => stainOrigin;
            set => SetProperty(ref stainOrigin, value);
        }
        OriginInfo SideLROrigin
        {
            get => sideLROrigin;
            set => SetProperty(ref sideLROrigin, value);
        }
        OriginInfo SideTBOrigin
        {
            get => sideTBOrigin;
            set => SetProperty(ref sideTBOrigin, value);
        }
        #region Property
        public int SelectedIdx
        {
            get => selectedIdx;
            set
            {
                SetProperty(ref selectedIdx, value);
                selectedViewer.SelectedIdx = value;
            }
        }
        public List<int> MemNumList
        {
            get => numList;
            set => SetProperty(ref numList, value);
        }
        public OriginImageViewer_ViewModel p_2DOriginViewer
        {
            get => m2DTDIViewer;
            set => SetProperty(ref m2DTDIViewer, value);
        }
        public OriginImageViewer_ViewModel p_StainOriginViewer
        {
            get => mStainViewer;
            set => SetProperty(ref mStainViewer, value);
        }
        public OriginImageViewer_ViewModel p_SideOriginTopBtmViewer
        {
            get => mSideTopBtmViewer;
            set => SetProperty(ref mSideTopBtmViewer, value);
        }
        public OriginImageViewer_ViewModel p_SideOriginLeftRightViewer
        {
            get => mSideLeftRightViewer;
            set => SetProperty(ref mSideLeftRightViewer, value);
        }
        #endregion
        public OriginViewerTab_ViewModel()
        {
            Main = new OriginViewerTab_Panel();
            Main.DataContext = this;
            m2DTDIViewer = new OriginImageViewer_ViewModel("EIP_Cover.Main.Front");
            mStainViewer = new OriginImageViewer_ViewModel("EIP_Cover.Stain.Front");
            mSideTopBtmViewer = new OriginImageViewer_ViewModel("EIP_Cover.Top");
            mSideLeftRightViewer = new OriginImageViewer_ViewModel("EIP_Cover.Left");

            InitOriginViewer(p_2DOriginViewer);
            InitOriginViewer(p_StainOriginViewer);
            InitOriginViewer(p_SideOriginTopBtmViewer);
            InitOriginViewer(p_SideOriginLeftRightViewer);

            originRecipe = GlobalObjects.Instance.Get<RecipeCoverFront>().GetItem<EUVOriginRecipe>();

            selectedIdx = 0;
            sideselectedIdx = 0;
            selectedViewer = p_2DOriginViewer;
            numList = new List<int>();
            for (int i = 0; i < m2DTDIViewer.p_ImageData.p_nPlane; i++)
                MemNumList.Add(i + 1);

            VegaPEventManager.RecipeUpdated += VegaPEventManager_RecipeUpdated;
        }

        private void VegaPEventManager_RecipeUpdated(object sender, RecipeEventArgs e)
        {
            RecipeBase recipe = e.recipe;
            originRecipe = recipe.GetItem<EUVOriginRecipe>();
            m2DTDIViewer.SetOriginBox(originRecipe.TDIOriginInfo.Origin, originRecipe.TDIOriginInfo.OriginSize);
            mStainViewer.SetOriginBox(originRecipe.StainOriginInfo.Origin, originRecipe.StainOriginInfo.OriginSize);
            mSideTopBtmViewer.SetOriginBox(originRecipe.SideTBOriginInfo.Origin, originRecipe.SideTBOriginInfo.OriginSize);
            mSideLeftRightViewer.SetOriginBox(originRecipe.SideLROriginInfo.Origin, originRecipe.SideLROriginInfo.OriginSize);
        }

        void InitOriginViewer(OriginImageViewer_ViewModel viewer)
        {
            viewer.OriginBoxReset += OriginBoxReset_Callback;
            viewer.OriginPointDone += OriginPointDone_Callback;
            viewer.OriginBoxDone += OriginBoxDone_Callback;
        }

        #region CallbackFunc
        public void OriginBoxReset_Callback()
        {
            Clear();
        }

        public void OriginPointDone_Callback()
        {
        }

        public void OriginBoxDone_Callback()
        {
            //VegaPEventManager.OnRecipeUpdated(this, new RecipeEventArgs());
        }

        public void Clear()
        {
            selectedViewer.ClearObjects(true);
        }

        void SetOriginRecipe()
        {

        }
        #endregion

        #region RelayCommand
        public ICommand TabChanged
        {
            get => new RelayCommand(() => {
                selectedIdx = Main.MainTab.SelectedIndex;
                sideselectedIdx = Main.SideTab.SelectedIndex;
                switch (selectedIdx)
                {
                    case 0:
                        selectedViewer = p_2DOriginViewer;
                        break;
                    case 1:
                        selectedViewer = p_StainOriginViewer;
                        break;
                    case 2:
                        if (selectedIdx + sideselectedIdx == 2)
                        {
                            selectedViewer = p_SideOriginTopBtmViewer;
                        }
                        else if (selectedIdx + sideselectedIdx == 3)
                        {
                            selectedViewer = p_SideOriginLeftRightViewer;
                        }
                        break;
                }
            });
        }

        public ICommand ImageOpen
        {
            get => new RelayCommand(() => 
            selectedViewer._openImage(SelectedIdx-1));
        }
        public ICommand ImageSave
        {
            get => new RelayCommand(() => selectedViewer._saveImage());
        }
        public ICommand ImageClear
        {
            get => new RelayCommand(() => selectedViewer._clearImage());
        }
        public ICommand btnSnap
        {
            get => new RelayCommand(() => Snap());
        }
        void Snap()
        {
            EQ.p_bStop = false;
            Vision vision = GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().m_handler.m_vision;
            if (vision.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }
        }
        public ICommand btnSaveMasterImage
        {
            get => new RelayCommand(() => {
                selectedViewer._saveMasterImage();
            });
        }
        public ICommand btnLoadMasterImage
        {
            get => new RelayCommand(() => {
                selectedViewer._saveMasterImage();
            });
        }
        #endregion
    }
}
