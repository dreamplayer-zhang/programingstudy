using RootTools.Trees;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Control.Xenax
{
    /// <summary>
    /// XenaxListAxis_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class XenaxListAxis_UI : UserControl
    {
        public XenaxListAxis_UI()
        {
            InitializeComponent();
        }

        XenaxListAxis m_listAxis;
        public void Init(XenaxListAxis listAxis)
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
            foreach (XenaxAxis axis in m_listAxis.m_aAxis)
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
            comboAxis.ItemsSource = m_asAxis;
        }

        List<string> m_asAxis = new List<string>();
        private void comboAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboAxis.SelectedIndex < 0) return;
            tabControlAxis.SelectedIndex = comboAxis.SelectedIndex;
        }
    }
}
