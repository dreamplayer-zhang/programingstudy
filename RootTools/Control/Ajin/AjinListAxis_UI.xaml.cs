using RootTools.Trees;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
                if (IsNewAxis(axis))
                {
                    TabItem tabItem = new TabItem();
                    tabItem.Header = axis.p_id;
                    tabItem.Content = axis.p_ui;
                    m_asAxis.Add(axis.p_id);
                    tabItem.Height = 0;
                    tabControlAxis.Items.Add(tabItem);
                    axis.RunTree(Tree.eMode.Init);
                    axis.RunTreeSetting(Tree.eMode.Init);
                    axis.RunTreeInterlock(Tree.eMode.Init);
                }
            }
            comboAxis.ItemsSource = m_asAxis; 
        }

        bool IsNewAxis(AjinAxis axis)
        {
            foreach (TabItem tabItem in tabControlAxis.Items)
            {
                if ((string)tabItem.Header == axis.p_id) return false;
            }
            return true;
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
