using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Run.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Run : UserControl
    {
        public Run()
        {
            InitializeComponent();
        }
       
    }
    public class ModuleStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RootTools.Module.ModuleBase.eState eState = (RootTools.Module.ModuleBase.eState)value;
            switch (eState)
            {
                //case RootTools.Module.ModuleBase.eState.Init: return Brushes.DimGray;
                //case RootTools.Module.ModuleBase.eState.Home: return Brushes.DimGray;
                //case RootTools.Module.ModuleBase.eState.Ready: return Brushes.LightYellow;
                case RootTools.Module.ModuleBase.eState.Run:
                    return Brushes.PaleGreen;
                case RootTools.Module.ModuleBase.eState.Error: return Brushes.Red;
                default: return SystemColors.ControlColorKey;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
