using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools_Vision;
using RootTools_Vision.WorkManager3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_VEGA_P_Vision
{
    public class RecipeStain_ViewModel : ObservableObject
    {       
        public RecipeMask_ViewModel recipeSetting;
        public RecipeStain_Panel Main;
        MaskRootViewer_ViewModel selectedViewer;
        #region [ImageViewer ViewModel]
        MaskRootViewer_ViewModel EIPcovertop_ImageViewerVM, EIPcoverbottom_ImageViewerVM;
        MaskRootViewer_ViewModel EIPbasetop_ImageViewerVM, EIPbasebottom_ImageViewerVM;

        public MaskRootViewer_ViewModel EIPCoverTop_ImageViewerVM
        {
            get => EIPcovertop_ImageViewerVM;
            set => SetProperty(ref EIPcovertop_ImageViewerVM, value);
        }
        public MaskRootViewer_ViewModel EIPCoverBottom_ImaageViewerVM
        {
            get => EIPcoverbottom_ImageViewerVM;
            set => SetProperty(ref EIPcoverbottom_ImageViewerVM, value);
        }
        public MaskRootViewer_ViewModel EIPBaseTop_ImageViewerVM
        {
            get => EIPbasetop_ImageViewerVM;
            set => SetProperty(ref EIPbasetop_ImageViewerVM, value);
        }
        public MaskRootViewer_ViewModel EIPBaseBottom_ImageViewerVM
        {
            get => EIPbasebottom_ImageViewerVM;
            set => SetProperty(ref EIPbasebottom_ImageViewerVM, value);
        }
        public MaskRootViewer_ViewModel SelectedViewer
        {
            get => selectedViewer;
            set => SetProperty(ref selectedViewer, value);
        }
        #endregion

        public RecipeStain_ViewModel(RecipeMask_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
            Main = new RecipeStain_Panel();
            Main.DataContext = this;

            EIPcovertop_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Cover.Stain.Front", recipeSetting.MaskTools);
            EIPcoverbottom_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Cover.Stain.Back", recipeSetting.MaskTools);
            EIPbasetop_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Plate.Stain.Front", recipeSetting.MaskTools);
            EIPbasebottom_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Plate.Stain.Back", recipeSetting.MaskTools);

            selectedViewer = EIPCoverTop_ImageViewerVM;
        }

        #region RelayCommand

        public ICommand btnBack
        {
            get => new RelayCommand(() =>
            {
            });
        }
        public ICommand btnSnap
        {
            get => new RelayCommand(() => Snap());
        }
        public ICommand btnInsp
        {
            get => new RelayCommand(() => selectedViewer.Inspection());
        }
        public ICommand TabChanged
        {
            get => new RelayCommand(() => {
                switch (Main.MaskViewerTab.SelectedIndex)
                {
                    case 0:
                        selectedViewer = EIPCoverTop_ImageViewerVM;
                        break;
                    case 1:
                        selectedViewer = EIPCoverBottom_ImaageViewerVM;
                        break;
                    case 2:
                        selectedViewer = EIPBaseTop_ImageViewerVM;
                        break;
                    case 3:
                        selectedViewer = EIPBaseBottom_ImageViewerVM;
                        break;
                }
            });
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

            vision.StartRun((Run_StainGrab)vision.CloneModuleRun(App.mStainGrab));
        }
        #endregion


    }
}
