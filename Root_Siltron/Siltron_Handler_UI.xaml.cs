using RootTools.Module;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Root_Siltron
{
    /// <summary>
    /// Siltron_Handler_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Siltron_Handler_UI : UserControl
    {
        public Siltron_Handler_UI()
        {
            InitializeComponent();
        }

        Siltron_Handler m_handler;
        public void Init(Siltron_Handler handler)
        {
            if (handler == null) return; 
            m_handler = handler;
            DataContext = handler;
            if (handler.m_enginner != null) loginUI.Init(handler.m_enginner.m_login);
            moduleListUI.Init(handler.m_moduleList);
            recipeUI.Init(handler.m_recipe);
            processUI.Init(handler.m_process);
            gafUI.Init(handler.m_gaf);
            InitTabControl();
        }

        void InitTabControl()
        {
            if (m_handler == null) return; 
            if (m_handler.m_moduleList == null) return; 
            foreach (KeyValuePair<ModuleBase, UserControl> kv in m_handler.m_moduleList.m_aModule)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = kv.Key.p_id;
                tabItem.Content = kv.Value;
                tabItem.Background = m_handler.p_brushModule;
                tabModule.Items.Add(tabItem);
            }
        }
    }
}
