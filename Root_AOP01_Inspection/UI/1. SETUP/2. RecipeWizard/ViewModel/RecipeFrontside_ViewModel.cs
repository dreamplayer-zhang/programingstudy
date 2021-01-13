using System.Windows.Input;
using Root_AOP01_Inspection.Module;

namespace Root_AOP01_Inspection
{
    class RecipeFrontside_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        AOP01_Engineer m_Engineer;
        public RecipeFrontside_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Engineer = setup.m_MainWindow.m_engineer;
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
                        mainVision.StartRun(alignKeyInspection);
                    }

                    if (p_bEnablePatternShift)
                    {
                        MainVision.Run_ShiftAndRotation shiftAndRotation = (MainVision.Run_ShiftAndRotation)mainVision.CloneModuleRun("ShiftAndRotation");
                        mainVision.StartRun(shiftAndRotation);
                    }

                    if (p_bEnableBarcodeInsp)
                    {
                        MainVision.Run_BarcodeInspection barcodeInspection = (MainVision.Run_BarcodeInspection)mainVision.CloneModuleRun("BarcodeInspection");
                        mainVision.StartRun(barcodeInspection);
                    }
                });
            }
        }
        #endregion
    }
}
