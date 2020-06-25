using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace RootTools.Control.Ajin
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AjinAxis_UI : UserControl
    {
        public AjinAxis_UI()
        {
            InitializeComponent();
        }

        AjinAxis m_axis;
        public void Init(AjinAxis axis)
        {
            this.DataContext = axis;
            m_axis = axis;
            //treeRootMainUI.Init(axis.p_treeRootMain);
            //axis.RunTree(Tree.eMode.Init);
            //axis.RunSetupTree(Tree.eMode.Init); 
            StartTimer(); 
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer(); 
        void StartTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            Timer_Status();
        }
        #endregion

        #region Status
        void Timer_Status()
        {
            Timer_Status(buttonServoOn, m_axis.p_bSeroOn); 
        }

        void Timer_Status(Button button, bool bOn)
        {
            button.Background = bOn ? Brushes.Yellow : Brushes.LightGray;
            button.Foreground = bOn ? Brushes.Black : Brushes.DarkGray; 
        }

        private void buttonServoOn_Click(object sender, RoutedEventArgs e)
        {
            m_axis.ServoOn(!m_axis.p_bSeroOn);
        }

        #endregion
    }
}
