using RootTools.Trees;
using System.Windows.Controls;

namespace RootTools.Comm
{
    /// <summary>
    /// RS232_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RS232_UI : UserControl
    {
        public RS232_UI()
        {
            InitializeComponent();
        }

        RS232 m_rs232;
        public void Init(RS232 rs232)
        {
            m_rs232 = rs232;
            this.DataContext = rs232;
            commLogUI.Init(rs232.m_commLog); 
            treeRootUI.Init(rs232.m_treeRoot);
            rs232.RunTree(Tree.eMode.Init);
        }
    }
}