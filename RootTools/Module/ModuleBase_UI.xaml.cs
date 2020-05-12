using RootTools.Control;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools.Module
{
    /// <summary>
    /// ModuleBase_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModuleBase_UI : UserControl
    {
        public ModuleBase_UI()
        {
            InitializeComponent();
        }

        ModuleBase m_module;
        public void Init(ModuleBase module)
        {
            m_module = module;
            this.DataContext = module;
            
            labelEQState.DataContext = EQ.m_EQ; 
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ; 
            
            infoListUI.Init(module.m_infoList);
            listDIUI.Init(module.m_listDI);
            listDOUI.Init(module.m_listDO);
            treeRootUI.Init(module.m_treeRoot);
            treeToolUI.Init(module.m_treeToolBox);

            module.RunTree(Tree.eMode.Init);
            module.RunToolTree(Tree.eMode.Init);

            m_minTabControl = tabControlTools.Items.Count;
            InitTabControl();
            module.OnChangeTool += Module_OnChangeTool;
        }

        private void Module_OnChangeTool()
        {
            InitTabControl();
        }

        int m_minTabControl = 0;
        void InitTabControl()
        {
            while (tabControlTools.Items.Count > m_minTabControl) tabControlTools.Items.RemoveAt(m_minTabControl);
            foreach (IAxis axis in m_module.m_listAxis)
            {
                if (axis != null)
                {
                    TabItem tabItem = new TabItem();
                    tabItem.Header = axis.p_sID.Replace(m_module.p_id + ".", "");
                    tabItem.Content = axis.p_ui;
                    tabControlTools.Items.Add(tabItem);
                }
            }
            foreach (ITool tool in m_module.m_aTool)
            {
                if (tool != null)
                {
                    TabItem tabItem = new TabItem();
                    tabItem.Header = tool.p_id.Replace(m_module.p_id + ".", "");
                    tabItem.Content = tool.p_ui;
                    tabItem.Background = Brushes.DarkSalmon; 
                    tabControlTools.Items.Add(tabItem);
                }
            }
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            m_module.ButtonRun();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_bPause = !EQ.p_bPause;
        }

        private void CheckBoxSetup_Click(object sender, RoutedEventArgs e)
        {
            treeRootUI.Visibility = (checkBoxSetup.IsChecked == false) ? Visibility.Visible : Visibility.Hidden;
            treeToolUI.Visibility = (checkBoxSetup.IsChecked == true) ? Visibility.Visible : Visibility.Hidden;
            m_module.RunToolTree(Tree.eMode.Init);
        }

        private void ButtonHome_Click(object sender, RoutedEventArgs e)
        {
            m_module.ButtonHome();
        }
    }
}
