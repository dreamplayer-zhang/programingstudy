using RootTools.Module;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Root_VEGA_P.Engineer
{
    /// <summary>
    /// VEGA_P_Handler_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VEGA_P_Handler_UI : UserControl
    {
        public VEGA_P_Handler_UI()
        {
            InitializeComponent();
        }

        VEGA_P_Handler m_handler;
        public void Init(VEGA_P_Handler handler)
        {
            m_handler = handler;
            DataContext = handler;
            moduleListUI.Init(handler.p_moduleList);
            recipeUI.Init(handler);
            //processUI.Init(handler.m_process); //forgetVegaP
            gafUI.Init(handler.m_gaf);
            InitTabControl();
        }

        void InitTabControl()
        {
            foreach (KeyValuePair<ModuleBase, UserControl> kv in m_handler.p_moduleList.m_aModule)
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
