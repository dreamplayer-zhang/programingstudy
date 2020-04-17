using RootTools.Module;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Root_Wind
{
    /// <summary>
    /// Wind_Handler_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Wind_Handler_UI : UserControl
    {
        public Wind_Handler_UI()
        {
            InitializeComponent();
        }

        Wind_Handler m_handler;
        public void Init(Wind_Handler handler)
        {
            m_handler = handler;
            this.DataContext = handler;
            moduleListUI.Init(handler.m_moduleList);
            recipeUI.Init(handler.m_recipe);
            processUI.Init(handler.m_process); 
            gafUI.Init(handler.m_gaf);
            //GAFManagerUI.DataContext = handler.m_GAF;
            //GAFManagerUI. = handler.m_GAF;
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
