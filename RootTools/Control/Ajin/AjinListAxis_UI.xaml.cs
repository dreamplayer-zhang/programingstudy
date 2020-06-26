using RootTools.Trees;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RootTools.Control.Ajin
{
    /// <summary>
    /// AjinListAxis_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AjinListAxis_UI : UserControl
    {
        public AjinListAxis_UI()
        {
            InitializeComponent();
        }

        AjinListAxis m_listAxis; 
        public void Init(AjinListAxis listAxis)
        {
            m_listAxis = listAxis;
            this.DataContext = listAxis;
            InitTabControl();
            listAxis.OnChangeAxisList += ListAxis_OnChangeAxisList;
        }

        private void ListAxis_OnChangeAxisList()
        {
            InitTabControl();
            comboAxis.SelectedIndex = 0; 
        }

        void InitTabControl()
        {
            tabControlAxis.Items.Clear();
            comboAxis.ItemsSource = null;
            m_asAxis.Clear(); 
            foreach (AjinAxis axis in m_listAxis.m_aAxis)
            {
                TabItem tabItem = new TabItem();
                Binding bindingHeader = new Binding("p_sName");
                bindingHeader.Source = axis;
                tabItem.SetBinding(TabItem.HeaderProperty, bindingHeader); 
                tabItem.Content = axis.p_ui;
                m_asAxis.Add(axis.p_id); 
                tabItem.Height = 0;
                tabControlAxis.Items.Add(tabItem);
                axis.RunTree(Tree.eMode.Init);
                axis.RunTreeSetting(Tree.eMode.Init); 
            }
            comboAxis.ItemsSource = m_asAxis; 
        }

        private void ButtonOpenMot_Click(object sender, RoutedEventArgs e)
        {
            m_listAxis.LoadMot(); 
        }

        private void ButtonSaveMot_Click(object sender, RoutedEventArgs e)
        {
            m_listAxis.SaveMot(); 
        }

        List<string> m_asAxis = new List<string>();
        private void comboAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboAxis.SelectedIndex < 0) return;
            tabControlAxis.SelectedIndex = comboAxis.SelectedIndex; 
        }
    }
}
