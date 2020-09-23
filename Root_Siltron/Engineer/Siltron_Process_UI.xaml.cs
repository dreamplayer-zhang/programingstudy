using RootTools;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace Root_Siltron
{
    /// <summary>
    /// Siltron_Process_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Siltron_Process_UI : UserControl
    {
        public Siltron_Process_UI()
        {
            InitializeComponent();
        }

        Siltron_Process m_process;
        public void Init(Siltron_Process process)
        {
            if (process == null) return; 
            m_process = process;
            this.DataContext = process;

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;

            treeReticleUI.Init(process.m_treeWafer);
            treeLocateUI.Init(process.m_treeLocate);
            treeSequenceUI.Init(process.m_treeSequence);
            process.RunTree(Tree.eMode.Init);
        }

        private void buttonClearInfoReticle_Click(object sender, RoutedEventArgs e)
        {
            m_process.ClearInfoWafer(); 
        }

        private void buttonSetRecover_Click(object sender, RoutedEventArgs e)
        {
            m_process.CalcRecover(); 
        }

        private void buttonRunStep_Click(object sender, RoutedEventArgs e)
        {
            m_process.p_sInfo = m_process.RunNextSequence(); 
        }
    }
}
