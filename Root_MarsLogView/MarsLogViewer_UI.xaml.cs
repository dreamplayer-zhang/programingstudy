using RootTools.Trees;
using System.Windows.Controls;

namespace Root_MarsLogView
{
    /// <summary>
    /// MarsLogViewer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MarsLogViewer_UI : UserControl
    {
        public MarsLogViewer_UI()
        {
            InitializeComponent();
        }

        MarsLogViewer m_logViewer; 
        public void Init(MarsLogViewer logViewer)
        {
            m_logViewer = logViewer;
            DataContext = logViewer;
            listPRCUI.Init(logViewer.m_listPRC);
            listXFRUI.Init(logViewer.m_listXFR);
            listFNCUI.Init(logViewer.m_listFNC);
            listLEHUI.Init(logViewer.m_listLEH);
            tcpServer0UI.Init(logViewer.m_tcpServer[0]);
            tcpServer1UI.Init(logViewer.m_tcpServer[1]);
            treeUI.Init(logViewer.m_treeRoot);
            logViewer.RunTree(Tree.eMode.Init); 
        }
    }
}
