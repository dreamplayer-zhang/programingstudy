using RootTools.Module;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Root
{
    /// <summary>
    /// Root_Handler_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Root_Handler_UI : UserControl
    {
        public Root_Handler_UI()
        {
            InitializeComponent();
        }

        Root_Handler m_handler; 
        public void Init(Root_Handler handler)
        {
            m_handler = handler;
            this.DataContext = handler;
            moduleListUI.Init(handler.m_moduleList);
            InitTabControl();
        }

        void InitTabControl()
        {
            foreach (KeyValuePair<ModuleBase, UserControl> kv in m_handler.m_moduleList.m_aModule)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = kv.Key.p_id;
                tabItem.Content = kv.Value;
                tabControl.Items.Add(tabItem);
            }
        }
    }
}
