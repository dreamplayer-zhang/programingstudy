using Root_EFEM.Module;
using Root_VEGA_D.Engineer;
using RootTools;
using RootTools.Module;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_VEGA_D
{
    /// <summary>
    /// HomeProgress_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomeProgress_UI : Window
    {
        bool bShow = false;
        static int nloadport = 2;
        StopWatch m_swWTR = new StopWatch();
        StopWatch[] m_swLoadport = new StopWatch[nloadport];
        StopWatch m_swVision = new StopWatch();
        VEGA_D_Handler m_handler;
        public HomeProgress_UI()
        {
            InitializeComponent();
            for (int i = 0; i < nloadport; i++)
            {
                m_swLoadport[i] = new StopWatch();
            }
        }

        public void HomeProgressShow()
        {
            m_swWTR.Start();
            m_swLoadport[0].Start();
            m_swLoadport[1].Start();
            m_swVision.Start();
            bShow = true;
        }

        public void Init(VEGA_D_Handler handler)
        {
            m_handler = handler;
            InitTimer();
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (EQ.p_eState != EQ.eState.Home)
            {
                bShow = false;
                this.Close();
            }

            if (bShow) this.Show();
            if (m_handler.m_wtr.p_eState == ModuleBase.eState.Home)
            {
                progressWTR.Value = (int)(100 * Math.Min((double)((double)m_swWTR.ElapsedMilliseconds / (((WTR_Cymechs)m_handler.m_wtr).m_secHome * 1000)), (double)1.0));
            }
            else if (m_handler.m_wtr.p_eState == ModuleBase.eState.Ready)
            {
                progressWTR.Value = 100;
                m_swLoadport[0].Start();
                m_swLoadport[1].Start();
                m_swVision.Start();
            }
            else if (m_handler.m_wtr.p_eState == ModuleBase.eState.Error) progressWTR.Value = 0;    //working
            else progressWTR.Value = 0;

            if (m_handler.m_loadport[0].p_eState == ModuleBase.eState.Home)
            {
                progressLP1.Value = (int)(100 * Math.Min((double)((double)m_swLoadport[0].ElapsedMilliseconds / (m_handler.m_aLoadport[0].p_secHome * 1000)), (double)1.0));
            }
            else if (m_handler.m_loadport[0].p_eState == ModuleBase.eState.Ready) progressLP1.Value = 100;
            else if (m_handler.m_loadport[0].p_eState == ModuleBase.eState.Error) progressLP1.Value = 0;    //working
            else progressLP1.Value = 0;

            if (m_handler.m_loadport[1].p_eState == ModuleBase.eState.Home)
            {
                progressLP2.Value = (int)(100 * Math.Min((double)((double)m_swLoadport[1].ElapsedMilliseconds / (m_handler.m_aLoadport[1].p_secHome * 1000)), (double)1.0));
            }
            else if (m_handler.m_loadport[1].p_eState == ModuleBase.eState.Ready) progressLP2.Value = 100;
            else if (m_handler.m_loadport[1].p_eState == ModuleBase.eState.Error) progressLP2.Value = 0;    //working
            else progressLP2.Value = 0;

            if (m_handler.m_vision.p_eState == ModuleBase.eState.Home)
            {
                progressVS.Value = (int)(100 * Math.Min((double)((double)m_swVision.ElapsedMilliseconds / (m_handler.m_vision.m_secHome * 1000)), (double)1.0));
            }
            else if (m_handler.m_vision.p_eState == ModuleBase.eState.Ready) progressVS.Value = 100;
            else if (m_handler.m_vision.p_eState == ModuleBase.eState.Error) progressVS.Value = 0;    //working
            else progressVS.Value = 0;

            //if (m_handler.m_camellia.p_eState == ModuleBase.eState.Home)
            //{
            //    progressVS.Value = (int)(100 * Math.Min((m_swAligner.ElapsedMilliseconds / (20 * 1000)), 1.0));
            //}
            //else if (m_handler.m_camellia.p_eState == ModuleBase.eState.Ready) progressVS.Value = 100;
            //else if (m_handler.m_camellia.p_eState == ModuleBase.eState.Error) progressVS.Value = 0;    //working
            //else progressVS.Value = 0;

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
}
