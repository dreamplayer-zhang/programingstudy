using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

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
        }

        void InitTabControl()
        {
            if (m_listAxis.m_aAxis.Count == tabControlAxis.Items.Count) return;
            tabControlAxis.Items.Clear();
            foreach (AjinAxis axis in m_listAxis.m_aAxis)
            {
                TabItem tabItem = new TabItem();
                Binding bindingHeader = new Binding("p_sID");
                bindingHeader.Source = axis;
                tabItem.SetBinding(TabItem.HeaderProperty, bindingHeader); 
                tabItem.Content = axis.p_ui;
                tabControlAxis.Items.Add(tabItem);
            }
        }

        private void ButtonOpenMot_Click(object sender, RoutedEventArgs e)
        {
            m_listAxis.LoadMot(); 
        }

        private void ButtonSaveMot_Click(object sender, RoutedEventArgs e)
        {
            m_listAxis.SaveMot(); 
        }
    }
}
