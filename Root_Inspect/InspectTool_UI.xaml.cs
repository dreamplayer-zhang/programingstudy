using RootTools.Inspects;
using System.Windows.Controls;

namespace Root_Inspect
{
    /// <summary>
    /// InspectTool_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InspectTool_UI : UserControl
    {
        public InspectTool_UI()
        {
            InitializeComponent();
        }

        InspectTool m_inspectTool; 
        public void Init(InspectTool inspectTool)
        {
            m_inspectTool = inspectTool;
            this.DataContext = inspectTool;
            listViewInspect.ItemsSource = inspectTool.p_aDataLog;
        }
    }
}
