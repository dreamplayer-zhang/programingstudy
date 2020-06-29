using RootTools.Control;
using RootTools.Trees;
using System.Collections.Generic;
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
        List<string> m_asAxis = new List<string>(); 
        void InitTabControl()
        {
            while (tabControlTools.Items.Count > m_minTabControl) tabControlTools.Items.RemoveAt(m_minTabControl);
            tabAxis.Items.Clear();
            m_asAxis.Clear(); 
            comboAxis.ItemsSource = null; 
            foreach (Axis axis in m_module.m_listAxis)
            {
                if (axis != null)
                {
                    TabItem tabItem = new TabItem();
                    tabItem.Header = axis.p_id.Replace(m_module.p_id + ".", "");
                    tabItem.Height = 0; 
                    tabItem.Content = axis.p_ui;
                    tabItem.Background = Brushes.DarkSalmon;
                    tabAxis.Items.Add(tabItem);
                    m_asAxis.Add(axis.p_id.Replace(m_module.p_id + ".", "")); 
                }
            }
            comboAxis.ItemsSource = m_asAxis;
            comboAxis.SelectedIndex = 0; 
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

        private void comboAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboAxis.SelectedIndex < 0) return;
            tabAxis.SelectedIndex = comboAxis.SelectedIndex;
        }
    }
}
