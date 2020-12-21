using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Root_WIND2
{
    /// <summary>
    /// ColorPicker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            InitializeComponent();
            cbxColors.ItemsSource = typeof(Colors).GetProperties();
            cbxColors.SelectedIndex = 0;
            
        }
        private void cbxColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxColors.DataContext == null)
            {
                cbxColors.SelectedIndex = 0;
            }
            if (cbxColors.DataContext != null)
            {
                InspectionMask mask = (InspectionMask)cbxColors.DataContext;
                var selected = cbxColors.SelectedItem as PropertyInfo;
                var selectColor = selected.GetValue(selected);
                mask.p_Color = (Color)selectColor;
                
            }

        }

        private void Picker_Loaded(object sender, RoutedEventArgs e)
        {
            var ff = cbxColors.SelectedItem;
            InspectionMask roi = this.DataContext as InspectionMask;
            var select = typeof(Colors).GetProperties()
                        .Where(p => p.GetValue(null, null).ToString() == roi.p_Color.ToString())
                        .Select(p => p).FirstOrDefault();
            cbxColors.SelectedItem = select;
        }

    }
}
