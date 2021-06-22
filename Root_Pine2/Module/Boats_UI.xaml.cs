using Root_Pine2_Vision.Module;
using RootTools.Module;
using RootTools.Trees;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_Pine2.Module
{
    /// <summary>
    /// Boats_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Boats_UI : UserControl
    {
        public Boats_UI()
        {
            InitializeComponent();
        }

        Boats m_boats;
        public void Init(Boats boats)
        {
            m_boats = boats;
            DataContext = boats;
            treeRootUI.Init(boats.m_treeRootQueue);
            boats.RunTreeQueue(Tree.eMode.Init);
        }

        public void OnTimer()
        {
            switch (m_boats.p_eState)
            {
                case ModuleBase.eState.Init: Background = Brushes.White; break;
                case ModuleBase.eState.Home: Background = Brushes.MediumPurple; break;
                case ModuleBase.eState.Ready: Background = Brushes.LightGreen; break;
                case ModuleBase.eState.Run: Background = Brushes.Yellow; break;
                case ModuleBase.eState.Error: Background = Brushes.OrangeRed; break;
            }
            textBlockVision.Foreground = m_boats.m_vision.m_remote.p_bEnable ? Brushes.Red : Brushes.LightGray; 
            textBlockA.Text = (m_boats.m_aBoat[Vision2D.eWorks.A].p_infoStrip != null) ? m_boats.m_aBoat[Vision2D.eWorks.A].p_id : "";
            textBlockB.Text = (m_boats.m_aBoat[Vision2D.eWorks.B].p_infoStrip != null) ? m_boats.m_aBoat[Vision2D.eWorks.B].p_id : "";
            textBlockStepA.Text = m_boats.m_aBoat[Vision2D.eWorks.A].p_eStep.ToString();
            textBlockStepB.Text = m_boats.m_aBoat[Vision2D.eWorks.B].p_eStep.ToString();
            OnRunTree();
        }

        int[] m_nQueue = new int[2] { 0, 0 };
        private void OnRunTree()
        {
            if ((m_boats.m_qModuleRun.Count == m_nQueue[0]) && (m_boats.m_qModuleRemote.Count == m_nQueue[1])) return;
            m_nQueue[0] = m_boats.m_qModuleRun.Count;
            m_nQueue[1] = m_boats.m_qModuleRemote.Count;
            m_boats.RunTreeQueue(Tree.eMode.Init);
        }

        private void textBlockVision_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_boats.m_vision.m_remote.p_bEnable = !m_boats.m_vision.m_remote.p_bEnable; 
        }
    }
}
