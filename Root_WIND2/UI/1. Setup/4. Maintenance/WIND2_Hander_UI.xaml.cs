using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Root_WIND2
{
    /// <summary>
    /// WIND2_Hander_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WIND2_Hander_UI : UserControl
    {
        WIND2_Handler m_handler;
        public WIND2_Hander_UI()
        {
            InitializeComponent();
        }

        public void Init(WIND2_Handler handler)
        {
            m_handler = handler;
            DataContext = handler;
            moduleListUI.Init(handler.p_moduleList);
            processUI.Init(handler.p_process);
            recipeUI.Init(handler.m_recipe);
            gafUI.Init(handler.m_gaf);
            
            treeRootUI.Init(handler.m_engineer.m_treeRoot);
            handler.m_engineer.RunTree(Tree.eMode.Init);

            InitTabHandler(); 
            InitTabModule();
        }

        public ModuleList_UI GetModuleList_UI()
        {
            return moduleListUI;
        }

        void InitTabHandler()
        {

        }

        void InitTabModule()
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
