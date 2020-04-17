using System.Windows.Controls;

namespace RootTools.ToolBoxs
{
    /// <summary>
    /// ToolSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolSet_UI : UserControl
    {
        public ToolSet_UI()
        {
            InitializeComponent();
        }

        ToolSet m_toolSet;
        public void Init(ToolSet toolSet)
        {
            m_toolSet = toolSet;
            this.DataContext = toolSet;
            toolSet.OnChangeTool += ToolSet_OnChangeTool;
        }

        private void ToolSet_OnChangeTool()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControl.Items.Clear();
            foreach (ITool tool in m_toolSet.m_aTool)
            {
                if (tool != null)
                {
                    TabItem tabItem = new TabItem();
                    tabItem.Header = tool.p_id;
                    tabItem.Content = tool.p_ui;
                    tabControl.Items.Add(tabItem);
                }
            }
        }
    }
}
