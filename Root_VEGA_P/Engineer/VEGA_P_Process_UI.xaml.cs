using RootTools;
using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Root_VEGA_P.Engineer
{
    /// <summary>
    /// VEGA_P_Process_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VEGA_P_Process_UI : UserControl
    {
        public VEGA_P_Process_UI()
        {
            InitializeComponent();
        }

        VEGA_P_Process m_process; 
        public void Init(VEGA_P_Process process)
        {
            m_process = process;
            DataContext = process;

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;

            treeRootUI.Init(process.m_treeRoot);
            process.RunTree(Tree.eMode.Init);

            buttonSetRecover.IsEnabled = false;
            InitTimer();
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();

        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.2);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_process.OnTimer();
            bool bEQReady = (EQ.p_eState == EQ.eState.Ready);
            buttonClear.IsEnabled = bEQReady && (m_process.p_nQueue > 0);
            buttonSetRecover.IsEnabled = bEQReady && m_process.IsEnableRecover(); 
            buttonRecipeOpen.IsEnabled = bEQReady && !m_process.IsEnableRecover();
            buttonRunStep.IsEnabled = bEQReady && (m_process.p_nQueue > 0);
            buttonRun.IsEnabled = bEQReady && (m_process.p_nQueue > 0);
        }
        #endregion

        #region Recipe
        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            m_process.Clear(); 
        }

        private void buttonSetRecover_Click(object sender, RoutedEventArgs e)
        {
            m_process.CalcRecover(); 
        }

        private void buttonRecipeOpen_Click(object sender, RoutedEventArgs e)
        {
            m_process.RecipeOpen(); 
        }
        #endregion

        #region Run
        private void buttonRunStep_Click(object sender, RoutedEventArgs e)
        {
            m_process.StartRunStep(); 
        }

        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            m_process.StartRunProcess(); 
        }

        private void checkBoxRnR_Click(object sender, RoutedEventArgs e)
        {
            m_process.StartRunRnR(checkBoxRnR.IsChecked == true);
        }
        #endregion

    }
}
