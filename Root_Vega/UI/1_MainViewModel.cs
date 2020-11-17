using System;
using RootTools;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.ObjectModel;
using RootTools.Module;
using System.Security.Cryptography.X509Certificates;

namespace Root_Vega
{
    class _1_Mainview_ViewModel : ObservableObject
    {
        Vega_Engineer m_Engineer;
        public Vega_Engineer p_Engineer
        {
            get
            {
                return m_Engineer;
            }
            set
            {
                SetProperty(ref m_Engineer, value);
            }
        }
        Vega_Handler m_Handler;
        public Vega_Handler p_Handler
        {
            get
            {
                return m_Handler;
            }
            set
            {
                SetProperty(ref m_Handler, value);
            }
        }
        Vega_Process m_Process;
        public Vega_Process p_Process
        {
            get
            {
                return m_Process;
            }
            set
            {
                SetProperty(ref m_Process, value);
            }
        }
        private MiniViewer_ViewModel m_MiniImageViewer;
        public MiniViewer_ViewModel p_MiniImageViewer
        {
            get
            {
                return m_MiniImageViewer;
            }
            set
            {
                SetProperty(ref m_MiniImageViewer, value);
            }
        }

        private MiniViewer_ViewModel m_MiniImageViewer_Top;
        public MiniViewer_ViewModel p_MiniImageViewer_Top
        {
            get
            {
                return m_MiniImageViewer_Top;
            }
            set
            {
                SetProperty(ref m_MiniImageViewer_Top, value);
            }
        }
        private MiniViewer_ViewModel m_MiniImageViewer_Btm;
        public MiniViewer_ViewModel p_MiniImageViewer_Btm
        {
            get
            {
                return m_MiniImageViewer_Btm;
            }
            set
            {
                SetProperty(ref m_MiniImageViewer_Btm, value);
            }
        }
        private MiniViewer_ViewModel m_MiniImageViewer_Left;
        public MiniViewer_ViewModel p_MiniImageViewer_Left
        {
            get
            {
                return m_MiniImageViewer_Left;
            }
            set
            {
                SetProperty(ref m_MiniImageViewer_Left, value);
            }
        }
        private MiniViewer_ViewModel m_MiniImageViewer_Right;
        public MiniViewer_ViewModel p_MiniImageViewer_Right
        {
            get
            {
                return m_MiniImageViewer_Right;
            }
            set
            {
                SetProperty(ref m_MiniImageViewer_Right, value);
            }
        }

        private readonly IDialogService m_DialogService;

        public _1_Mainview_ViewModel(Vega_Engineer engineer, IDialogService dialogService)
        {
            m_DialogService = dialogService;
            m_Engineer = engineer;
            p_Handler = (Vega_Handler)engineer.ClassHandler();
            p_Process = p_Handler.m_process;
            InitAlarmData();
            p_MiniImageViewer = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("PatternVision.Memory", "PatternVision", "Main")));
            //p_MiniImageViewer = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "SideTop")), false, true);
            p_MiniImageViewer_Btm = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "SideBottom")), true);
            p_MiniImageViewer_Top = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "SideTop")), true);
            p_MiniImageViewer_Left = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "SideLeft")));
            p_MiniImageViewer_Right = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "SideRight")));
        }

        void LoadLp1()
        {
        }

        void LoadLp2()
        {

        }

        public void SetAlarm()
        {
            //Random rand = new Random();
            //int nrand = rand.Next(4);

            //if (nrand == 0)
            //    ((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm1);
            //if (nrand == 1)
            //    ((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm2);
            //if (nrand == 2)
            //    ((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm3);
            //if (nrand == 3)
            //    ((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm4);
        }

        public void ClearAlarm()
        {
            //((GAF_Manager)m_Engineer.ClassGAFManager()).ClearAllAlarm();
        }

        enum eAlarm
        {
            TestAlarm1,
            TestAlarm2,
            TestAlarm3,
            TestAlarm4,
        }

        public void InitAlarmData()
        {
            //((GAF_Manager)m_Engineer.ClassGAFManager()).GetALID(this.ToString(), eAlarm.TestAlarm1, "Test Alarm 1", "이건 Test용으로 하는거");
            //((GAF_Manager)m_Engineer.ClassGAFManager()).GetALID(this.ToString(), eAlarm.TestAlarm2, "Test Alarm 2", "1. 공압이 떨어졌습니다. \n 2.선연결 확인해주세요.");
            //((GAF_Manager)m_Engineer.ClassGAFManager()).GetALID(this.ToString(), eAlarm.TestAlarm3, "Test Alarm 3", "1.어쩌구\n 2. 어쩌구");
            //((GAF_Manager)m_Engineer.ClassGAFManager()).GetALID(this.ToString(), eAlarm.TestAlarm4, "Test Alarm 4", "?!._-특수문자 테스트!@#$| 되는지 확인");
        }

        void TestFunction()
        {
            p_MiniImageViewer_Left.SetRoiRect();
            p_MiniImageViewer.SetRoiRect();
            p_MiniImageViewer.SetImageSource();
            p_MiniImageViewer_Btm.SetImageSource();
            p_MiniImageViewer_Top.SetImageSource();
            p_MiniImageViewer_Left.SetImageSource();
            p_MiniImageViewer_Right.SetImageSource();
            //((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm2);

            //if (EQ.p_eState == EQ.eState.Init)
            //    EQ.p_eState = EQ.eState.Error;
            //else
            //    EQ.p_eState = EQ.eState.Init;
        }

        void TestFunction2()
        {
            //((GAF_Manager)m_Engineer.ClassGAFManager()).SetAlarm(this.ToString(), eAlarm.TestAlarm1);
            //if (EQ.p_eState == EQ.eState.Init)
            //    EQ.p_eState = EQ.eState.Error;
            //else
            //    EQ.p_eState = EQ.eState.Init;
        }

        public RelayCommand TestCommand
        {
            get
            {
                return new RelayCommand(TestFunction);
            }
        }
        public RelayCommand TestCommand2
        {
            get
            {
                return new RelayCommand(TestFunction2);
            }
        }

        public RelayCommand LoadCommandLP1
        {
            get
            {
                return new RelayCommand(LoadLp1);
            }
        }

        public RelayCommand LoadCommandLP2
        {
            get
            {
                return new RelayCommand(LoadLp2);
            }
        }


    }
}

