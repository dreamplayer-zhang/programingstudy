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

namespace Root_AOP01_Packing
{
    /// <summary>
    /// SelectMode.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectMode : UserControl
    {
        MainWindow m_MainWindow;
        public SelectMode()
        {
            InitializeComponent();
        }
        public void Init(MainWindow main)
        {
            m_MainWindow = main;
        }
        private void GroupBox_MouseEnter(object sender, MouseEventArgs e)
        {
            ((GroupBox)sender).Background = Brushes.AliceBlue;
        }
        private void GroupBox_MouseLeave(object sender, MouseEventArgs e)
        {
            ((GroupBox)sender).Background = Brushes.Gainsboro;
        }
        private void Setup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_MainWindow.MainPanel.Children.Clear();
            m_MainWindow.MainPanel.Children.Add(m_MainWindow.Setup);
        }

        private void Review_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_MainWindow.MainPanel.Children.Clear();
            m_MainWindow.MainPanel.Children.Add(m_MainWindow.Review);
        }

        private void Run_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_MainWindow.MainPanel.Children.Clear();
            m_MainWindow.MainPanel.Children.Add(m_MainWindow.Run);
        }
    }
}
