using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Module;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class Recipe1um_ViewModel : ObservableObject
    {
        public RecipeMask_ViewModel recipeSetting;
        public Recipe1um_Panel Main;

        MaskRootViewer_ViewModel EIPcoverBottom, EIPcoverBottom_Teach, EIPbaseplateTop, EIPbaseplateTop_Teach,selectedViewer;
        ScrewUI_ViewModel EIPcoverBottom_Step, EIPbaseplate_step;
        int selectedTab;
        #region Property
        public int SelectedTab
        {
            get => selectedTab;
            set
            {
                SetProperty(ref selectedTab, value);
                if (value == 0)
                    selectedViewer = EIPCoverBottom;
                else
                    selectedViewer = EIPBasePlateTop;
            }
        }
        public MaskRootViewer_ViewModel SelectedViewer
        {
            get => selectedViewer;
            set => SetProperty(ref selectedViewer, value);
        }
        public MaskRootViewer_ViewModel EIPCoverBottom
        {
            get => EIPcoverBottom;
            set => SetProperty(ref EIPcoverBottom, value);
        }
        public MaskRootViewer_ViewModel EIPCoverBottom_Teach
        {
            get => EIPcoverBottom_Teach;
            set => SetProperty(ref EIPcoverBottom_Teach, value);
        }
        public MaskRootViewer_ViewModel EIPBasePlateTop
        {
            get => EIPbaseplateTop;
            set => SetProperty(ref EIPbaseplateTop, value);
        }
        public MaskRootViewer_ViewModel EIPBasePlateTop_Teach
        {
            get => EIPbaseplateTop_Teach;
            set => SetProperty(ref EIPbaseplateTop_Teach, value);
        }
        public ScrewUI_ViewModel EIPCoverBottom_Step
        {
            get => EIPcoverBottom_Step;
            set => SetProperty(ref EIPcoverBottom_Step, value);
        }
        public ScrewUI_ViewModel EIPBasePlate_Step
        {
            get => EIPbaseplate_step;
            set => SetProperty(ref EIPbaseplate_step, value);
        }
        #endregion
        public Recipe1um_ViewModel(RecipeMask_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
            Main = new Recipe1um_Panel();
            Main.DataContext = this;
            EIPcoverBottom = new MaskRootViewer_ViewModel("EIP_Cover.Main.Front", recipeSetting.MaskTools);
            EIPcoverBottom_Teach = new MaskRootViewer_ViewModel("EIP_Cover.Stack.Back", recipeSetting.MaskTools);
            EIPbaseplateTop = new MaskRootViewer_ViewModel("EIP_Plate.Main.Front", recipeSetting.MaskTools);
            EIPbaseplateTop_Teach = new MaskRootViewer_ViewModel("EIP_Cover.Stack.Back", recipeSetting.MaskTools);

            EIPcoverBottom_Step = new ScrewUI_ViewModel("EIP_Cover.Stack.Front");
            EIPbaseplate_step = new ScrewUI_ViewModel("EIP_Plate.Stack.Front");
        }

        public ICommand ImageOpen
        {
            get => new RelayCommand(() => selectedViewer._openImage());
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

            vision.StartRun((Run_ZStack)vision.CloneModuleRun(App.mZStack));
        }
    }
}
