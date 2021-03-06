using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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

namespace Root_ADI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region TitleBar
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            NormalizeButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Collapsed;
        }

        private void NormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            MaximizeButton.Visibility = Visibility.Visible;
            NormalizeButton.Visibility = Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    MaximizeButton.Visibility = Visibility.Visible;
                    NormalizeButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    NormalizeButton.Visibility = Visibility.Visible;
                    MaximizeButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.DragMove();
            }
        }

        #endregion

        bool m_blogin = false;
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!m_blogin)
            {
                tbUser.Text = "Engineer";
                m_blogin = true;
                LogArea.Height = new GridLength(300);
                LogSplitter.Height = new GridLength(3);
                LogSplitter.IsEnabled = m_blogin;
                TabMaint.IsEnabled = m_blogin;
                TabMaint.Opacity = 1;
            }
            else
            {
                m_blogin = false;
                tbUser.Text = "User";
                LogArea.Height = new GridLength(0);
                LogSplitter.Height = new GridLength(0);
                LogSplitter.IsEnabled = m_blogin;
                TabMaint.IsEnabled = m_blogin;
                TabMaint.Opacity = 0.5;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ReviewWindow rw = new ReviewWindow();
            rw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            rw.Show();
        }
    }
    public class Dummy
    {
        public string no
        {
            get;set;
        }
        public string type
        {
            get;set;
        }
        public string x
        {
            get;set;
        }
        public string y
        {
            get; set;
        }
        public string index
        {
            get;set;
        }
        public string loac
        {
            get; set;
        }
    }
}
