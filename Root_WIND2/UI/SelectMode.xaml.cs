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

namespace Root_WIND2
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
        public void Init()
        {
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
            UIManager.Instance.ChangUISetup();
        }

        private void Review_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIManager.Instance.ChangUIReview();
        }

        private void Run_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIManager.Instance.ChangUIRun();
        }

        private void ButtonRecipeWizard_Clicked(object sender, RoutedEventArgs e)
        {
            UIManager.Instance.ChangUISetup2();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Tune tune = new Tune();
            tune.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            tune.ShowDialog();
        }
    }
}
