using System.Windows.Controls;

namespace RootTools.SQLogs
{
    /// <summary>
    /// SQLog_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SQLog_UI : UserControl
    {
        public SQLog_UI()
        {
            InitializeComponent();
        }

        _SQLog m_sqLog; 
        public void Init(_SQLog sqLog)
        {
            m_sqLog = sqLog;
            this.DataContext = sqLog;
            treeRootUI.Init(sqLog.m_treeRoot);
            sqLog.RunTree(Trees.Tree.eMode.Init); 
        }
    }
}
