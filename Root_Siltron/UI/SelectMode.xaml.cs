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

namespace Root_Siltron
{
    /// <summary>
    /// SelectModeUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectMode : UserControl
    {
        MainWindow m_Mainwindow;
        public SelectMode()
        {
            InitializeComponent();
        }
        public void Init(MainWindow main = null)
        {
            m_Mainwindow = main;
        }
        private void GroupBox_MouseEnter(object sender, MouseEventArgs e)
        {
            ((GroupBox)sender).Background = Brushes.AliceBlue;
        }
        private void GroupBox_MouseLeave(object sender, MouseEventArgs e)
        {
            ((GroupBox)sender).Background = Brushes.Gainsboro;
        }
        private void SetupGroupBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_Mainwindow.MainPanel.Children.Clear();
            m_Mainwindow.MainPanel.Children.Add(m_Mainwindow.m_SetupUI);
        }
    }
}
