using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Root_EFEM.Module;
using RootTools;
using RootTools.Module;

namespace Root_CAMELLIA
{
    /// <summary>
    /// HomeProgress_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomeProgress_UI : Window
    {
        static int nloadport = 2;
        StopWatch m_swWTR = new StopWatch();
        StopWatch[] m_swLoadport = new StopWatch[nloadport];
        StopWatch m_swAligner = new StopWatch();
        StopWatch m_swVision = new StopWatch();
        CAMELLIA_Handler m_handler;
        bool bShow = false;
        public HomeProgress_UI()
        {
            InitializeComponent();
            for(int i=0; i< nloadport; i++)
            {
                m_swLoadport[i] = new StopWatch();
            }
        }

        public void HomeProgressShow()
        {
            m_swWTR.Start();
            m_swLoadport[0].Start();
            m_swLoadport[1].Start();
            m_swAligner.Start();
            m_swVision.Start();
            bShow = true;
        }

        public void Init(CAMELLIA_Handler handler)
        {
            m_handler = handler;
            //this.Owner
            InitTimer();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MinizimeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            //this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void Reset()
        {
            m_swWTR.Reset();
            m_swLoadport[0].Reset();
            m_swLoadport[1].Reset();
            m_swAligner.Reset();
            m_swVision.Reset();
            bShow = false;
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
                if (bShow)
                {

                }
                else
                {
                    this.Close();
                }
                //return;
            }

            if (bShow)
            {
                this.Show();
            }
            else
            {
                return;
            }
            if (m_handler.m_wtr.p_eState == ModuleBase.eState.Home)
            {
                progressWTR.Foreground = Brushes.Green;
                progressWTR.Value = (int)(100 * Math.Min((double)((double)m_swWTR.ElapsedMilliseconds / (((WTR_RND)m_handler.m_wtr).m_secHome * 1000)), (double)1.0));
            }
            else if (m_handler.m_wtr.p_eState == ModuleBase.eState.Ready)
            {
                progressWTR.Foreground = Brushes.Blue;
                progressWTR.Value = 100;
            }
            else if (m_handler.m_wtr.p_eState == ModuleBase.eState.Error)
            {
                progressWTR.Foreground = Brushes.Red;
                //progressWTR.Background = Brushes.Crimson;
                //progressWTR.Value = 0;
                //progressWTR.Value = 0;    //working
            }
            else progressWTR.Value = 0;

            if (m_handler.m_loadport[0].p_eState == ModuleBase.eState.Home)
            {
                progressLP1.Foreground = Brushes.Green;
                progressLP1.Value = (int)(100 * Math.Min((double)((double)m_swLoadport[0].ElapsedMilliseconds / (m_handler.m_aLoadport[0].p_secHome * 1000)), (double)1.0));
            }
            else if (m_handler.m_loadport[0].p_eState == ModuleBase.eState.Ready)
            {
                progressLP1.Foreground = Brushes.Blue;
                progressLP1.Value = 100;
            }
            else if (m_handler.m_loadport[0].p_eState == ModuleBase.eState.Error)
            {
                progressLP1.Foreground = Brushes.Red;
               // progressLP1.Value = 0;    //working
            }
            else progressLP1.Value = 0;

            if (m_handler.m_loadport[1].p_eState == ModuleBase.eState.Home)
            {
                progressLP2.Foreground = Brushes.Green;
                progressLP2.Value = (int)(100 * Math.Min((double)((double)m_swLoadport[1].ElapsedMilliseconds / (m_handler.m_aLoadport[1].p_secHome * 1000)), (double)1.0));
            }
            else if (m_handler.m_loadport[1].p_eState == ModuleBase.eState.Ready)
            {
                progressLP2.Foreground = Brushes.Blue;
                progressLP2.Value = 100;
            }
            else if (m_handler.m_loadport[1].p_eState == ModuleBase.eState.Error)
            {
                progressLP2.Foreground = Brushes.Red;
                //progressLP2.Value = 0;    //working
            }
            else progressLP2.Value = 0;

            if (m_handler.m_Aligner.p_eState == ModuleBase.eState.Home)
            {
                progressAL.Foreground = Brushes.Green;
                progressAL.Value = (int)(100 * Math.Min((double)((double)m_swAligner.ElapsedMilliseconds / (20 * 1000)), (double)1.0));
            }
            else if (m_handler.m_Aligner.p_eState == ModuleBase.eState.Ready)
            {
                progressAL.Foreground = Brushes.Blue;
                progressAL.Value = 100;
            }
            else if (m_handler.m_Aligner.p_eState == ModuleBase.eState.Error)
            {
                progressAL.Foreground = Brushes.Red;
            }
            else progressAL.Value = 0;

            if (m_handler.m_camellia.p_eState == ModuleBase.eState.Home)
            {
                progressVS.Foreground = Brushes.Green;
                progressVS.Value = (int)(100 * Math.Min((double)((double)m_swVision.ElapsedMilliseconds / (30 * 1000)), (double)1.0));
            }
            else if (m_handler.m_camellia.p_eState == ModuleBase.eState.Ready)
            {
                progressVS.Foreground = Brushes.Blue;
                progressVS.Value = 100;
            }
            else if (m_handler.m_camellia.p_eState == ModuleBase.eState.Error)
            {
                progressVS.Foreground = Brushes.Red;
            }
            else progressVS.Value = 0;

            if (m_handler.m_wtr.p_eState == ModuleBase.eState.Ready && m_handler.m_loadport[0].p_eState == ModuleBase.eState.Ready &&
                m_handler.m_loadport[1].p_eState == ModuleBase.eState.Ready && m_handler.m_Aligner.p_eState == ModuleBase.eState.Ready
                && m_handler.m_camellia.p_eState == ModuleBase.eState.Ready)
            {
               // bShow = false;
                this.Close();
            }
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            bShow = false;
        }
    }
}
