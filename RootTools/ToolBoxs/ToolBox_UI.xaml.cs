using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools.ToolBoxs
{
    /// <summary>
    /// ToolBox_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolBox_UI : UserControl
    {
        public ToolBox_UI()
        {
            InitializeComponent();
        }

        ToolBox m_toolBox;
        public void Init(ToolBox toolBox)
        {
            m_toolBox = toolBox;
            this.DataContext = m_toolBox;
            foreach (KeyValuePair<IToolSet, UserControl> kv in toolBox.m_aToolSet)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = kv.Key.p_id;
                tabItem.Content = kv.Value;
                tabItem.Background = Brushes.DarkSalmon;
                tabControl.Items.Add(tabItem);
            }
        }
    }
}
