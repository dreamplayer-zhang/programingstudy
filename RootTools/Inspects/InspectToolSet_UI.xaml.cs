using System.Windows.Controls;

namespace RootTools.Inspects
{
    /// <summary>
    /// InspectToolSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InspectToolSet_UI : UserControl
    {
        public InspectToolSet_UI()
        {
            InitializeComponent();
        }

        InspectToolSet m_inspectToolSet;
        public void Init(InspectToolSet inspectToolSet)
        {
            m_inspectToolSet = inspectToolSet;
            inspectToolSet.OnChangeTool += InspectToolSet_OnChangeTool;
            InitTabControl();
        }

        private void InspectToolSet_OnChangeTool()
        {
            InitTabControl();
        }

        void InitTabControl()
        {
            tabControl.Items.Clear();
            foreach (InspectTool inspectTool in m_inspectToolSet.m_aInspectTool)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = inspectTool.p_id;
                tabItem.Content = inspectTool.p_ui;
                tabControl.Items.Add(tabItem);
            }
        }
    }
}
