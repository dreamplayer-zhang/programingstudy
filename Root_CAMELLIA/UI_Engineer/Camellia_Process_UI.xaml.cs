using RootTools;
using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Root_CAMELLIA
{
    /// <summary>
    /// EFEM_Process_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CAMELLIA_Process_UI : UserControl
    {
        public CAMELLIA_Process_UI()
        {
            InitializeComponent();
        }

        CAMELLIA_Process m_process;
        public void Init(CAMELLIA_Process process)
        {
            m_process = process;
            this.DataContext = process;

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;

            treeInfoWaferUI.Init(process.m_treeWafer);
            treeLocateUI.Init(process.m_treeLocate);
            treeSequenceUI.Init(process.m_treeSequence);
            process.RunTree(Tree.eMode.Init);

            buttonSetRecover.IsEnabled = false; 
            InitTimer(); 
        }

        #region Button
        private void buttonClearInfoReticle_Click(object sender, RoutedEventArgs e)
        {
            m_process.ClearInfoWafer();
        }

        private void buttonSetRecover_Click(object sender, RoutedEventArgs e)
        {
            //m_process.CalcRecover(); //forget Delete Button
        }

        private void buttonRunStep_Click(object sender, RoutedEventArgs e)
        {
            m_process.p_sInfo = m_process.RunNextSequence();
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();

        void InitTimer()
        {
            //m_timer.Interval = TimeSpan.FromSeconds(1);
            //m_timer.Tick += M_timer_Tick;
            //m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_process.RunTree(Tree.eMode.Init);
        }
        #endregion

    }
}
