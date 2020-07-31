using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RootTools.Control.ACS
{
    /// <summary>
    /// ACSListAxis_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ACSListAxis_UI : UserControl
    {
        public ACSListAxis_UI()
        {
            InitializeComponent();
        }

        ACSListAxis m_listAxis;
        public void Init(ACSListAxis listAxis)
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
            foreach (ACSAxis axis in m_listAxis.m_aAxis)
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
