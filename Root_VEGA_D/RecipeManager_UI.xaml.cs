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

namespace Root_VEGA_D
{
    /// <summary>
    /// RecipeManager_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecipeManager_UI : UserControl
    {
        public RecipeManager_UI()
        {
            InitializeComponent();
        }

        private void DateTimePicker_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            int a = 10;
        }

        private void DateTimePicker_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            int t = 10;
        }

        private void DateTimePicker_DateChanged(object sender, RoutedEventArgs e)
        {
            int a = 10;
        }
    }
}
