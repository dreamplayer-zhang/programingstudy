using RootTools.Module;
using RootTools.Trees;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_Pine2.Module
{
    /// <summary>
    /// LoadEV_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoadEV_UI : UserControl
    {
        public LoadEV_UI()
        {
            InitializeComponent();
        }

        LoadEV m_loadEV;
        public void Init(LoadEV loadEV)
        {
            m_loadEV = loadEV;
            DataContext = loadEV;
            treeRootUI.Init(loadEV.m_treeRootQueue);
            loadEV.RunTreeQueue(Tree.eMode.Init);
        }

        public void OnTimer()
        {
            switch (m_loadEV.p_eState)
            {
                case ModuleBase.eState.Init: Background = Brushes.White; break;
                case ModuleBase.eState.Home: Background = Brushes.MediumPurple; break;
                case ModuleBase.eState.Ready: Background = Brushes.LightGreen; break;
                case ModuleBase.eState.Run: Background = Brushes.Yellow; break;
                case ModuleBase.eState.Error: Background = Brushes.OrangeRed; break;
            }
            textBlockDone.Text = m_loadEV.p_bDone ? "Done" : "";
            OnRunTree();
        }

        int[] m_nQueue = new int[2] { 0, 0 };
        private void OnRunTree()
        {
            if ((m_loadEV.m_qModuleRun.Count == m_nQueue[0]) && (m_loadEV.m_qModuleRemote.Count == m_nQueue[1])) return;
            m_nQueue[0] = m_loadEV.m_qModuleRun.Count;
            m_nQueue[1] = m_loadEV.m_qModuleRemote.Count;
            m_loadEV.RunTreeQueue(Tree.eMode.Init);
        }

        private void gridLoad_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_loadEV.StartLoad(); 
        }

        private void gridInfo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (m_loadEV.p_eState)
            {
                case ModuleBase.eState.Init: m_loadEV.p_eState = ModuleBase.eState.Home; break;
                case ModuleBase.eState.Error: m_loadEV.Reset(); break;
            }
        }
    }
}
