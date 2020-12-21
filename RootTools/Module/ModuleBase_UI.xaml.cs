using RootTools.Control;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

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

            treeRootToolUI.Init(module.m_treeRootTool);
            module.RunTreeTool(Tree.eMode.Init);
            treeRootSetupUI.Init(module.m_treeRootSetup);
            module.RunTree(Tree.eMode.Init);
            treeRootRunUI.Init(module.m_treeRootRun);
            module.RunTreeRun(Tree.eMode.Init);
            treeRootQueueUI.Init(module.m_treeRootQueue);
            module.RunTreeQueue(Tree.eMode.Init);

            m_minTabControl = tabControlTools.Items.Count;
            InitTabControl();
            module.OnChangeTool += Module_OnChangeTool;

            InitTimer(); 
        }

        #region Tool Tab
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
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(300);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_module.RunTreeQueue(Tree.eMode.Init);
        }
        #endregion

        #region UI Function
        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            m_module.ButtonRun();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_bPause = !EQ.p_bPause;
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
        #endregion
    }
}
