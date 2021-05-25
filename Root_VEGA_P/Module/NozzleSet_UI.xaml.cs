using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Root_VEGA_P.Module
{
    /// <summary>
    /// NozzleSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NozzleSet_UI : UserControl
    {
        public NozzleSet_UI()
        {
            InitializeComponent();
        }

        NozzleSet m_nozzleSet;
        List<CheckBox> m_aCheckBox = new List<CheckBox>(); 
        public void Init(NozzleSet nozzleSet)
        {
            m_nozzleSet = nozzleSet;
            DataContext = nozzleSet;
            textBlockHeader.Text = nozzleSet.m_sExt; 
            for (int n = 1; n <= nozzleSet.p_nNozzle; n++)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Margin = new Thickness(3, 3, 3, 3);
                checkBox.Content = n.ToString("Nozzel 00");
                stackPanelNozzle.Children.Add(checkBox);
                m_aCheckBox.Add(checkBox); 
            }
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            m_nozzleSet.FileOpen(); 
            for (int n = 0; n < m_nozzleSet.p_nNozzle; n++)
            {
                m_aCheckBox[n].IsChecked = m_nozzleSet.m_aOpen[n]; 
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            for (int n = 0; n < m_nozzleSet.p_nNozzle; n++)
            {
                m_nozzleSet.m_aOpen[n] = (m_aCheckBox[n].IsChecked == true);
            }
            m_nozzleSet.FileSave(); 
        }
    }
}
