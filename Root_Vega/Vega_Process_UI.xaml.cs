using RootTools;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace Root_Vega
{
    /// <summary>
    /// Vega_Process_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Vega_Process_UI : UserControl
    {
        public Vega_Process_UI()
        {
            InitializeComponent();
        }

        Vega_Process m_process;
        public void Init(Vega_Process process)
        {
            m_process = process;
            this.DataContext = process;

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;

            treeReticleUI.Init(process.m_treeReticle);
            treeLocateUI.Init(process.m_treeLocate);
            treeSequenceUI.Init(process.m_treeSequence);
            process.RunTree(Tree.eMode.Init);
        }

        private void buttonClearInfoReticle_Click(object sender, RoutedEventArgs e)
        {
            m_process.ClearInfoReticle();
        }

        private void buttonSetRecover_Click(object sender, RoutedEventArgs e)
        {
            m_process.CalcRecover();
            EQ.p_eState = EQ.eState.Run;
        }

        private void buttonRunStep_Click(object sender, RoutedEventArgs e)
        {
            m_process.p_sInfo = m_process.RunNextSequence();
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            m_process.RunTree(Tree.eMode.Init);
        }
    }
}
