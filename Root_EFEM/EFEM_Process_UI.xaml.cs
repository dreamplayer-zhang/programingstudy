using RootTools;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace Root_EFEM
{
    /// <summary>
    /// EFEM_Process_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EFEM_Process_UI : UserControl
    {
        public EFEM_Process_UI()
        {
            InitializeComponent();
        }

        EFEM_Process m_process;
        public void Init(EFEM_Process process)
        {
            m_process = process;
            this.DataContext = process;

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;

//            treeInfoWaferUI.Init(process.m_treeReticle);
//            treeLocateUI.Init(process.m_treeLocate);
//            treeSequenceUI.Init(process.m_treeSequence);
//            process.RunTree(Tree.eMode.Init);
        }

        private void buttonClearInfoReticle_Click(object sender, RoutedEventArgs e)
        {
//            m_process.ClearInfoReticle();
        }

        private void buttonSetRecover_Click(object sender, RoutedEventArgs e)
        {
//            m_process.CalcRecover();
        }

        private void buttonRunStep_Click(object sender, RoutedEventArgs e)
        {
//            m_process.p_sInfo = m_process.RunNextSequence();
        }
    }
}
