using Root_EFEM.Module;
using Root_VEGA_D.Engineer;
using RootTools;
using RootTools.Module;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace Root_VEGA_D
{
    /// <summary>
    /// HomeProgress_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomeProgress_UI : Window
    {
        bool bShow = false;
        static int nloadport = 2;
        VEGA_D_Handler m_handler;
        public HomeProgress_UI()
        {
            InitializeComponent();
        }

        public void HomeProgressShow()
        {
            bShow = true;
        }

        public void Init(VEGA_D_Handler handler)
        {
            m_handler = handler;
            gridWTR_Home.DataContext = m_handler.m_wtr;
            gridLPA_Home.DataContext = m_handler.m_loadport[0];
            gridLPB_Home.DataContext = m_handler.m_loadport[1];
            gridVision_Home.DataContext = m_handler.m_vision;
            InitTimer();
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(200);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (EQ.p_eState != EQ.eState.Home)
            {
                bShow = false;
                //if(this.IsActive)
                //this.Close();
            }
            if (bShow) this.Show();
            if (m_handler.m_wtr.p_eState == ModuleBase.eState.Ready && m_handler.m_loadport[0].p_eState == ModuleBase.eState.Ready &&
                m_handler.m_loadport[1].p_eState == ModuleBase.eState.Ready && m_handler.m_vision.p_eState == ModuleBase.eState.Ready)
                this.Close();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            bShow = false;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MinizimeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
    public class HomeStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModuleBase.eState state = (ModuleBase.eState)value;
            switch (state)
            {
                case ModuleBase.eState.Init: return Brushes.MediumSlateBlue;
                case ModuleBase.eState.Ready: return Brushes.DarkGray;
                case ModuleBase.eState.Home: return Brushes.Yellow;
                default: return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
