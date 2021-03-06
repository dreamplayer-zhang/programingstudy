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

namespace Root_CAMELLIA
{
    /// <summary>
    /// PMCheckReview.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PMCheckReview : UserControl
    {
        public PMCheckReview()
        {
            InitializeComponent();
        }
        private void btn_ReflectanceRepeatability_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new PM_Reflectance_ViewModel();
        }

        
        private void btn_ThicknessRepeatability_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new PM_Thickness_ViewModel();

        }

        private void btn_SensorHoleOffset_Click(object sender, RoutedEventArgs e)
        {
            DataContext = new PM_SensorHoleOffset_ViewModel();
        }

    }
}
