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
using System.Windows.Shapes;

namespace Root_CAMELLIA
{
    /// <summary>
    /// Dlg_SplashScreen.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Dlg_SplashScreen : Window
    {
        Dlg_SplashScreen_ViewModel m_splashViewModel = new Dlg_SplashScreen_ViewModel();
        public Dlg_SplashScreen()
        {
            InitializeComponent();
            DataContext = m_splashViewModel;
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.RightButton != MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
