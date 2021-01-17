using System.Windows.Input;
using Root_AOP01_Inspection.Module;
using RootTools;

namespace Root_AOP01_Inspection
{
    class RecipeFrontside_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        AOP01_Engineer m_Engineer;
        MainVision m_mainVision;
        public MainVision p_mainVision
        {
            get { return m_mainVision; }
            set { SetProperty(ref m_mainVision, value); }
        }
        public RecipeFrontside_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Engineer = setup.m_MainWindow.m_engineer;
            m_mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;

            p_ImageViewer_VM = new RootViewer_ViewModel();
            p_ImageViewer_VM.init(ProgramManager.Instance.ImageMain);
        }

        #region Property
        bool m_bEnableAlignKeyInsp = false;
        public bool p_bEnableAlignKeyInsp
        {
            get { return m_bEnableAlignKeyInsp; }
            set { SetProperty(ref m_bEnableAlignKeyInsp, value); }
        }
        bool m_bEnableBarcodeInsp = false;
        public bool p_bEnableBarcodeInsp
        {
            get { return m_bEnableBarcodeInsp; }
            set { SetProperty(ref m_bEnableBarcodeInsp, value); }
        }
        bool m_bEnablePatternShift = false;
        public bool p_bEnablePatternShift
        {
            get { return m_bEnablePatternShift; }
            set { SetProperty(ref m_bEnablePatternShift, value); }
        }

        #region Align Key Parameter
        double m_dAlignKeyTemplateMatchingScore = 90;
        public double p_dAlignKeyTemplateMatchingScore
        {
            get { return m_dAlignKeyTemplateMatchingScore; }
            set { SetProperty(ref m_dAlignKeyTemplateMatchingScore, value); }
        }

        int m_nAlignKeyNGSpec_um = 30;
        public int p_nAlignKeyNGSpec_um
        {
            get { return m_nAlignKeyNGSpec_um; }
            set { SetProperty(ref m_nAlignKeyNGSpec_um, value); }
        }
        #endregion

        #region Pattern Shift & Rotation Parameter
        double m_dPatternShiftAndRotationTemplateMatchingScore = 90;
        public double p_dPatternShiftAndRotationTemplateMatchingScore
        {
            get { return m_dPatternShiftAndRotationTemplateMatchingScore; }
            set { SetProperty(ref m_dPatternShiftAndRotationTemplateMatchingScore, value); }
        }

        double m_dPatternShiftAndRotationShiftSpec = 0.5;
        public double p_dPatternShiftAndRotationShiftSpec
        {
            get { return m_dPatternShiftAndRotationShiftSpec; }
            set { SetProperty(ref m_dPatternShiftAndRotationShiftSpec, value); }
        }

        double m_dPatternShiftAndRotationRotationSpec = 0.5;
        public double p_dPatternShiftAndRotationRotationSpec
        {
            get { return m_dPatternShiftAndRotationRotationSpec; }
            set { SetProperty(ref m_dPatternShiftAndRotationRotationSpec, value); }
        }
        #endregion

        #region Barcode Inspection Parameter
        int m_nBarcodeThreshold = 70;
        public int p_nBarcodeThreshold
        {
            get { return m_nBarcodeThreshold; }
            set { SetProperty(ref m_nBarcodeThreshold, value); }
        }
        #endregion

        #endregion

        #region RootViewer
        private RootViewer_ViewModel m_ImageViewer_VM;
        public RootViewer_ViewModel p_ImageViewer_VM
        {
            get
            {
                return m_ImageViewer_VM;
            }
            set
            {
                SetProperty(ref m_ImageViewer_VM, value);
            }
        }
        #endregion

        #region RelayCommand
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeWizardPanel();
                });
            }
        }
        public ICommand btnSnap
        {
            get
            {
                return new RelayCommand(() => {
                    MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
                    MainVision.Run_Grab grab = (MainVision.Run_Grab)mainVision.CloneModuleRun("Run Grab");
                    mainVision.StartRun(grab);
                });
            }
        }

        public ICommand btnInspection
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MainVision mainVision = ((AOP01_Handler)m_Engineer.ClassHandler()).m_mainVision;
                    if (p_bEnableAlignKeyInsp)
                    {
                        MainVision.Run_AlignKeyInspection alignKeyInspection = (MainVision.Run_AlignKeyInspection)mainVision.CloneModuleRun("AlignKeyInspection");
                        alignKeyInspection.m_dMatchScore = p_dAlignKeyTemplateMatchingScore / 100;
                        alignKeyInspection.m_nNGSpec_um = p_nAlignKeyNGSpec_um;
                        mainVision.StartRun(alignKeyInspection);
                    }

                    if (p_bEnablePatternShift)
                    {
                        MainVision.Run_PatternShiftAndRotation patternShiftAndRotation = (MainVision.Run_PatternShiftAndRotation)mainVision.CloneModuleRun("PatternShiftAndRotation");
                        patternShiftAndRotation.m_dMatchScore = p_dPatternShiftAndRotationTemplateMatchingScore / 100;
                        patternShiftAndRotation.m_dNGSpecDistance_um = p_dPatternShiftAndRotationShiftSpec;
                        patternShiftAndRotation.m_dNGSpecDegree = p_dPatternShiftAndRotationRotationSpec;
                        mainVision.StartRun(patternShiftAndRotation);
                    }

                    if (p_bEnableBarcodeInsp)
                    {
                        MainVision.Run_BarcodeInspection barcodeInspection = (MainVision.Run_BarcodeInspection)mainVision.CloneModuleRun("BarcodeInspection");
                        barcodeInspection.m_nThreshold = p_nBarcodeThreshold;
                        mainVision.StartRun(barcodeInspection);
                    }
                });
            }
        }
        #endregion
    }
}
