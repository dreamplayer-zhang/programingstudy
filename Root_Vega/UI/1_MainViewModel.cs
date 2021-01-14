using System;
using RootTools;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.ObjectModel;
using RootTools.Module;
using Root_Vega.Module;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Threading;
using RootTools.Inspects;

namespace Root_Vega
{
    public class _1_Mainview_ViewModel : ObservableObject
    {
        public Dispatcher _dispatcher;
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
        DispatcherTimer m_dispatcherTimer;
        PatternVision m_PatternVision;
        public PatternVision p_PatternVision
        {
            get { return m_PatternVision; }
            set
            {
                SetProperty(ref m_PatternVision, value);
            }
        }
        SideVision m_SideVision;
        public SideVision p_SideVision
        {
            get { return m_SideVision; }
            set
            {
                SetProperty(ref m_SideVision, value);
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
        private MiniViewer_ViewModel m_MiniImageViewer_BevelTop;
        public MiniViewer_ViewModel p_MiniImageViewer_BevelTop
        {
            get 
            {
                return m_MiniImageViewer_BevelTop; 
            }
            set
            {
                SetProperty(ref m_MiniImageViewer_BevelTop, value);
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
        private MiniViewer_ViewModel m_MiniImageViewer_BevelBtm;
        public MiniViewer_ViewModel p_MiniImageViewer_BevelBtm
        {
            get
            {
                return m_MiniImageViewer_BevelBtm;
            }
            set
            {
                SetProperty(ref m_MiniImageViewer_BevelBtm, value);
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
        private MiniViewer_ViewModel m_MiniImageViewer_BevelLeft;
        public MiniViewer_ViewModel p_MiniImageViewer_BevelLeft
        {
            get
            {
                return m_MiniImageViewer_BevelLeft;
            }
            set
            {
                SetProperty(ref m_MiniImageViewer_BevelLeft, value);
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
        private MiniViewer_ViewModel m_MiniImageViewer_BevelRight;
        public MiniViewer_ViewModel p_MiniImageViewer_BevelRight
        {
            get
            {
                return m_MiniImageViewer_BevelRight;
            }
            set
            {
                SetProperty(ref m_MiniImageViewer_BevelRight, value);
            }
        }
        double m_dPatternInspProgress = 0.0;
        public double p_dPatternInspProgress
        {
            get { return m_dPatternInspProgress; }
            set
            {
                SetProperty(ref m_dPatternInspProgress, value);
            }
        }

        double m_dSideInspProgress = 0.0;
        public double p_dSideInspProgress
        {
            get { return m_dSideInspProgress; }
            set
            {
                SetProperty(ref m_dSideInspProgress, value);
            }
        }

        int m_nTotalDefectCount = 0;
        public int p_nTotalDefectCount
        {
            get { return m_nTotalDefectCount; }
            set
            {
                SetProperty(ref m_nTotalDefectCount, value);
            }
        }
        string m_sLotElapsedTime = "";
        public string p_sLotElapsedTime
        {
            get { return m_sLotElapsedTime; }
            set
            {
                SetProperty(ref m_sLotElapsedTime, value);
            }
        }

        private readonly IDialogService m_DialogService;

        public _1_Mainview_ViewModel(Vega_Engineer engineer, IDialogService dialogService)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            m_DialogService = dialogService;
            m_Engineer = engineer;
            m_dispatcherTimer = new DispatcherTimer();
            m_dispatcherTimer.Interval = TimeSpan.FromTicks(10000000);
            m_dispatcherTimer.Tick += new EventHandler(timer_Tick);
            m_dispatcherTimer.Start();
            p_Handler = (Vega_Handler)engineer.ClassHandler();
            p_Process = p_Handler.m_process;
            InitAlarmData();
            p_MiniImageViewer = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("PatternVision.Memory", "PatternVision", "Main")));
            p_MiniImageViewer_Btm = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "SideBottom")), true);
            p_MiniImageViewer_BevelBtm = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "BevelBottom")), true);
            p_MiniImageViewer_Top = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "SideTop")), true);
            p_MiniImageViewer_BevelTop = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "BevelTop")), true);
            p_MiniImageViewer_Left = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "SideLeft")));
            p_MiniImageViewer_BevelLeft = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "BevelLeft")));
            p_MiniImageViewer_Right = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "SideRight")));
            p_MiniImageViewer_BevelRight = new MiniViewer_ViewModel(new ImageData(p_Engineer.GetMemory("SideVision.Memory", "Grab", "BevelRight")));

            p_PatternVision = ((Vega_Handler)m_Engineer.ClassHandler()).m_patternVision;
            p_SideVision = ((Vega_Handler)m_Engineer.ClassHandler()).m_sideVision;

            App.m_engineer.m_InspManager.AddChromeDefect += App_AddDefectMain;
            App.m_engineer.m_InspManager.AddLeftSideDefect += App_AddDefectSideLeft;
            App.m_engineer.m_InspManager.AddLeftBevelDefect += App_AddDefectBevelLeft;
            App.m_engineer.m_InspManager.AddRightSideDefect += App_AddDefectSideRight;
            App.m_engineer.m_InspManager.AddRightBevelDefect += App_AddDefectBevelRight;
            App.m_engineer.m_InspManager.AddTopSideDefect += App_AddDefectSideTop;
            App.m_engineer.m_InspManager.AddTopBevelDefect += App_AddDefectBevelTop;
            App.m_engineer.m_InspManager.AddBotSideDefect += App_AddDefectSideBottom;
            App.m_engineer.m_InspManager.AddBotBevelDefect += App_AddDefectBevelBottom;

            App.m_engineer.m_InspManager.ClearDefect += _ClearDefect;
            InspectionManager.RefreshDefect += InspectionManager_RefreshDefect;
        }

        ~_1_Mainview_ViewModel()
        {
            App.m_engineer.m_InspManager.AddChromeDefect -= App_AddDefectMain;
            App.m_engineer.m_InspManager.AddLeftSideDefect -= App_AddDefectSideLeft;
            App.m_engineer.m_InspManager.AddLeftBevelDefect -= App_AddDefectBevelLeft;
            App.m_engineer.m_InspManager.AddRightSideDefect -= App_AddDefectSideRight;
            App.m_engineer.m_InspManager.AddRightBevelDefect -= App_AddDefectBevelRight;
            App.m_engineer.m_InspManager.AddTopSideDefect -= App_AddDefectSideTop;
            App.m_engineer.m_InspManager.AddTopBevelDefect -= App_AddDefectBevelTop;
            App.m_engineer.m_InspManager.AddBotSideDefect -= App_AddDefectSideBottom;
            App.m_engineer.m_InspManager.AddBotBevelDefect -= App_AddDefectBevelBottom;
        }

        private void App_AddDefectMain(RootTools.DefectDataWrapper item)
        {
            try
            {
                p_MiniImageViewer.lstDefect.Add(new CPoint((int)item.fPosX, (int)item.fPosY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void App_AddDefectSideLeft(RootTools.DefectDataWrapper item)
        {
            try
            {
                p_MiniImageViewer_Left.lstDefect.Add(new CPoint((int)item.fPosX, (int)item.fPosY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void App_AddDefectBevelLeft(RootTools.DefectDataWrapper item)
        {
            try
            {
                p_MiniImageViewer_BevelLeft.lstDefect.Add(new CPoint((int)item.fPosX, (int)item.fPosY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void App_AddDefectSideRight(RootTools.DefectDataWrapper item)
        {
            try
            {
                p_MiniImageViewer_Right.lstDefect.Add(new CPoint((int)item.fPosX, (int)item.fPosY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void App_AddDefectBevelRight(RootTools.DefectDataWrapper item)
        {
            try
            {
                p_MiniImageViewer_BevelRight.lstDefect.Add(new CPoint((int)item.fPosX, (int)item.fPosY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void App_AddDefectSideTop(RootTools.DefectDataWrapper item)
        {
            try
            {
                p_MiniImageViewer_Top.lstDefect.Add(new CPoint((int)item.fPosX, (int)item.fPosY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void App_AddDefectBevelTop(RootTools.DefectDataWrapper item)
        {
            try
            {
                p_MiniImageViewer_BevelTop.lstDefect.Add(new CPoint((int)item.fPosX, (int)item.fPosY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void App_AddDefectSideBottom(RootTools.DefectDataWrapper item)
        {
            try
            {
                p_MiniImageViewer_Btm.lstDefect.Add(new CPoint((int)item.fPosX, (int)item.fPosY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void App_AddDefectBevelBottom(RootTools.DefectDataWrapper item)
        {
            try
            {
                p_MiniImageViewer_BevelBtm.lstDefect.Add(new CPoint((int)item.fPosX, (int)item.fPosY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void InspectionManager_RefreshDefect()
        {
        }

        private void _ClearDefect()
        {
            try
            {
                p_MiniImageViewer.lstDefect.Clear();
                p_MiniImageViewer_Left.lstDefect.Clear();
                p_MiniImageViewer_BevelLeft.lstDefect.Clear();
                p_MiniImageViewer_Right.lstDefect.Clear();
                p_MiniImageViewer_BevelRight.lstDefect.Clear();
                p_MiniImageViewer_Top.lstDefect.Clear();
                p_MiniImageViewer_BevelTop.lstDefect.Clear();
                p_MiniImageViewer_Btm.lstDefect.Clear();
                p_MiniImageViewer_BevelBtm.lstDefect.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (p_PatternVision.p_nTotalBlockCount == 0) p_dPatternInspProgress = 0.0;
            else p_dPatternInspProgress = (double)p_Engineer.m_InspManager.p_nPatternInspDoneNum / (double)p_PatternVision.p_nTotalBlockCount * 100;

            if (p_SideVision.p_nTotalBlockCount == 0) p_dSideInspProgress = 0.0;
            else p_dSideInspProgress = (double)p_Engineer.m_InspManager.p_nSideInspDoneNum / (double)p_SideVision.p_nTotalBlockCount * 100;

            p_nTotalDefectCount = p_Engineer.m_InspManager.m_nTotalDefectCount;

            for (int i = 0; i < p_Engineer.m_handler.m_aLoadport.Length; i++)
            {
                if (p_Engineer.m_handler.m_aLoadport[i].m_swLotTime.IsRunning) p_sLotElapsedTime = p_Engineer.m_handler.m_aLoadport[i].m_swLotTime.ElapsedMilliseconds.ToString();
            }
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

        public bool bTest = false;

        public bool p_test
        {
            get
            {
                return bTest;
            }
            set
            {
                SetProperty(ref bTest, value);
            }
        }

        public void UpdateMiniViewer()
        {
            p_MiniImageViewer.SetImageSource();
            p_MiniImageViewer_Btm.SetImageSource();
            p_MiniImageViewer_BevelBtm.SetImageSource();
            p_MiniImageViewer_Top.SetImageSource();
            p_MiniImageViewer_BevelTop.SetImageSource();
            p_MiniImageViewer_Left.SetImageSource();
            p_MiniImageViewer_BevelLeft.SetImageSource();
            p_MiniImageViewer_Right.SetImageSource();
            p_MiniImageViewer_BevelRight.SetImageSource();
            p_test = !p_test;
        }

        public RelayCommand TestCommand
        {
            get
            {
                return new RelayCommand(UpdateMiniViewer);
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
    public class FDCConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BrushConverter bc = new BrushConverter();
            System.Windows.Media.Brush result = System.Windows.Media.Brushes.DimGray;
            bool state = (bool)value;

            switch (state)
            {
                case true: return Colors.Red;
                case false: return Colors.Green;
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