namespace MainViewerConverter
{
    public class ProcessColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            BrushConverter bc = new BrushConverter();
            System.Windows.Media.Brush Color_ProcessDone = System.Windows.Media.Brushes.SeaGreen;
            System.Windows.Media.Brush Color_Processing = (Brush)bc.ConvertFrom("#FFF3CD6E");
            System.Windows.Media.Brush Color_ProcessWait = System.Windows.Media.Brushes.DimGray;

            ModuleRunBase data = (ModuleRunBase)values[0];
            ObservableCollection<ModuleRunBase> datalist = (ObservableCollection<ModuleRunBase>)values[1];

            int nIndex = datalist.IndexOf(data);
            if (nIndex == 0)
                return Color_Processing;
            else if (nIndex > 0)
                return Color_ProcessWait;
            else
                return Color_ProcessDone;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class StringToModuleStateConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ModuleBase.eState state = (ModuleBase.eState)value;
            return state.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class VisibleLoadToLPStateConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = true;
            ModuleBase.eState state = (ModuleBase.eState)value;
            switch (state)
            {
                case ModuleBase.eState.Init:
                case ModuleBase.eState.Home:
                case ModuleBase.eState.Run:
                case ModuleBase.eState.Error:
                    result = false;
                    break;
                case ModuleBase.eState.Ready:
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class ColorLoadToLPStateConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Windows.Media.Brush result = System.Windows.Media.Brushes.SeaGreen;
            ModuleBase.eState state = (ModuleBase.eState)value;
            switch (state)
            {
                case ModuleBase.eState.Init:
                case ModuleBase.eState.Home:
                case ModuleBase.eState.Run:
                case ModuleBase.eState.Error:
                    result = Brushes.DarkGray;
                    break;
                case ModuleBase.eState.Ready:
                    result = Brushes.SeaGreen;
                    break;
                default:
                    result = Brushes.DimGray;
                    break;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class ColorToEQStateConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Windows.Media.Brush result = System.Windows.Media.Brushes.SeaGreen;
            EQ.eState state = (EQ.eState)value;

            switch (state)
            {
                case EQ.eState.Init:
                    result = Brushes.Yellow;
                    break;
                case EQ.eState.Home:
                    result = Brushes.MediumPurple;
                    break;
                case EQ.eState.Ready:
                    result = Brushes.Salmon;
                    break;
                case EQ.eState.Run:
                    result = Brushes.SeaGreen;
                    break;
                case EQ.eState.Error:
                    result = Brushes.Red;
                    break;
                default:
                    result = Brushes.Red;
                    break;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class ColorToModuleStateConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BrushConverter bc = new BrushConverter();
            System.Windows.Media.Brush result = System.Windows.Media.Brushes.DimGray;
            ModuleBase.eState state = (ModuleBase.eState)value;

            switch (state)
            {
                case ModuleBase.eState.Init:
                    result = Brushes.Yellow;
                    break;
                case ModuleBase.eState.Home:
                    result = Brushes.MediumPurple;
                    break;
                case ModuleBase.eState.Ready:
                    result = Brushes.Salmon;
                    break;
                case ModuleBase.eState.Run:
                    result = Brushes.SeaGreen;
                    break;
                case ModuleBase.eState.Error:
                    result = Brushes.Red;
                    break;
                default:
                    break;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


}
